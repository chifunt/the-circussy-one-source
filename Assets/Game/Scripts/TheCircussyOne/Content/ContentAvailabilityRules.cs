using System.Collections.Generic;

namespace TheCircussyOne.Content
{
    public static class ContentAvailabilityRules
    {
        public static bool IsActive(IContentDefinition definition)
        {
            return definition is not IActivatableContentDefinition activatable || activatable.IsActive;
        }

        public static bool IsActiveAndValid(IContentDefinition definition)
        {
            return definition != null
                && IsActive(definition)
                && ContentId.IsValidValue(definition.Id)
                && !string.IsNullOrWhiteSpace(definition.DisplayName);
        }

        public static int CountActive<TDefinition>(IReadOnlyList<TDefinition> definitions)
            where TDefinition : class, IContentDefinition
        {
            if (definitions == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < definitions.Count; i++)
            {
                if (IsActiveAndValid(definitions[i]))
                {
                    count++;
                }
            }

            return count;
        }

        public static TDefinition FirstActive<TDefinition>(IReadOnlyList<TDefinition> definitions)
            where TDefinition : class, IContentDefinition
        {
            if (definitions == null)
            {
                return null;
            }

            for (int i = 0; i < definitions.Count; i++)
            {
                TDefinition definition = definitions[i];
                if (IsActiveAndValid(definition))
                {
                    return definition;
                }
            }

            return null;
        }
    }
}
