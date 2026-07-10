using CondoSync.Core.Entities;
using CondoSync.Core.Enums;
using CondoSync.Core.Exceptions;
using FluentAssertions;

namespace CondoSync.Tests.Unit.Core.Entities;

public class BookingTests
{
    private static readonly Guid CondominiumId = Guid.NewGuid();
    private static readonly Guid ServiceId = Guid.NewGuid();
    private static readonly Guid UnitId = Guid.NewGuid();
    private static readonly Guid ResidentId = Guid.NewGuid();

    [Fact]
    public void Create_WithoutApproval_ShouldSetApproved()
    {
        var booking = Booking.Create(
            CondominiumId, ServiceId, UnitId, ResidentId,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            new TimeOnly(10, 0), new TimeOnly(12, 0),
            requiresApproval: false);

        booking.Status.Should().Be(BookingStatus.Approved);
    }

    [Fact]
    public void Create_WithApproval_ShouldSetPending()
    {
        var booking = Booking.Create(
            CondominiumId, ServiceId, UnitId, ResidentId,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            new TimeOnly(14, 0), new TimeOnly(16, 0),
            requiresApproval: true);

        booking.Status.Should().Be(BookingStatus.Pending);
    }

    [Fact]
    public void Approve_PendingBooking_ShouldSetApproved()
    {
        var booking = CreatePendingBooking();
        var approvedBy = Guid.NewGuid();

        booking.Approve(approvedBy);

        booking.Status.Should().Be(BookingStatus.Approved);
        booking.ApprovedBy.Should().Be(approvedBy);
        booking.ApprovedAt.Should().NotBeNull();
    }

    [Fact]
    public void Approve_NonPendingBooking_ShouldThrow()
    {
        var booking = CreatePendingBooking();
        booking.Approve(Guid.NewGuid());

        var act = () => booking.Approve(Guid.NewGuid());

        act.Should().Throw<DomainException>().WithMessage("*apenas reservas pendentes*");
    }

    [Fact]
    public void Cancel_ShouldSetCancelled()
    {
        var booking = CreatePendingBooking();
        var cancelledBy = Guid.NewGuid();

        booking.Cancel(cancelledBy, "Indisponível");

        booking.Status.Should().Be(BookingStatus.Cancelled);
        booking.CancelledBy.Should().Be(cancelledBy);
    }

    [Fact]
    public void CheckIn_ShouldSetCheckedInAt()
    {
        var booking = CreateApprovedBooking();

        booking.CheckIn();

        booking.CheckedInAt.Should().NotBeNull();
    }

    [Fact]
    public void MarkAsNoShow_ShouldSetStatus()
    {
        var booking = CreateApprovedBooking();

        booking.MarkAsNoShow();

        booking.Status.Should().Be(BookingStatus.NoShow);
    }

    [Fact]
    public void CheckOut_ShouldCompleteBooking()
    {
        var booking = CreateApprovedBooking();
        booking.CheckIn();

        booking.CheckOut();

        booking.Status.Should().Be(BookingStatus.Completed);
        booking.CheckedOutAt.Should().NotBeNull();
    }

    private Booking CreatePendingBooking()
    {
        return Booking.Create(CondominiumId, ServiceId, UnitId, ResidentId,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            new TimeOnly(9, 0), new TimeOnly(10, 0),
            requiresApproval: true);
    }

    private Booking CreateApprovedBooking()
    {
        return Booking.Create(CondominiumId, ServiceId, UnitId, ResidentId,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            new TimeOnly(9, 0), new TimeOnly(10, 0),
            requiresApproval: false);
    }
}
