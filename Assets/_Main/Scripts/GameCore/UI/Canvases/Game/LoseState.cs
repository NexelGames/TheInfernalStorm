using Game.Core;

namespace Game.UI {
    public class LoseState : UICanvasController {
        public override void EnableCanvas(bool active, float fadeDuration) {
            base.EnableCanvas(active, fadeDuration);

            // active means that canvas will appear
            if (active) {
                // you can calculate statistics and etc here
            }
        }

        public void OnReturnMenuClick() {
            // disable graphic raycaster to avoid UI interactions while scene loading
            _graphicRaycaster.enabled = false;
            SceneSystem.Instance.LoadScene("MainMenu");
        }

        public void OnRestartClick() {
            // disable graphic raycaster to avoid UI interactions while scene loading
            _graphicRaycaster.enabled = false;
            SceneSystem.Instance.LoadScene("GameScene", LoadScreens.FadeInOut);
        }
    }
}

