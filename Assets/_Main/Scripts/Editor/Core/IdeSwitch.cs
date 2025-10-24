using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Diagnostics;

public class IdeSwitch {

    [MenuItem("Assets/NotePad++")]
    private static void OpenExternalEditor() {
        var selected = Selection.activeObject;
        string sel = AssetDatabase.GetAssetPath(selected);

        ProcessStartInfo startInfo = new ProcessStartInfo("notepad++.exe");
        startInfo.WindowStyle = ProcessWindowStyle.Normal;
        startInfo.Arguments = @sel;
        Process.Start(startInfo);
    }
}