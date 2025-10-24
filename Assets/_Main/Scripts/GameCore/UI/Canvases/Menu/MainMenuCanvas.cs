using Game.Core;
using Game.UI;
using PrimeTween;
using UnityEngine;

public class MainMenuCanvas : UICanvasController {
    [SerializeField] private float _disableCanvasDuration = 0.5f;

    public void OnPlayBtnClickGameScene() {
        UIManager.instance.DisableActiveCanvas(_disableCanvasDuration);

        Tween.Delay(this, _disableCanvasDuration, target => target.LoadGame("GameScene"));
    }

    public void OnPlayBtnClickLevelScene() {
        UIManager.instance.DisableActiveCanvas(_disableCanvasDuration);

        Tween.Delay(this, _disableCanvasDuration, target => target.LoadGame("LevelScene"));
    }

    private void LoadGame(string nameScene) {
        SceneSystem.Instance.LoadScene(nameScene, LoadScreens.CutoutCube, LoadScreens.FadeInOut);
    }
}
