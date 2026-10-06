using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TeamAvailability.Application.Common.Interfaces;
using TeamAvailability.Infrastructure.Options;

namespace TeamAvailability.Infrastructure.Integrations;

public sealed class JiraCloudClient(
    HttpClient httpClient,
    IOptions<JiraOptions> options,
    ILogger<JiraCloudClient> logger) : IJiraClient
{
    private readonly JiraOptions _options = options.Value;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(_options.BaseUrl)
        && !string.IsNullOrWhiteSpace(_options.Email)
        && !string.IsNullOrWhiteSpace(_options.ApiToken);

    public async Task<JiraIssueResult?> CreateOnCallIssueAsync(
        string projectKey,
        string issueTypeId,
        string summary,
        string description,
        string? assigneeEmail,
        CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
        {
            logger.LogWarning("Jira integration is not configured. Skipping issue creation.");
            return null;
        }

        string? accountId = null;
        if (!string.IsNullOrWhiteSpace(assigneeEmail))
            accountId = await FindAccountIdByEmailAsync(assigneeEmail, cancellationToken);

        var fields = new Dictionary<string, object>
        {
            ["project"] = new { key = projectKey },
            ["summary"] = summary,
            ["issuetype"] = new { id = issueTypeId },
            ["description"] = BuildAdfDocument(description)
        };

        if (!string.IsNullOrWhiteSpace(accountId))
            fields["assignee"] = new { accountId };

        using var request = CreateRequest(HttpMethod.Post, "/rest/api/3/issue");
        request.Content = new StringContent(JsonSerializer.Serialize(new { fields }), Encoding.UTF8, "application/json");

        var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogError("Jira issue creation failed ({Status}): {Body}", response.StatusCode, body);
            response.EnsureSuccessStatusCode();
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        var issueKey = document.RootElement.GetProperty("key").GetString()
            ?? throw new InvalidOperationException("Jira response did not contain issue key.");

        var issueUrl = $"{_options.BaseUrl.TrimEnd('/')}/browse/{issueKey}";
        logger.LogInformation("Created Jira on-call issue {IssueKey} for {Summary}", issueKey, summary);
        return new JiraIssueResult(issueKey, issueUrl);
    }

    public async Task AddCommentAsync(string issueKey, string comment, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
        {
            logger.LogWarning("Jira integration is not configured. Skipping comment.");
            return;
        }

        using var request = CreateRequest(HttpMethod.Post, $"/rest/api/3/issue/{issueKey}/comment");
        request.Content = new StringContent(
            JsonSerializer.Serialize(new { body = BuildAdfDocument(comment) }),
            Encoding.UTF8,
            "application/json");

        var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private async Task<string?> FindAccountIdByEmailAsync(string email, CancellationToken cancellationToken)
    {
        using var request = CreateRequest(
            HttpMethod.Get,
            $"/rest/api/3/user/search?query={Uri.EscapeDataString(email)}&maxResults=1");

        var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("Jira user search failed for {Email} ({Status})", email, response.StatusCode);
            return null;
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        if (document.RootElement.GetArrayLength() == 0)
        {
            logger.LogWarning("No Jira user found for email {Email}", email);
            return null;
        }

        return document.RootElement[0].GetProperty("accountId").GetString();
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string relativePath)
    {
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_options.Email}:{_options.ApiToken}"));
        var request = new HttpRequestMessage(method, $"{_options.BaseUrl.TrimEnd('/')}{relativePath}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return request;
    }

    private static object BuildAdfDocument(string text) => new
    {
        type = "doc",
        version = 1,
        content = new[]
        {
            new
            {
                type = "paragraph",
                content = new[] { new { type = "text", text } }
            }
        }
    };
}
