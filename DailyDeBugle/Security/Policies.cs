namespace DailyDeBugle.Security;

public static class Roles
{
    public const string Reader = "Reader";
    public const string Author = "Author";
    public const string Editor = "Editor";
    public const string LayoutDesigner = "LayoutDesigner";
    public const string EditorInChief = "EditorInChief";
    public const string Admin = "Admin";
}

public static class Policies
{
    public const string ViewContent = nameof(ViewContent);                  // any authenticated user
    public const string WriteArticles = nameof(WriteArticles);              // Author + EiC
    public const string ReviewArticles = nameof(ReviewArticles);            // Editor + EiC
    public const string LayoutIssue = nameof(LayoutIssue);                  // LayoutDesigner + EiC
    public const string ManageIssues = nameof(ManageIssues);                // EiC
    public const string ManagePublications = nameof(ManagePublications);    // EiC
    public const string AccessAdminPanel = nameof(AccessAdminPanel);        // Admin only
}

