using System.Collections.Generic;

namespace TheCircussyOne.Content
{
    public interface IContentCatalog<out TDefinition>
        where TDefinition : class, IContentDefinition
    {
        IReadOnlyList<TDefinition> Definitions { get; }
    }
}
