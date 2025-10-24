using Game.Core;

namespace Game.UI {
    public class PauseState : UICanvasController {
		public override void EnableCanvas(bool active, float fadeDuration) {
            base.EnableCanvas(active, fadeDuration);

            // active means that canvas will appear
            if (active) {
                // you can refresh all game UI data here (this the beginning of canvas appear)
            }
        }
		
		public void OnContinueGameClick() {
            UIManager.instance.GetCanvas<GameCanvas>().PauseGame(false);
        }
    }
}

