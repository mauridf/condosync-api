using CondoSync.Core.Entities;
using FluentAssertions;

namespace CondoSync.Tests.Unit.Core.Entities;

public class NotificationTemplateTests
{
    private static readonly Guid CondominiumId = Guid.NewGuid();

    [Fact]
    public void Create_ShouldSetProperties()
    {
        var template = NotificationTemplate.Create(
            CondominiumId, "ReservaConfirmada",
            "Reserva #{bookingRef} Confirmada",
            "Sua reserva para {date} às {time} foi confirmada.",
            "Booking", "in_app", "Template para confirmação de reservas");

        template.Name.Should().Be("ReservaConfirmada");
        template.IsActive.Should().BeTrue();
    }

    [Fact]
    public void RenderTitle_WithVariables_ShouldReplacePlaceholders()
    {
        var template = NotificationTemplate.Create(
            CondominiumId, "BoletimGerado",
            "Boleto #{billRef} - Vencimento {dueDate}",
            "Seu boleto no valor de {amount} vence em {dueDate}.",
            "Bill");

        var vars = new Dictionary<string, string>
        {
            ["billRef"] = "BOL-2024-001",
            ["dueDate"] = "15/12/2024",
            ["amount"] = "R$ 450,00"
        };

        template.RenderTitle(vars).Should().Be("Boleto #BOL-2024-001 - Vencimento 15/12/2024");
        template.RenderBody(vars).Should().Be("Seu boleto no valor de R$ 450,00 vence em 15/12/2024.");
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveFalse()
    {
        var template = NotificationTemplate.Create(
            CondominiumId, "Teste", "Título", "Corpo", "System");

        template.Deactivate();

        template.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_ShouldSetIsActiveTrue()
    {
        var template = NotificationTemplate.Create(
            CondominiumId, "Teste", "Título", "Corpo", "System");
        template.Deactivate();

        template.Activate();

        template.IsActive.Should().BeTrue();
    }
}
