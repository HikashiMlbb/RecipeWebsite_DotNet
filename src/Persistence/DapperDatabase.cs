using Dapper;

namespace Persistence;

public static class DapperDatabase
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
                                            CREATE TABLE IF NOT EXISTS "Users" (
                                              "Id" SERIAL PRIMARY KEY,
                                              "Username" TEXT NOT NULL,
                                              "Password" TEXT NOT NULL,
                                              "Role" TEXT NOT NULL,
                                              CONSTRAINT "UsernameUnique" UNIQUE("Username")
                                            );
                                            """;

    private const string CreateRecipesTable = """
                                              CREATE TABLE IF NOT EXISTS "Recipes" (
                                                  "Id" SERIAL PRIMARY KEY,
                                                  "AuthorId" INT NOT NULL REFERENCES "Users"("Id") ON DELETE CASCADE,
                                                  "Title" TEXT NOT NULL,
                                                  "Description" TEXT NOT NULL,
                                                  "Instruction" TEXT NOT NULL,
                                                  "ImageName" TEXT NOT NULL,
                                                  "Difficulty" TEXT NOT NULL,
                                                  "PublishedAt" TIMESTAMPTZ NOT NULL,
                                                  "CookingTime" INTERVAL NOT NULL,
                                                  "Rating" NUMERIC(2, 1) NOT NULL,
                                                  "Votes" INT NOT NULL
                                              );
                                              """;

    private const string CreateRecipeRatingsTable = """
                                                    CREATE TABLE IF NOT EXISTS "RecipeRatings" (
                                                        "RecipeId" INT NOT NULL REFERENCES "Recipes"("Id") ON DELETE CASCADE,
                                                        "UserId" INT NULL REFERENCES "Users"("Id") ON DELETE SET NULL,
                                                        "Rate" INT NOT NULL,
                                                        CONSTRAINT "RateData" UNIQUE("RecipeId", "UserId")
                                                    );
                                                    """;

    private const string CreateIngredientsTable = """
                                                  CREATE TABLE IF NOT EXISTS "Ingredients" (
                                                      "Id" BIGSERIAL PRIMARY KEY,
                                                      "RecipeId" INT NOT NULL REFERENCES "Recipes"("Id") ON DELETE CASCADE,
                                                      "Name" TEXT NOT NULL,
                                                      "Count" DECIMAL NOT NULL,
                                                      "Unit" TEXT NOT NULL
                                                  );
                                                  """;

    private const string CreateCommentsTable = """
                                               CREATE TABLE IF NOT EXISTS "Comments" (
                                                   "Id" BIGSERIAL PRIMARY KEY,
                                                   "RecipeId" INT NOT NULL REFERENCES "Recipes"("Id") ON DELETE CASCADE,
                                                   "UserId" INT NULL REFERENCES "Users"("Id") ON DELETE SET NULL,
                                                   "Content" TEXT NOT NULL,
                                                   "PublishedAt" TIMESTAMP WITH TIME ZONE NOT NULL
                                               );
                                               """;

    private const string CreateRatingTriggerFunction = """
                                                       CREATE OR REPLACE FUNCTION "OnRecipeRated"() RETURNS TRIGGER AS $$
                                                       BEGIN
                                                           IF TG_OP = 'INSERT' OR TG_OP = 'UPDATE' THEN
                                                               UPDATE "Recipes" SET 
                                                                   "Votes" = (SELECT COUNT(*) FROM "RecipeRatings" WHERE "RecipeId" = NEW."RecipeId"),
                                                                   "Rating" = (SELECT AVG("Rate") FROM "RecipeRatings" WHERE "RecipeId" = NEW."RecipeId")
                                                               WHERE "Id" = NEW."RecipeId";
                                                               RETURN NEW;
                                                           ELSIF TG_OP = 'DELETE' THEN
                                                               UPDATE "Recipes" SET 
                                                                   "Votes" = (SELECT COUNT(*) FROM "RecipeRatings" WHERE "RecipeId" = OLD."RecipeId"),
                                                                   "Rating" = (SELECT AVG("Rate") FROM "RecipeRatings" WHERE "RecipeId" = OLD."RecipeId")
                                                               WHERE "Id" = OLD."RecipeId";
                                                               RETURN OLD;
                                                           END IF;
                                                           RETURN NULL;
                                                       END;
                                                       $$ LANGUAGE plpgsql;
                                                       """;

    private const string CreateRatingTrigger = """
                                               CREATE OR REPLACE TRIGGER "RecipeRatedTrigger" AFTER INSERT OR UPDATE OR DELETE 
                                                   ON "RecipeRatings"
                                                   FOR EACH ROW
                                                   EXECUTE PROCEDURE "OnRecipeRated"();
                                               """;

    private const string CreateRateFunction = """
                                              CREATE OR REPLACE FUNCTION "RateRecipe"(_recipe_id INT, _user_id INT, _rate INT) RETURNS INT AS $$
                                                  DECLARE
                                                      result INT;
                                                  BEGIN
                                                      SELECT "Rate" INTO result FROM "RecipeRatings" WHERE "RecipeId" = _recipe_id AND "UserId" = _user_id AND "Rate" = _rate;
                                                      IF FOUND THEN
                                                          DELETE FROM "RecipeRatings" WHERE "RecipeId" = _recipe_id AND "UserId" = _user_id;
                                                          RETURN 0;
                                                      END IF; 
                                                          
                                                      INSERT INTO "RecipeRatings" ("RecipeId", "UserId", "Rate")
                                                      VALUES (_recipe_id, _user_id, _rate)
                                                      ON CONFLICT ("RecipeId", "UserId") DO UPDATE SET "Rate" = EXCLUDED."Rate" RETURNING "RecipeRatings"."Rate" INTO result;
                                                      RETURN result;
                                                  END;
                                              $$ LANGUAGE plpgsql;
                                              """;

    #endregion
}