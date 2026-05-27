using Microsoft.AspNetCore.Authorization;

namespace DailyDeBugle.Security;

public static class AuthorizationPolicyConfiguration
{
    public static void Configure(AuthorizationOptions options)
    {
        options.AddPolicy(Policies.ViewContent, policy =>
            policy.RequireAssertion(ctx =>
                ctx.User.Identity?.IsAuthenticated == true));

        options.AddPolicy(Policies.WriteArticles, policy =>
            policy.RequireRole(Roles.Author, Roles.EditorInChief, Roles.Admin));

        options.AddPolicy(Policies.ReviewArticles, policy =>
            policy.RequireRole(Roles.Editor, Roles.EditorInChief, Roles.Admin));

        options.AddPolicy(Policies.LayoutIssue, policy =>
            policy.RequireRole(Roles.LayoutDesigner, Roles.EditorInChief, Roles.Admin));

        options.AddPolicy(Policies.ManageIssues, policy =>
            policy.RequireRole(Roles.EditorInChief, Roles.Admin));

        options.AddPolicy(Policies.ManagePublications, policy =>
            policy.RequireRole(Roles.EditorInChief, Roles.Admin));

        options.AddPolicy(Policies.AccessAdminPanel, policy =>
            policy.RequireRole(Roles.Admin));

        options.AddPolicy(Policies.CommentOnPublishedIssues, policy =>
            policy.RequireRole(Roles.Reader));
    }
}
