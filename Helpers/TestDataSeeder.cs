// Bogus is a .NET library for generating fake data (great for seeding test data)
using Bogus;

// Npgsql is the ADO.NET provider for PostgreSQL
using Npgsql;

// Respawn resets database state between tests by truncating tables
using Respawn;


public static class TestDataSeeder
{
    /// <summary>
    /// Resets the PostgreSQL database using Respawn and seeds it with fake data.
    /// </summary>
    /// <param name="context">EF Core DbContext</param>
    /// <param name="connectionString">Connection string to PostgreSQL DB</param>
    public static async Task ResetAndSeedAsync(MyDbContext context, string connectionString)
    {
        // Open a raw Npgsql connection to the PostgreSQL database
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        // Create a Respawner instance configured for PostgreSQL,
        // specifying only the "public" schema should be reset
        var respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,      // Tell Respawn we are using PostgreSQL
            SchemasToInclude = new[] { "public" } // Only reset the "public" schema
        });

        // Reset the database (truncate all tables in the specified schema)
        await respawner.ResetAsync(connection);

        // Create a Faker to generate test data for the SampleEntity
        var faker = new Faker<SampleEntity>()
            .RuleFor(e => e.Name, f => f.Name.FullName()) // Generate a fake full name
            .RuleFor(e => e.CreatedAt, f => f.Date.Recent().ToUniversalTime()); // Recent date in UTC

        // Generate and add 5 fake SampleEntity records to the in-memory context
        context.SampleEntities.AddRange(faker.Generate(5));

        // Persist the fake data to the database
        await context.SaveChangesAsync();
    }
}
