using System.Security.Claims;
using DailyDeBugle.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DailyDeBugle.Tests;

public class AuthorizationPolicyConfigurationTests
{
    private static ServiceProvider BuildServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthorization(AuthorizationPolicyConfiguration.Configure);
        return services.BuildServiceProvider();
    }

    [Theory]
    [InlineData(Roles.Reader, Policies.ViewContent, true)]
    [InlineData(Roles.Reader, Policies.WriteArticles, false)]
    [InlineData(Roles.Author, Policies.WriteArticles, true)]
    [InlineData(Roles.Editor, Policies.ReviewArticles, true)]
    [InlineData(Roles.LayoutDesigner, Policies.LayoutIssue, true)]
    [InlineData(Roles.EditorInChief, Policies.ManagePublications, true)]
    [InlineData(Roles.Admin, Policies.ViewContent, true)]
    [InlineData(Roles.Admin, Policies.WriteArticles, true)]
    [InlineData(Roles.Admin, Policies.ReviewArticles, true)]
    [InlineData(Roles.Admin, Policies.LayoutIssue, true)]
    [InlineData(Roles.Admin, Policies.ManageIssues, true)]
    [InlineData(Roles.Admin, Policies.ManagePublications, true)]
    public async Task Role_matches_policy_expectation(string role, string policy, bool expectSuccess)
    {
        await using var sp = BuildServices();
        var auth = sp.GetRequiredService<IAuthorizationService>();

        var user = new ClaimsPrincipal(new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.Role, role), new Claim(ClaimTypes.Name, "u") },
            "test"));

        var result = await auth.AuthorizeAsync(user, resource: null, policyName: policy);
        Assert.Equal(expectSuccess, result.Succeeded);
    }
}
