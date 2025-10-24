using Game.Core;
using UnityEngine;

namespace Game.UI {
    public class GameCanvas : UICanvasController {

        public MenuState MainState;
        public WinState WinState;
        public LoseState LoseState;

        [SerializeField] private PauseState _pauseState;
        private UICanvasController _lastActiveState;

        protected override void Awake() {
            base.Awake();
            _lastActiveState = _activeState;
        }

        public override void EnableCanvas(bool active, float fadeDuration) {
            base.EnableCanvas(active, fadeDuration);

            // active means that canvas will appear
            if (active) {
                // you can refresh all game UI data here (this the beginning of canvas appear)
            }
        }

        public void TogglePause(float fadeDuration) {
            PauseGame(_activeState != _pauseState, fadeDuration);
        }

        public void PauseGame(bool active, float fadeDuration = 0.25f) {
            if (active) {
                TimeManager.Instance.FreezeTime();
                SetState(_pauseState, fadeDuration);
            }
            else {
                TimeManager.Instance.UnfreezeTime();
                SetState(_lastActiveState, fadeDuration);
            }
        }
    }
}
