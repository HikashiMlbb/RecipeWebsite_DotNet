using Dapper;

namespace Persistence;

public class DapperDatabase
{
    public static void Initialize(DapperConnectionFactory factory)
    {
        using var db = factory.Create();

        db.Execute(CreateUsersTable);
        db.Execute(CreateRecipesTable);
        db.Execute(CreateRecipeRatingsTable);
        db.Execute(CreateIngredientsTable);
        db.Execute(CreateCommentsTable);
        db.Execute(CreateRatingTriggerFunction);
        db.Execute(CreateRatingTrigger);
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
                                                      Recipe_Id INT NOT NULL REFERENCES Recipes(Id) ON DELETE CASCADE,
                                                      Name TEXT NOT NULL,
                                                      Count REAL NOT NULL,
                                                      Unit TEXT NOT NULL
                                                  );
                                                  """;
    
    private const string CreateCommentsTable = """
                                               CREATE TABLE IF NOT EXISTS Comments (
                                                   Recipe_Id INT NOT NULL REFERENCES Recipes(Id) ON DELETE CASCADE,
                                                   User_Id INT NULL REFERENCES Users(Id) ON DELETE SET NULL,
                                                   Content TEXT NOT NULL,
                                                   Published_At TIMESTAMP WITH TIME ZONE NOT NULL
                                               );
                                               """;
    
    private const string CreateRatingTriggerFunction = """
                                                        CREATE OR REPLACE FUNCTION On_Recipe_Rated() RETURNS TRIGGER AS $$
                                                            BEGIN
                                                                UPDATE Recipes SET 
                                                                    Votes = (SELECT COUNT(*) FROM Recipe_Ratings WHERE Recipe_Id = NEW.Recipe_Id),
                                                                    Rating = (SELECT AVG(Rate) FROM Recipe_Ratings WHERE Recipe_Id = NEW.Recipe_Id)
                                                                WHERE Id = NEW.Recipe_Id;
                                                                RETURN NEW;
                                                            END;
                                                        $$ LANGUAGE plpgsql;
                                                        """;
    
    private const string CreateRatingTrigger = """
                                               CREATE OR REPLACE TRIGGER Recipe_Rated_Trigger AFTER INSERT OR UPDATE OR DELETE 
                                                   ON Recipe_Ratings
                                                   FOR EACH ROW
                                                   EXECUTE PROCEDURE On_Recipe_Rated();
                                               """;

    #endregion
}