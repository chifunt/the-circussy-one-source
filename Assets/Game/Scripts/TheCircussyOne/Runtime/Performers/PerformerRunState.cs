using System;
using TheCircussyOne.Content;

namespace TheCircussyOne.Runtime
{
    public sealed class PerformerRunState
    {
        public PerformerDefinition SelectedPerformer { get; private set; }
        public bool IsSelectionComplete => SelectedPerformer != null;
        public string SelectedPerformerId => SelectedPerformer != null ? SelectedPerformer.Id : string.Empty;

        public event Action<PerformerDefinition> PerformerSelected;

        public void Select(PerformerDefinition performer)
        {
            if (performer == null)
            {
                return;
            }

            SelectedPerformer = performer;
            PerformerSelected?.Invoke(performer);
        }

        public void Clear()
        {
            SelectedPerformer = null;
        }
    }
}
