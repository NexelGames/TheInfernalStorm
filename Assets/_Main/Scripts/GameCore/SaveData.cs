using Game.Core;
using Game.JoystickUI;
using System.Collections.Generic;

/// <summary>
/// Main class that holds all savable data
/// </summary>
[System.Serializable]
public class SaveData {
    /// <summary>
    /// version check here <see cref="VersionValidator"/>
    /// </summary>
    public string GameVersion;

    /// <summary>
    /// Users currency, to manipulate use <see cref="CashManager"/> API
    /// Start values in Currency Settings config
    /// </summary>
    public Dictionary<Currencies, float> Currencies = null;

#if UNITY_EDITOR
    public void LogAdditionalData() {
        System.Text.StringBuilder info = new System.Text.StringBuilder("Save additional data: \n");

        info.Append(Newtonsoft.Json.JsonConvert.SerializeObject(Currencies) + "\n");

        UnityEngine.Debug.Log(info.ToString());
    }
#endif
}