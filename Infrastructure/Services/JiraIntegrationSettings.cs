using Microsoft.Extensions.Options;
using TeamAvailability.Application.Common.Interfaces;
using TeamAvailability.Infrastructure.Options;

namespace TeamAvailability.Infrastructure.Services;

public sealed class JiraIntegrationSettings(IOptions<JiraOptions> options) : IJiraIntegrationSettings
{
    private readonly JiraOptions _options = options.Value;

    public bool Enabled => _options.Enabled;
    public string ProjectKey => _options.ProjectKey;
    public string IssueTypeId => _options.IssueTypeId;
    public string BaseUrl => _options.BaseUrl.TrimEnd('/');
}
