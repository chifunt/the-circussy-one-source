namespace TheCircussyOne.Content
{
    public interface IContentDefinition
    {
        string Id { get; }
        string DisplayName { get; }
        ContentTagSet Tags { get; }
    }
}
