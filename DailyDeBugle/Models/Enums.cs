namespace DailyDeBugle.Models
{
    public enum UserRole
    {
        Author,
        Editor,
        LayoutDesigner,
        EditorInChief
    }

    public enum ArticleStatus
    {
        Draft,
        UnderReview,
        RequiresRevision,
        Approved,
        Rejected
    }

    public enum IssueStatus
    {
        InProgress,
        LayoutInProgress,
        ReadyForPublish,
        Published
    }

    public enum ElementType
    {
        TextFrame,
        ImageFrame,
        AdBlock
    }
}