using Dapper;

namespace Persistence;

public class DapperDatabase
{
    public static void Initialize(DapperConnectionFactory factory)
    {
        using var db = factory.Create();
        db.Open();

        db.Execute(CreateUsersTable);
        db.Execute(CreateRecipesTable);
        db.Execute(CreateRecipeRatingsTable);
        db.Execute(CreateIngredientsTable);
        db.Execute(CreateCommentsTable);
        db.Execute(CreateRatingTriggerFunction);
        db.Execute(CreateRatingTrigger);
        db.Execute(CreateRateFunction);
    }

    #region Sql Queries Definition

    private const string CreateUsersTable = """
                                            CREATE TABLE IF NOT EXISTS Users (
                                              Id SERIAL PRIMARY KEY,
                                              Username TEXT NOT NULL,
                                              Password TEXT NOT NULL,
                                              Role TEXT NOT NULL,
                                              CONSTRAINT Username_Unique UNIQUE(Username)
                                            );
                                            """;
    
    private const string CreateRecipesTable = """
                                              CREATE TABLE IF NOT EXISTS Recipes (
                                                  Id SERIAL PRIMARY KEY,
                                                  Author_Id INT NOT NULL REFERENCES Users(Id) ON DELETE CASCADE,
                                                  Title TEXT NOT NULL,
                                                  Description TEXT NOT NULL,
                                                  Instruction TEXT NOT NULL,
                                                  Image_Name TEXT NOT NULL,
                                                  Difficulty TEXT NOT NULL,
                                                  Published_At TIMESTAMPTZ NOT NULL,
                                                  Cooking_Time INTERVAL NOT NULL,
                                                  Rating NUMERIC(2, 1) NOT NULL,
                                                  Votes INT NOT NULL
                                              );
                                              """;

    private const string CreateRecipeRatingsTable = """
                                                    CREATE TABLE IF NOT EXISTS Recipe_Ratings (
                                                        Recipe_Id INT NOT NULL REFERENCES Recipes(Id) ON DELETE CASCADE,
                                                        User_Id INT NULL REFERENCES Users(Id) ON DELETE SET NULL,
                                                        Rate INT NOT NULL,
                                                        CONSTRAINT Rate_Data UNIQUE(Recipe_Id, User_Id)
                                                    );
                                                    """;

    private const string CreateIngredientsTable = """
                                                  CREATE TABLE IF NOT EXISTS Ingredients (
                                                      Id BIGSERIAL PRIMARY KEY,
                                                      Recipe_Id INT NOT NULL REFERENCES Recipes(Id) ON DELETE CASCADE,
                                                      Name TEXT NOT NULL,
                                                      Count REAL NOT NULL,
                                                      Unit TEXT NOT NULL
                                                  );
                                                  """;
    
    private const string CreateCommentsTable = """
                                               CREATE TABLE IF NOT EXISTS Comments (
                                                   Id BIGSERIAL PRIMARY KEY,
                                                   Recipe_Id INT NOT NULL REFERENCES Recipes(Id) ON DELETE CASCADE,
                                                   User_Id INT NULL REFERENCES Users(Id) ON DELETE SET NULL,
                                                   Content TEXT NOT NULL,
                                                   Published_At TIMESTAMP WITH TIME ZONE NOT NULL
                                               );
                                               """;
    
    private const string CreateRatingTriggerFunction = """
                                                        CREATE OR REPLACE FUNCTION On_Recipe_Rated() RETURNS TRIGGER AS $$
                                                        BEGIN
                                                            IF TG_OP = 'INSERT' OR TG_OP = 'UPDATE' THEN
                                                                UPDATE Recipes SET 
                                                                    Votes = (SELECT COUNT(*) FROM Recipe_Ratings WHERE Recipe_Id = NEW.Recipe_Id),
                                                                    Rating = (SELECT AVG(Rate) FROM Recipe_Ratings WHERE Recipe_Id = NEW.Recipe_Id)
                                                                WHERE Id = NEW.Recipe_Id;
                                                                RETURN NEW;
                                                            ELSIF TG_OP = 'DELETE' THEN
                                                                UPDATE Recipes SET 
                                                                    Votes = (SELECT COUNT(*) FROM Recipe_Ratings WHERE Recipe_Id = OLD.Recipe_Id),
                                                                    Rating = (SELECT COALESCE(AVG(Rate), 0) FROM Recipe_Ratings WHERE Recipe_Id = OLD.Recipe_Id)
                                                                WHERE Id = OLD.Recipe_Id;
                                                                RETURN OLD;
                                                            END IF;
                                                            
                                                            RETURN NULL;
                                                        END;
                                                        $$ LANGUAGE plpgsql;
                                                        """;
    
    private const string CreateRatingTrigger = """
                                               CREATE OR REPLACE TRIGGER Recipe_Rated_Trigger AFTER INSERT OR UPDATE OR DELETE 
                                                   ON Recipe_Ratings
                                                   FOR EACH ROW
                                                   EXECUTE PROCEDURE On_Recipe_Rated();
                                               """;
    
    private const string CreateRateFunction = """
                                                CREATE OR REPLACE FUNCTION Rate_Recipe(_recipe_id INT, _user_id INT, _rate INT) RETURNS INT AS $$
                                                    DECLARE
                                                        result INT;
                                                    BEGIN
                                                        SELECT Rate INTO result FROM Recipe_Ratings WHERE Recipe_Id = _recipe_id AND User_Id = _user_id AND Rate = _rate;
                                                        IF FOUND THEN
                                                            DELETE FROM Recipe_Ratings WHERE Recipe_Id = _recipe_id AND User_Id = _user_id;
                                                            RETURN 0;
                                                        END IF; 
                                                            
                                                        INSERT INTO Recipe_Ratings (Recipe_Id, User_Id, Rate)
                                                        VALUES (_recipe_id, _user_id, _rate)
                                                        ON CONFLICT (Recipe_Id, User_Id) DO UPDATE SET Rate = EXCLUDED.Rate RETURNING Recipe_Ratings.Rate INTO result;
                                                        RETURN result;
                                                    END;
                                                $$ LANGUAGE plpgsql;
                                                """;

    #endregion
}