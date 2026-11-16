namespace SorteFederation.Builders;

public class JobRunnerOptions
{
    public const string SectionName = "JobRunnerOptions";

    public TimeSpan ConfirmationReminderDelay { get; set; } = TimeSpan.FromSeconds(30);

    public int UnconfirmedAccountLifetimeDays { get; set; } = 7;

    public TimeSpan SimulatedWorkDuration { get; set; } = TimeSpan.FromSeconds(2);
}
