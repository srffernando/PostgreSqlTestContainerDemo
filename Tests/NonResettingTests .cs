using NUnit.Framework;
using Microsoft.EntityFrameworkCore;

[TestFixture]
[Category("NoReset")]
[Parallelizable(ParallelScope.Fixtures)]
public class SampleNonResettingTests : IAsyncDisposable
{
    private readonly PostgreContainerSetup _setup;

    public SampleNonResettingTests()
    {
        _setup = PostgreTestContainer.InitializeAsync().Result;
    }

    public async ValueTask DisposeAsync() => await _setup.DisposeAsync();

    [Test]
    [Order(1)]
    public async Task Should_insert_additional_entity_when_database_is_not_cleared()
    {
        await using var context = new MyDbContext(_setup.Options);
        var entity = new SampleEntity
        {
            Name = "AddedWithoutReset",
            CreatedAt = DateTime.UtcNow
        };

        context.SampleEntities.Add(entity);
        await context.SaveChangesAsync();

        var count = await context.SampleEntities.CountAsync();
        TestContext.WriteLine($"[InsertTest] Total entities in DB: {count}");

        Assert.That(count, Is.GreaterThan(0)); // Should keep increasing across tests
    }

    [Test]
    [Order(2)]
    public async Task Should_retrieve_all_entities_when_database_state_is_accumulated()
    {
        await using var context = new MyDbContext(_setup.Options);
        var allEntities = await context.SampleEntities.ToListAsync();

        TestContext.WriteLine($"[RetrieveTest] Total entities: {allEntities.Count}");

        Assert.That(allEntities.Count, Is.GreaterThan(0));
        Assert.That(allEntities.Any(e => e.Name == "AddedWithoutReset"), Is.True);
    }

    [Test]
    [Order(3)]
    public async Task Should_continue_growing_entity_count_when_database_is_persistent()
    {
        await using var context = new MyDbContext(_setup.Options);
        var entityCountBefore = await context.SampleEntities.CountAsync();

         context.SampleEntities.Add(new SampleEntity
         {
             Name = $"Entity#{entityCountBefore + 1}",
             CreatedAt = DateTime.UtcNow
         });

         await context.SaveChangesAsync();
         var entityCountAfter = await context.SampleEntities.CountAsync();

         TestContext.WriteLine($"[GrowthTest] Before: {entityCountBefore}, After: {entityCountAfter}");

         Assert.That(entityCountAfter, Is.EqualTo(entityCountBefore + 1));
       
    }
}
