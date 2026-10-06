namespace TeamAvailability.Application.Common.Interfaces;

public interface IJiraClient
{
    bool IsConfigured { get; }

    Task<JiraIssueResult?> CreateOnCallIssueAsync(
        string projectKey,
        string issueTypeId,
        string summary,
        string description,
        string? assigneeEmail,
        CancellationToken cancellationToken = default);

    Task AddCommentAsync(string issueKey, string comment, CancellationToken cancellationToken = default);
}
