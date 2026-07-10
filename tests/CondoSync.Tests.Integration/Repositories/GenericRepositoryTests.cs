using CondoSync.Core.Entities;
using CondoSync.Core.Interfaces;
using CondoSync.Infrastructure.Data;
using CondoSync.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace CondoSync.Tests.Integration.Repositories;

public class GenericRepositoryTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("condosync_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private CondoSyncDbContext _context = null!;
    private IRepository<Document> _repository = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var options = new DbContextOptionsBuilder<CondoSyncDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .Options;

        _context = new CondoSyncDbContext(options);
        await _context.Database.EnsureCreatedAsync();

        _repository = new GenericRepository<Document>(_context);
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _container.DisposeAsync();
    }

    [Fact]
    public async Task AddAsync_ShouldPersistEntity()
    {
        var doc = Document.Create(
            Guid.NewGuid(), Guid.NewGuid(), "Test Document",
            "test.pdf", "/bucket/test.pdf", "application/pdf", 100);

        await _repository.AddAsync(doc);
        await _context.SaveChangesAsync();

        var retrieved = await _repository.GetByIdAsync(doc.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Test Document");
    }

    [Fact]
    public async Task FindAsync_ShouldReturnMatchingEntities()
    {
        var tenantId = Guid.NewGuid();
        var docs = new[]
        {
            Document.Create(tenantId, Guid.NewGuid(), "Doc1", "f1.pdf", "/b/f1.pdf", "application/pdf", 50),
            Document.Create(tenantId, Guid.NewGuid(), "Doc2", "f2.pdf", "/b/f2.pdf", "application/pdf", 75),
            Document.Create(Guid.NewGuid(), Guid.NewGuid(), "Other", "f3.pdf", "/b/f3.pdf", "application/pdf", 30),
        };

        foreach (var doc in docs)
            await _repository.AddAsync(doc);
        await _context.SaveChangesAsync();

        var result = await _repository.FindAsync(d => d.CondominiumId == tenantId);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Remove_ShouldDeleteEntity()
    {
        var doc = Document.Create(
            Guid.NewGuid(), Guid.NewGuid(), "To Delete",
            "del.pdf", "/bucket/del.pdf", "application/pdf", 10);

        await _repository.AddAsync(doc);
        await _context.SaveChangesAsync();

        _repository.Remove(doc);
        await _context.SaveChangesAsync();

        var retrieved = await _repository.GetByIdAsync(doc.Id);
        retrieved.Should().BeNull();
    }
}
