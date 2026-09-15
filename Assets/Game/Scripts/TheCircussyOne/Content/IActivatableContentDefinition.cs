namespace TheCircussyOne.Content
{
    public interface IActivatableContentDefinition : IContentDefinition
    {
        bool IsActive { get; }
    }
}
