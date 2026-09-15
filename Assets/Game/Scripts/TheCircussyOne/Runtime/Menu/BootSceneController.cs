using TheCircussyOne.Config;
using TheCircussyOne.Visuals;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheCircussyOne.Runtime
{
    public sealed class BootSceneController : MonoBehaviour
    {
        [SerializeField] private HudVisualConfig transitionConfig;
        [SerializeField] private string mainMenuSceneName = "MainMenu";
        [SerializeField, Min(0f)] private float minimumVisibleSeconds = 0.8f;

        private void Start()
        {
            RunTransitionOverlayHost.GetOrCreate().Play(
                transitionConfig,
                "Opening Night",
                "Raising the curtain...",
                () => SceneManager.LoadSceneAsync(mainMenuSceneName, LoadSceneMode.Single),
                minimumVisibleSecondsOverride: minimumVisibleSeconds);
        }
    }
}
