using Extensions;
using Game;
using Game.Core;
using Game.UI;
using VContainer;
using VContainer.Unity;

public class GameplayLifetimeScope : LifetimeScope
{
    // execution order (lifetime scope is earlier then all other scripts)
    // inside lifetime scope execution: Configure is first, then Awake, OnEnable and Start

    protected override void Configure(IContainerBuilder builder) {
#if UNITY_EDITOR
        EditorScenePrepare.Setup();
#endif

        var pool = PoolManager.instance;
        var UI = UIManager.instance;
        var cash = CashManager.Instance;

        pool.OnSceneChanged();
        UI.OnSceneChanged();

        var sceneUI = transform.GetComponentFirstLevelChilds<SceneUIContainer>();
        if (sceneUI) {
            sceneUI.Setup();
        }

        builder.RegisterComponent(cash);
        builder.RegisterComponent(pool);
        builder.RegisterComponent(UI);
    }
}

