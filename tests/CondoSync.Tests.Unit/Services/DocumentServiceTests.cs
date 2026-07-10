using CondoSync.Application.Features.Documents.DTOs;
using CondoSync.Application.Services;
using CondoSync.Core.Entities;
using CondoSync.Core.Enums;
using CondoSync.Core.Interfaces;
using FluentAssertions;
using Moq;

namespace CondoSync.Tests.Unit.Services;

public class DocumentServiceTests
{
    private readonly Mock<IRepository<Document>> _repoMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<ITenantProvider> _tenantMock;
    private readonly Mock<IStorageService> _storageMock;
    private readonly DocumentService _service;

    public DocumentServiceTests()
    {
        _repoMock = new Mock<IRepository<Document>>();
        _uowMock = new Mock<IUnitOfWork>();
        _tenantMock = new Mock<ITenantProvider>();
        _storageMock = new Mock<IStorageService>();

        _tenantMock.Setup(t => t.GetCurrentTenantId()).Returns(Guid.NewGuid());

        _service = new DocumentService(
            _repoMock.Object, _uowMock.Object, _tenantMock.Object,
            _storageMock.Object, Mock.Of<Microsoft.Extensions.Logging.ILogger<DocumentService>>());
    }

    [Fact]
    public async Task GetDocumentsAsync_ShouldReturnFilteredByTenant()
    {
        var tenantId = Guid.NewGuid();
        _tenantMock.Setup(t => t.GetCurrentTenantId()).Returns(tenantId);

        var docs = new List<Document>
        {
            CreateDocument(tenantId, "Doc1"),
            CreateDocument(tenantId, "Doc2"),
            CreateDocument(Guid.NewGuid(), "Other"),
        };
        _repoMock.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Document, bool>>>(), default))
            .ReturnsAsync((System.Linq.Expressions.Expression<Func<Document, bool>> expr, CancellationToken _) =>
                docs.AsQueryable().Where(expr).ToList());

        var result = await _service.GetDocumentsAsync();

        result.Should().HaveCount(2);
        result.Select(d => d.Name).Should().BeEquivalentTo("Doc1", "Doc2");
    }

    [Fact]
    public async Task DeleteDocumentAsync_ExistingDoc_ShouldSoftDelete()
    {
        var tenantId = Guid.NewGuid();
        _tenantMock.Setup(t => t.GetCurrentTenantId()).Returns(tenantId);

        var doc = CreateDocument(tenantId, "Test");
        _repoMock.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Document, bool>>>(), default))
            .ReturnsAsync([doc]);

        var result = await _service.DeleteDocumentAsync(doc.Id);

        result.Should().BeTrue();
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task DeleteDocumentAsync_NonExisting_ShouldReturnFalse()
    {
        _repoMock.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Document, bool>>>(), default))
            .ReturnsAsync([]);

        var result = await _service.DeleteDocumentAsync(Guid.NewGuid());

        result.Should().BeFalse();
    }

    private static Document CreateDocument(Guid tenantId, string name)
    {
        return Document.Create(tenantId, Guid.NewGuid(), name,
            "file.pdf", "/bucket/file.pdf", "application/pdf", 100);
    }
}
