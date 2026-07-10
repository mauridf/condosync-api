using CondoSync.Core.Entities;
using CondoSync.Core.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CondoSync.Infrastructure.Data;

public class CondoSyncDbContext : DbContext
{
    private readonly Tenant.TenantInterceptor? _tenantInterceptor;

    // Entidades principais
    public DbSet<Condominium> Condominiums { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Unit> Units { get; set; } = null!;
    public DbSet<Resident> Residents { get; set; } = null!;
    public DbSet<Service> Services { get; set; } = null!;
    public DbSet<Booking> Bookings { get; set; } = null!;
    public DbSet<Notice> Notices { get; set; } = null!;
    public DbSet<NoticeComment> NoticeComments { get; set; } = null!;
    public DbSet<Ticket> Tickets { get; set; } = null!;
    public DbSet<TicketMessage> TicketMessages { get; set; } = null!;
    public DbSet<Bill> Bills { get; set; } = null!;
    public DbSet<Visitor> Visitors { get; set; } = null!;
    public DbSet<CommonArea> CommonAreas { get; set; } = null!;
    public DbSet<Poll> Polls { get; set; } = null!;
    public DbSet<PollVote> PollVotes { get; set; } = null!;
    public DbSet<Document> Documents { get; set; } = null!;
    public DbSet<Notification> Notifications { get; set; } = null!;
    public DbSet<ActivityLog> ActivityLogs { get; set; } = null!;
    public DbSet<UnitInvitation> UnitInvitations { get; set; } = null!;
    public DbSet<CondominiumSettings> CondominiumSettings { get; set; } = null!;
    public DbSet<GuestList> GuestLists { get; set; } = null!;

    // Infraestrutura
    public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;
    public DbSet<EventStoreEntry> EventStore { get; set; } = null!;

    public CondoSyncDbContext(DbContextOptions<CondoSyncDbContext> options, Tenant.TenantInterceptor? tenantInterceptor = null) : base(options)
    {
                _tenantInterceptor = tenantInterceptor;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (_tenantInterceptor != null)
        {
            optionsBuilder.AddInterceptors(_tenantInterceptor);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("public");

        // Ignorar DomainEvent (usado apenas em memória via AggregateRoot)
        modelBuilder.Ignore<DomainEvent>();

        // Converter nomes de propriedades para snake_case
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entity.GetProperties())
            {
                property.SetColumnName(ToSnakeCase(property.Name));
            }

            foreach (var key in entity.GetKeys())
            {
                key.SetName(ToSnakeCase(key.GetName() ?? ""));
            }

            foreach (var foreignKey in entity.GetForeignKeys())
            {
                foreignKey.SetConstraintName(ToSnakeCase(foreignKey.GetConstraintName() ?? ""));
            }

            foreach (var index in entity.GetIndexes())
            {
                index.SetDatabaseName(ToSnakeCase(index.GetDatabaseName() ?? ""));
            }
        }

        // Aplicar configurações de cada entidade
        modelBuilder.ApplyConfiguration(new Configurations.CondominiumConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.UserConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.UnitConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.ResidentConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.ServiceConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.BookingConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.NoticeConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.NoticeCommentConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.TicketConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.TicketMessageConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.BillConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.VisitorConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.CommonAreaConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.PollConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.PollVoteConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.DocumentConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.NotificationConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.ActivityLogConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.UnitInvitationConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.CondominiumSettingsConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.GuestListConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.OutboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.EventStoreEntryConfiguration());
    }

    private static string ToSnakeCase(string input)
    {
        return string.Concat(input.Select((c, i) =>
            i > 0 && char.IsUpper(c) ? "_" + c.ToString() : c.ToString())).ToLowerInvariant();
    }
}