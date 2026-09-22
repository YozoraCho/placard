using Dalamud.Interface.ImGuiNotification;
using Dalamud.Plugin.Services;
using Placard.Core.Localization;
using Placard.Core.Runtime;

namespace Placard.Core.Housing;

internal sealed class HousingReminderService : IDisposable
{
    private const long TickIntervalMilliseconds = 1000;
    private static readonly TimeSpan NotificationDuration = TimeSpan.FromSeconds(20);

    private readonly Configuration configuration;
    private readonly INotificationManager notifications;
    private readonly HousingWatchStore watch;
    private readonly FrameworkTicker ticker;
    private readonly HashSet<string> delivered = new(StringComparer.Ordinal);

    public HousingReminderService(Configuration configuration, IFramework framework,
        INotificationManager notifications, HousingWatchStore watch)
    {
        this.configuration = configuration;
        this.notifications = notifications;
        this.watch = watch;
        ticker = new FrameworkTicker(framework, TickIntervalMilliseconds, OnTick);
    }

    private void OnTick() => TickReminders(DateTime.UtcNow);

    private void TickReminders(DateTime nowUtc)
    {
        var reminders = configuration.HousingReminders;
        for (var index = 0; index < reminders.Count; index++)
        {
            var reminder = reminders[index];
            if (reminder.Notified)
            {
                continue;
            }

            if (reminder.Phase == (byte)HousingLotteryPhase.Entry && !configuration.HousingNotifyEntry)
            {
                continue;
            }

            if (reminder.Phase == (byte)HousingLotteryPhase.Results && !configuration.HousingNotifyResults)
            {
                continue;
            }

            if (reminder.FireAtUtc is not { } fireAt || nowUtc < fireAt)
            {
                continue;
            }

            var key = reminder.DedupeKey;
            if (!delivered.Add(key))
            {
                watch.MarkNotified(reminder);
                continue;
            }

            Deliver(reminder, nowUtc);
            watch.MarkNotified(reminder);
        }

        watch.PruneFiredReminders(nowUtc);
    }

    private void Deliver(HousingReminderRecord reminder, DateTime nowUtc)
    {
        var place = HousingFormat.Place(HousingDistricts.DisplayName(reminder.DistrictId), reminder.Ward);
        var remaining = HousingFormat.Remaining(reminder.PhaseEndUtc, nowUtc) ?? TimeSpan.Zero;
        var countdown = HousingFormat.Countdown(remaining);
        var phase = (HousingLotteryPhase)reminder.Phase;
        if (phase == HousingLotteryPhase.Results)
        {
            Notify(Loc.T(L.Housing.NotifyResultsTitle), Loc.T(L.Housing.NotifyResultsBody, countdown));
        }
        else
        {
            var body = Loc.T(L.Housing.NotifyEntryBody, reminder.Plot, place, countdown);
            var detail = Loc.T(L.Housing.NotifyEntryDetail, HousingFormat.Entries(reminder.Entries),
                HousingFormat.ScanAge(FromUnix(reminder.LastSeenUnix), nowUtc));
            Notify(Loc.T(L.Housing.NotifyEntryTitle), string.Concat(body, "\n", detail));
        }

        PlacardLog.Debug($"Housing reminder delivered for {reminder.DedupeKey}.");
    }

    private void Notify(string title, string body)
    {
        notifications.AddNotification(new Notification
        {
            Title = title,
            Content = body,
            Type = NotificationType.Info,
            InitialDuration = NotificationDuration,
        });
    }

    private static DateTime FromUnix(long unixSeconds) =>
        unixSeconds <= 0L ? default : DateTimeOffset.FromUnixTimeSeconds(unixSeconds).UtcDateTime;

    public void Dispose()
    {
        ticker.Dispose();
    }
}
