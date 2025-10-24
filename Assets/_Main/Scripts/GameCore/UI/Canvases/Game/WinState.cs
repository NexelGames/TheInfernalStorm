using Game.Core;

namespace Game.UI {
    public class WinState : UICanvasController {
        public override void EnableCanvas(bool active, float fadeDuration) {
            base.EnableCanvas(active, fadeDuration);

            // active means that canvas will appear
            if (active) {
                // you can calculate statistics and etc here
            }
        }
    }
}

