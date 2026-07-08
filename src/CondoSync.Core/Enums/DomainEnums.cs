namespace CondoSync.Core.Enums;

public enum SuperAdminRole
{
    SuperAdmin = 0,
    Support = 1,
    Analyst = 2
}

public enum UserRole
{
    CondoAdmin = 0,
    SubAdmin = 1,
    Employee = 2,
    Owner = 3,
    Tenant = 4,
    Resident = 5,
    Visitor = 6
}

public enum ResidentType
{
    Owner = 0,
    Tenant = 1,
    FamilyMember = 2,
    Dependent = 3,
    Employee = 4
}

public enum ServiceType
{
    Bookable = 0,
    Requestable = 1,
    Informational = 2,
    Scheduled = 3
}

public enum BookingStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Cancelled = 3,
    Completed = 4,
    NoShow = 5
}

public enum NoticeCategory
{
    General = 0,
    Urgent = 1,
    Maintenance = 2,
    Event = 3,
    Security = 4,
    Financial = 5
}

public enum TicketPriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    Urgent = 3,
    Critical = 4
}

public enum TicketStatus
{
    Open = 0,
    InProgress = 1,
    WaitingParts = 2,
    WaitingThirdParty = 3,
    Resolved = 4,
    Closed = 5,
    Reopened = 6
}

public enum BillStatus
{
    Pending = 0,
    Paid = 1,
    Overdue = 2,
    PartiallyPaid = 3,
    Cancelled = 4,
    Waived = 5,
    Loss = 6
}

public enum VisitorType
{
    Guest = 0,
    ServiceProvider = 1,
    Delivery = 2,
    Family = 3,
    Employee = 4
}

public enum VisitorStatus
{
    Authorized = 0,
    Arrived = 1,
    Departed = 2,
    Cancelled = 3,
    Expired = 4
}

public enum SubscriptionPlan
{
    Trial = 0,
    Free = 1,
    Basic = 2,
    Premium = 3,
    Enterprise = 4
}

public enum SubscriptionStatus
{
    Active = 0,
    Suspended = 1,
    Cancelled = 2,
    Trial = 3
}

public enum PaymentMethod
{
    Boleto = 0,
    Pix = 1,
    CreditCard = 2,
    DebitCard = 3,
    Cash = 4,
    BankTransfer = 5
}

public enum PaymentStatus
{
    Pending = 0,
    Paid = 1,
    Refunded = 2,
    Waived = 3,
    NotRequired = 4
}

public enum NotificationType
{
    Booking = 0,
    Bill = 1,
    Ticket = 2,
    Notice = 3,
    Visitor = 4,
    Poll = 5,
    System = 6
}

public enum PollStatus
{
    Draft = 0,
    Active = 1,
    Closed = 2,
    Cancelled = 3
}

public enum PollType
{
    Single = 0,
    Multiple = 1,
    Ranked = 2
}

public enum DocumentType
{
    Minutes = 0,
    Regulation = 1,
    Budget = 2,
    Report = 3,
    Contract = 4,
    Insurance = 5,
    Other = 6
}

public enum UnitOccupancyStatus
{
    Vacant = 0,
    OccupiedByOwner = 1,
    OccupiedByTenant = 2,
    UnderRenovation = 3
}

public enum UnitType
{
    Apartment = 0,
    House = 1,
    Commercial = 2,
    Garage = 3,
    Storage = 4
}