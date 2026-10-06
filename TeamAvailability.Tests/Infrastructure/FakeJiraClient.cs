using TeamAvailability.Application.Common.Interfaces;

namespace TeamAvailability.Tests.Infrastructure;

public sealed class FakeJiraClient : IJiraClient
{
    public bool IsConfigured => true;
    public List<(string ProjectKey, string Summary, string? Email)> CreatedIssues { get; } = [];

    public Task<JiraIssueResult?> CreateOnCallIssueAsync(
        string projectKey,
        string issueTypeId,
        string summary,
        string description,
        string? assigneeEmail,
        CancellationToken cancellationToken = default)
    {
        CreatedIssues.Add((projectKey, summary, assigneeEmail));
        return Task.FromResult<JiraIssueResult?>(new JiraIssueResult("TEST-1", "https://jira.test/browse/TEST-1"));
    }

    public Task AddCommentAsync(string issueKey, string comment, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
