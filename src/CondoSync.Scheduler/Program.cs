using CondoSync.Application;
using CondoSync.Infrastructure;
using CondoSync.Scheduler.Jobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = Host.CreateApplicationBuilder(args);

    builder.Services.AddSerilog((services, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(builder.Configuration)
            .ReadFrom.Services(services)
            .Enrich.WithProperty("Application", "CondoSync.Scheduler")
            .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
            .Enrich.FromLogContext();
    });

    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddApplication();
    builder.Services.AddAutoMapper(cfg => cfg.AddMaps(typeof(CondoSync.Application.Common.Mappings.CondominiumProfile).Assembly));

    builder.Services.AddQuartz(q =>
    {
        q.UseSimpleTypeLoader();
        q.UseInMemoryStore();

        var outboxJobKey = new JobKey("ProcessOutboxMessages");
        q.AddJob<ProcessOutboxMessagesJob>(opts => opts.WithIdentity(outboxJobKey));
        q.AddTrigger(opts => opts
            .ForJob(outboxJobKey)
            .WithIdentity("ProcessOutboxMessages-Trigger")
            .WithCronSchedule("0 * * * * ?"));

        var noShowJobKey = new JobKey("ExpirePastBookings");
        q.AddJob<ExpirePastBookingsJob>(opts => opts.WithIdentity(noShowJobKey));
        q.AddTrigger(opts => opts
            .ForJob(noShowJobKey)
            .WithIdentity("ExpirePastBookings-Trigger")
            .WithCronSchedule("0 */5 * * * ?"));

        var checkOutJobKey = new JobKey("CompletePastBookings");
        q.AddJob<CompletePastBookingsJob>(opts => opts.WithIdentity(checkOutJobKey));
        q.AddTrigger(opts => opts
            .ForJob(checkOutJobKey)
            .WithIdentity("CompletePastBookings-Trigger")
            .WithCronSchedule("0 */5 * * * ?"));

        var inviteJobKey = new JobKey("ExpireUnitInvitations");
        q.AddJob<ExpireUnitInvitationsJob>(opts => opts.WithIdentity(inviteJobKey));
        q.AddTrigger(opts => opts
            .ForJob(inviteJobKey)
            .WithIdentity("ExpireUnitInvitations-Trigger")
            .WithCronSchedule("0 0 * * * ?"));

        var slaJobKey = new JobKey("CheckTicketSlaBreach");
        q.AddJob<CheckTicketSlaBreachJob>(opts => opts.WithIdentity(slaJobKey));
        q.AddTrigger(opts => opts
            .ForJob(slaJobKey)
            .WithIdentity("CheckTicketSlaBreach-Trigger")
            .WithCronSchedule("0 */5 * * * ?"));

        var archiveJobKey = new JobKey("ArchiveOldActivityLogs");
        q.AddJob<ArchiveOldActivityLogsJob>(opts => opts.WithIdentity(archiveJobKey));
        q.AddTrigger(opts => opts
            .ForJob(archiveJobKey)
            .WithIdentity("ArchiveOldActivityLogs-Trigger")
            .WithCronSchedule("0 0 3 * * ?"));

        var fineJobKey = new JobKey("BillFineCalculation");
        q.AddJob<BillFineCalculationJob>(opts => opts.WithIdentity(fineJobKey));
        q.AddTrigger(opts => opts
            .ForJob(fineJobKey)
            .WithIdentity("BillFineCalculation-Trigger")
            .WithCronSchedule("0 0 2 * * ?"));

        var reminderJobKey = new JobKey("SendPaymentReminders");
        q.AddJob<SendPaymentRemindersJob>(opts => opts.WithIdentity(reminderJobKey));
        q.AddTrigger(opts => opts
            .ForJob(reminderJobKey)
            .WithIdentity("SendPaymentReminders-Trigger")
            .WithCronSchedule("0 0 8 * * ?"));

        var guestListJobKey = new JobKey("CleanupExpiredGuestLists");
        q.AddJob<CleanupExpiredGuestListsJob>(opts => opts.WithIdentity(guestListJobKey));
        q.AddTrigger(opts => opts
            .ForJob(guestListJobKey)
            .WithIdentity("CleanupExpiredGuestLists-Trigger")
            .WithCronSchedule("0 0 1 * * ?"));

        var visitorJobKey = new JobKey("ExpireVisitors");
        q.AddJob<ExpireVisitorsJob>(opts => opts.WithIdentity(visitorJobKey));
        q.AddTrigger(opts => opts
            .ForJob(visitorJobKey)
            .WithIdentity("ExpireVisitors-Trigger")
            .WithCronSchedule("0 0 * * * ?"));
    });

    builder.Services.AddQuartzHostedService(opts =>
    {
        opts.WaitForJobsToComplete = true;
    });

    var app = builder.Build();

    Log.Information("CondoSync Scheduler iniciando no ambiente {Environment} com 10 jobs...",
        app.Services.GetRequiredService<IHostEnvironment>().EnvironmentName);
    Log.Information("Jobs registrados: ProcessOutboxMessages (1min), ExpirePastBookings (5min), CompletePastBookings (5min), ExpireUnitInvitations (1h), CheckTicketSlaBreach (5min), ArchiveOldActivityLogs (diario 3h), BillFineCalculation (diario 2h), SendPaymentReminders (diario 8h), CleanupExpiredGuestLists (diario 1h), ExpireVisitors (1h)");

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Scheduler terminou inesperadamente");
}
finally
{
    Log.CloseAndFlush();
}
