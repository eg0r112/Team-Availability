namespace TeamAvailability.Infrastructure.Options;

public sealed class JiraOptions
{
    public const string SectionName = "Jira";

    public bool Enabled { get; init; }
    public string BaseUrl { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string ApiToken { get; init; } = string.Empty;
    public string ProjectKey { get; init; } = string.Empty;
    public string IssueType { get; init; } = "Task";
    public string IssueTypeId { get; init; } = string.Empty;
}
