using Game.Core;
using UnityEditor;
using UnityEngine;
using Alchemy.Editor;
using UnityEngine.UIElements;

[CustomEditor(typeof(CameraFollower))]
public class CameraFollowerCustomEditor : AlchemyEditor {
    IntegerField _povIndexFieldUI;

    public override VisualElement CreateInspectorGUI() {
        var root = base.CreateInspectorGUI();

        root.Add(new Label(""));
        root.Add(new Label("Editor Tools"));

        _povIndexFieldUI = new IntegerField("POV Index") { name = "POV Index", maxLength = 3 };
        _povIndexFieldUI.RegisterCallback<ChangeEvent<int>>((value) => {
            var camera = (CameraFollower)target;
            if (value.newValue > camera.CameraPOV_s.Length - 1) _povIndexFieldUI.value = camera.CameraPOV_s.Length - 1;
            else if (value.newValue < 0) _povIndexFieldUI.value = 0;
        });
        root.Add(_povIndexFieldUI);

        var button = new Button() { text = "Refresh POV index" };
        button.clicked += RefreshPOVIndexClick;
        root.Add(button);

        button = new Button() { text = "Add POV to end" };
        button.clicked += AddPOVClick;
        root.Add(button);

        button = new Button() { text = "Preview POV" };
        button.clicked += PreviewPOVIndexClick;
        root.Add(button);

        return root;
    }

    private void AddPOVClick() {
        AddPOV((CameraFollower)target);
        EditorUtility.SetDirty(target);
        serializedObject.ApplyModifiedProperties();
    }

    private void RefreshPOVIndexClick() {
        RefreshPOV(_povIndexFieldUI.value, (CameraFollower)target);
        EditorUtility.SetDirty(target);
        serializedObject.ApplyModifiedProperties();
    }

    private void PreviewPOVIndexClick() {
        PreviewPOV(_povIndexFieldUI.value, (CameraFollower)target);
    }

    private void AddPOV(CameraFollower cameraFollower) {
        var povs = cameraFollower.CameraPOV_s;
        var newArray = new CameraPOVData[povs.Length + 1];
        for (int i = 0; i < povs.Length; i++) {
            newArray[i] = povs[i];
        }

        var camera = cameraFollower.CameraTransform.GetComponent<Camera>();
        newArray[povs.Length] = new CameraPOVData() {
            CameraLocalPosition = cameraFollower.CameraTransform.parent.localPosition,
            CameraLocalRotation = cameraFollower.CameraTransform.parent.localRotation,
            FovOrOrhograpicSize = camera.orthographic ? camera.orthographicSize : camera.fieldOfView,
        };
        cameraFollower.CameraPOV_s = newArray;
    }

    private void RefreshPOV(int index, CameraFollower cameraFollower) {
        cameraFollower.CameraPOV_s[index].CameraLocalPosition = cameraFollower.CameraTransform.parent.localPosition;
        cameraFollower.CameraPOV_s[index].CameraLocalRotation = cameraFollower.CameraTransform.parent.localRotation;
        var camera = cameraFollower.CameraTransform.GetComponent<Camera>();
        if (camera.orthographic) {
            cameraFollower.CameraPOV_s[index].FovOrOrhograpicSize = camera.orthographicSize;
        }
        else {
            cameraFollower.CameraPOV_s[index].FovOrOrhograpicSize = camera.fieldOfView;
        }
    }

    private void PreviewPOV(int index, CameraFollower cameraFollower) {
        var POVs = cameraFollower.CameraPOV_s;
        cameraFollower.CameraTransform.parent.localPosition = POVs[index].CameraLocalPosition;
        cameraFollower.CameraTransform.parent.localRotation = POVs[index].CameraLocalRotation;
        var camera = cameraFollower.CameraTransform.GetComponent<Camera>();
        if (camera.orthographic) {
            camera.orthographicSize = POVs[index].FovOrOrhograpicSize;
        }
        else {
            camera.fieldOfView = POVs[index].FovOrOrhograpicSize;
        }
    }
}
