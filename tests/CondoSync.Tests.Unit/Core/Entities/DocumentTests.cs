using CondoSync.Core.Entities;
using CondoSync.Core.Enums;
using FluentAssertions;

namespace CondoSync.Tests.Unit.Core.Entities;

public class DocumentTests
{
    private static readonly Guid CondominiumId = Guid.NewGuid();
    private static readonly Guid UploadedBy = Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_ShouldSetProperties()
    {
        var doc = Document.Create(
            CondominiumId, UploadedBy, "Ata Reunião",
            "ata.pdf", "/bucket/ata.pdf", "application/pdf", 1024,
            DocumentType.Minutes, "Ata da reunião de condomínio");

        doc.Id.Should().NotBeEmpty();
        doc.Name.Should().Be("Ata Reunião");
        doc.Version.Should().Be(1);
        doc.IsActive.Should().BeTrue();
        doc.DeletedAt.Should().BeNull();
        doc.CondominiumId.Should().Be(CondominiumId);
    }

    [Fact]
    public void CreateNewVersion_ShouldIncrementVersionAndKeepPreviousReference()
    {
        var doc = Document.Create(CondominiumId, UploadedBy, "Regulamento",
            "v1.pdf", "/bucket/v1.pdf", "application/pdf", 512);

        doc.CreateNewVersion("v2.pdf", "/bucket/v2.pdf", "application/pdf", 768);

        doc.Version.Should().Be(2);
        doc.PreviousVersionId.Should().Be(doc.Id);
        doc.FileName.Should().Be("v2.pdf");
    }

    [Fact]
    public void SoftDelete_ShouldSetDeletedAtAndDeactivate()
    {
        var doc = Document.Create(CondominiumId, UploadedBy, "Contrato",
            "contrato.pdf", "/bucket/contrato.pdf", "application/pdf", 2048);

        doc.SoftDelete();

        doc.DeletedAt.Should().NotBeNull();
        doc.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Create_WithEmptyName_ShouldThrow()
    {
        var act = () => Document.Create(CondominiumId, UploadedBy, "",
            "file.pdf", "/bucket/file.pdf", "application/pdf", 100);

        act.Should().Throw<Exception>();
    }
}
