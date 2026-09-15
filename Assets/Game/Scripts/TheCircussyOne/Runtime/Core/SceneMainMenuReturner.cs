using PrimeTween;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheCircussyOne.Runtime
{
    public sealed class SceneMainMenuReturner : IMainMenuReturner
    {
        private const string MainMenuSceneName = "MainMenu";
        private const string Title = "Lowering the Curtain";
        private const string Subtitle = "Returning to the main menu...";

        private readonly IRunTransitionService transitionService;
        private bool isReturning;

        public SceneMainMenuReturner(IRunTransitionService transitionService = null)
        {
            this.transitionService = transitionService;
        }

        public void ReturnToMainMenu()
        {
            if (isReturning)
            {
                return;
            }

            isReturning = true;
            try
            {
                if (transitionService != null && transitionService.IsEnabled)
                {
                    bool started = transitionService.PlaySceneReload(
                        Title,
                        Subtitle,
                        () =>
                        {
                            PrepareForSceneChange();
                            return SceneManager.LoadSceneAsync(MainMenuSceneName, LoadSceneMode.Single);
                        });
                    if (!started)
                    {
                        isReturning = false;
                    }

                    return;
                }

                PrepareForSceneChange();
                SceneManager.LoadSceneAsync(MainMenuSceneName, LoadSceneMode.Single);
            }
            catch
            {
                isReturning = false;
                throw;
            }
        }

        private static void PrepareForSceneChange()
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Tween.StopAll();
        }
    }
}
