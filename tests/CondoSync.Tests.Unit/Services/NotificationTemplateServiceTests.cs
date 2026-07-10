using CondoSync.Application.Features.Notifications.DTOs;
using CondoSync.Application.Services;
using CondoSync.Core.Entities;
using CondoSync.Core.Interfaces;
using FluentAssertions;
using Moq;

namespace CondoSync.Tests.Unit.Services;

public class NotificationTemplateServiceTests
{
    private readonly Mock<IRepository<NotificationTemplate>> _repoMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<ITenantProvider> _tenantMock;
    private readonly NotificationTemplateService _service;

    public NotificationTemplateServiceTests()
    {
        _repoMock = new Mock<IRepository<NotificationTemplate>>();
        _uowMock = new Mock<IUnitOfWork>();
        _tenantMock = new Mock<ITenantProvider>();

        _tenantMock.Setup(t => t.GetCurrentTenantId()).Returns(Guid.NewGuid());

        _service = new NotificationTemplateService(
            _repoMock.Object, _uowMock.Object, _tenantMock.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<NotificationTemplateService>>());
    }

    [Fact]
    public async Task CreateAsync_ShouldAddAndReturnTemplate()
    {
        var request = new CreateNotificationTemplateRequest(
            "ReservaConfirmada",
            "Reserva #{ref} Confirmada",
            "Sua reserva foi confirmada.",
            "Booking");

        _repoMock.Setup(r => r.AddAsync(It.IsAny<NotificationTemplate>(), default))
            .ReturnsAsync((NotificationTemplate t, CancellationToken _) => t);

        var result = await _service.CreateAsync(request);

        result.Name.Should().Be("ReservaConfirmada");
        result.Channel.Should().Be("in_app");
        result.IsActive.Should().BeTrue();
        _repoMock.Verify(r => r.AddAsync(It.IsAny<NotificationTemplate>(), default), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnOnlyTenantTemplates()
    {
        var tenantId = Guid.NewGuid();
        _tenantMock.Setup(t => t.GetCurrentTenantId()).Returns(tenantId);

        var templates = new List<NotificationTemplate>
        {
            CreateTemplate(tenantId, "T1"),
            CreateTemplate(tenantId, "T2"),
            CreateTemplate(Guid.NewGuid(), "T3"),
        };

        _repoMock.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<NotificationTemplate, bool>>>(), default))
            .ReturnsAsync((System.Linq.Expressions.Expression<Func<NotificationTemplate, bool>> expr, CancellationToken _) =>
                templates.AsQueryable().Where(expr).ToList());

        var result = await _service.GetAllAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task ToggleActiveAsync_ShouldFlipIsActive()
    {
        var tenantId = Guid.NewGuid();
        _tenantMock.Setup(t => t.GetCurrentTenantId()).Returns(tenantId);

        var template = CreateTemplate(tenantId, "Test");
        _repoMock.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<NotificationTemplate, bool>>>(), default))
            .ReturnsAsync([template]);

        var result = await _service.ToggleActiveAsync(template.Id);

        result.Should().BeTrue();
        template.IsActive.Should().BeFalse();
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    private static NotificationTemplate CreateTemplate(Guid tenantId, string name)
    {
        return NotificationTemplate.Create(tenantId, name, "Title", "Body", "System");
    }
}
