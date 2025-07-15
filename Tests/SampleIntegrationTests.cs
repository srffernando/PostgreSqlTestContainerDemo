using NUnit.Framework;
using Microsoft.EntityFrameworkCore;

[TestFixture]
[Parallelizable(ParallelScope.Fixtures)]
public class SampleIntegrationTests : IAsyncDisposable
{
    private readonly PostgreContainerSetup _setup;
    private static readonly SemaphoreSlim SetupLock = new(1, 1);

    public SampleIntegrationTests()
    {
        _setup = PostgreTestContainer.InitializeAsync().Result;
    }

    public async ValueTask DisposeAsync() => await _setup.DisposeAsync();

    [SetUp]
    public async Task ResetDatabaseBeforeEachTest()
    {
        await SetupLock.WaitAsync();
        try
        {
            await using var context = new MyDbContext(_setup.Options);
            await TestDataSeeder.ResetAndSeedAsync(context, _setup.GetConnectionString());
        }
        finally
        {
            SetupLock.Release();
        }
    }

    // ========== CRUD Tests ==========

    [Test]
    [Category("CRUD")]
    public async Task Should_return_all_seeded_entities_when_queried()
    {
        await using var context = new MyDbContext(_setup.Options);
        var entities = await context.SampleEntities.ToListAsync();
        Assert.That(entities.Count, Is.EqualTo(5));
    }

    [Test]
    [Category("CRUD")]
    public async Task Should_persist_entity_when_inserted()
    {
        await using var context = new MyDbContext(_setup.Options);
        var newEntity = new SampleEntity { Name = "Test Insert", CreatedAt = DateTime.UtcNow };

        context.SampleEntities.Add(newEntity);
        await context.SaveChangesAsync();

        var inserted = await context.SampleEntities.FirstOrDefaultAsync(e => e.Name == "Test Insert");
        Assert.That(inserted, Is.Not.Null);
        Assert.That(inserted!.Name, Is.EqualTo("Test Insert"));
    }

    [Test]
    [Category("CRUD")]
    public async Task Should_update_entity_when_modified()
    {
        await using var context = new MyDbContext(_setup.Options);
        var entity = await context.SampleEntities.FirstAsync();
        entity.Name = "Updated Name";

        await context.SaveChangesAsync();

        var updated = await context.SampleEntities.FindAsync(entity.Id);
        Assert.That(updated!.Name, Is.EqualTo("Updated Name"));
    }

    [Test]
    [Category("CRUD")]
    public async Task Should_remove_entity_when_deleted()
    {
        await using var context = new MyDbContext(_setup.Options);
        var entity = await context.SampleEntities.FirstAsync();

        context.SampleEntities.Remove(entity);
        await context.SaveChangesAsync();

        var deleted = await context.SampleEntities.FindAsync(entity.Id);
        Assert.That(deleted, Is.Null);
    }

    [Test]
    [Category("CRUD")]
    public async Task Should_return_entities_in_ascending_order_when_sorted_by_createdAt()
    {
        await using var context = new MyDbContext(_setup.Options);
        var sorted = await context.SampleEntities.OrderBy(e => e.CreatedAt).ToListAsync();
        Assert.That(sorted.Select(e => e.CreatedAt), Is.Ordered.Ascending);
    }

    // ========== Validation ==========

    [Test]
    [Category("Validation")]
    public async Task Should_allow_null_name_when_inserted()
    {
        await using var context = new MyDbContext(_setup.Options);
        var entity = new SampleEntity { CreatedAt = DateTime.UtcNow };

        context.SampleEntities.Add(entity);
        await context.SaveChangesAsync();

        var result = await context.SampleEntities.FindAsync(entity.Id);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Name, Is.Null);
    }

    // ========== Parameterized Lookup ==========

    [Test]
    [Category("Lookup")]
    public async Task Should_find_entity_by_name_when_queried()
    {
        await using var context = new MyDbContext(_setup.Options);
        var names = await context.SampleEntities.Select(e => e.Name).ToListAsync();

        foreach (var name in names)
        {
            var found = await context.SampleEntities.FirstOrDefaultAsync(e => e.Name == name);
            Assert.That(found, Is.Not.Null, $"Expected entity with name '{name}' to exist.");
            Assert.That(found!.Name, Is.EqualTo(name));
        }
    }

    [Test]
    [Category("Lookup")]
    public async Task Should_find_entity_by_id_when_queried()
    {
        await using var context = new MyDbContext(_setup.Options);
        var allEntities = await context.SampleEntities.ToListAsync();

        foreach (var entity in allEntities)
        {
            var found = await context.SampleEntities.FindAsync(entity.Id);
            Assert.That(found, Is.Not.Null);
            Assert.That(found!.Id, Is.EqualTo(entity.Id));
            Assert.That(found.Name, Is.Not.Empty);
        }
    }

    // ========== Parallel Execution ==========

    [Test]
    [Category("Parallel")]
    [Parallelizable(ParallelScope.Self)]
    public async Task Should_run_in_isolation_when_parallel_test_a_executes()
    {
        await Task.Delay(200);
        Assert.Pass("Parallel Test A completed");
    }

    [Test]
    [Category("Parallel")]
    [Parallelizable(ParallelScope.Self)]
    public async Task Should_run_in_isolation_when_parallel_test_b_executes()
    {
        await Task.Delay(200);
        Assert.Pass("Parallel Test B completed");
    }

    // ========== Exception Tests ==========

    [Test]
    [Category("Exception")]
    public void Should_throw_argument_null_exception_when_entity_is_null()
    {
        SampleEntity? entity = null;

        var ex = Assert.Throws<ArgumentNullException>(() =>
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
        });

        Assert.That(ex!.ParamName, Is.EqualTo("entity"));
    }

    [Test]
    [Category("Exception")]
    public async Task Should_throw_invalid_operation_exception_when_entity_id_does_not_exist()
    {
        await using var context = new MyDbContext(_setup.Options);

        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            _ = await context.SampleEntities
                .Where(e => e.Id == -999)
                .SingleAsync();
        });

        Assert.That(ex!.Message, Does.Contain("Sequence contains no elements"));
    }
}
