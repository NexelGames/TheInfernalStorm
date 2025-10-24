using UnityEngine;
using System.IO;
using UnityEngine.Events;
using Alchemy.Inspector;

namespace Game.Core {
    // this system takes some parts from BayatGames.SaveGameFree package: 
    // https://assetstore.unity.com/packages/tools/input-management/save-game-free-gold-update-81519

    /// <summary>
    /// Main class for game save/load operations
    /// </summary>
    public class SaveSystem : MonoBehaviour {
        #region SINGLETON PATTERN    
        private static SaveSystem _instance;
        public static SaveSystem Instance {
            get {
                if (_instance == null) {
                    _instance = FindAnyObjectByType<SaveSystem>(FindObjectsInactive.Include);

                    if (_instance == null) {
                        GameObject container = new GameObject("SaveSystem");
                        _instance = container.AddComponent<SaveSystem>();
                    }
                    else if (_instance.Inited == false) {
                        _instance.LoadData();
                    }
                }
                return _instance;
            }
        }
        #endregion

        /// <summary>
        /// Core object of saveSystem that you will refer to.<br>
        /// By refering this property you can read/change data. <br>
        /// </br>Data will be saved automaticaly on OnApplicationFocus() where focus lost</br>
        /// </summary>
        public SaveData Data => _data;
        public GameSettings GameSettings => _gameSettings;

        /// <summary>
        /// arg1 - data before save, you can add or override information before it will be saved
        /// </summary>
        public static UnityEvent<SaveData> OnDataSave = new UnityEvent<SaveData>();

        [SerializeField] private bool _verboseLogging = false;

        public static ISaveSerializer BinarySerializer = new SaveBinarySerializer();
        public static ISaveSerializer JsonSerializer = new SaveJSONSerializer();

        [Blockquote("Don't override default variables in inspector,\n" +
            "you need to override them in script.\n" +
            "You can override values in inspector for test purposes runtime."), SerializeField]
        private SaveData _data;
        private GameSettings _gameSettings;

        private bool Inited => string.IsNullOrEmpty(_savePath) == false;

        public static string PathToFiles => Path.Combine(Application.persistentDataPath, "GameData");
        public static readonly string SaveDataBackupFolderName = "BackupGameData";

        /// <summary>
        /// Save data file name in <see cref="PathToFiles"/>
        /// </summary>
        public static readonly string FileName = "gameData.okd";
        public static readonly string SettingsFileName = "gameSettings.json";

        /// <summary>
        /// temp path with save filename combined to load/save/delete data
        /// </summary>
        private string _savePath = null;
        private string _gameSettingsPath = null;


        private void Awake() {
            if (_instance == null) {
                _instance = this;
            }
            else if (_instance != this) {
                Destroy(gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);

            if (Inited == false) {
                LoadData();
            }
        }

        /// <summary>
        /// You should not do it manualy, only if you know what you do (game loads automaticaly as soon as <see cref="Data"/> called once)
        /// </summary>
        public void LoadData() {
            _savePath = Path.Combine(PathToFiles, FileName);
            _gameSettingsPath = Path.Combine(PathToFiles, SettingsFileName);

            if (_verboseLogging) {
                print("save path: " + _savePath);
            }

            if (!File.Exists(_savePath))   //means there is no saves -> new game
            {
                if (_verboseLogging) {
                    print("New save was created! No save file was found.");
                }

                var directoryPath = Path.GetDirectoryName(_savePath);
                if (!Directory.Exists(directoryPath)) {
                    Directory.CreateDirectory(directoryPath);
                }

                FileStream file = File.Create(_savePath);
                _data = new SaveData() { GameVersion = string.Empty };
                BinarySerializer.Serialize(_data, file, null);
                file.Close();
            }
            else {
                if (_verboseLogging) {
                    print("Found save file -> read data.");
                }

                FileStream file = File.OpenRead(_savePath);
                _data = BinarySerializer.Deserialize<SaveData>(file, null);
                file.Close();
            }

            if (!File.Exists(_gameSettingsPath)) {
                FileStream file = File.Create(_gameSettingsPath);
                _gameSettings = new();
                JsonSerializer.Serialize(_gameSettings, file, null);
                file.Close();
            }
            else {
                FileStream file = File.OpenRead(_gameSettingsPath);
                _gameSettings = JsonSerializer.Deserialize<GameSettings>(file, null);
                file.Close();
            }

            if (_data == null) { // possible due to deserelization errors
                _data = new SaveData() { GameVersion = string.Empty };
            }

            if (_gameSettings == null) { // possible due to deserelization errors
                _gameSettings = new();
            }
        }

        /// <param name="filePath">path to file in <see cref="PathToFiles"/></param>
        /// <param name="serializer">leave null to use binary</param>
        public static T LoadData<T>(string filePath, ISaveSerializer serializer = null) {
            if (serializer == null) {
                serializer = BinarySerializer;
            }

            filePath = Path.Combine(PathToFiles, filePath);

            if (File.Exists(filePath)) {
                FileStream file = File.OpenRead(filePath);
                var data = serializer.Deserialize<T>(file, null);
                file.Close();
                return data;
            }
            else {
                return default;
            }
        }

#if UNITY_EDITOR
        private void Update() {
            if (Input.GetKeyDown(KeyCode.Delete)) {
                DeleteSaveFile();
            }
        }
#endif

#if UNITY_EDITOR
        [Button, LabelText("Log additional save data")]
        private void LogAdditionalSaveData() => _data.LogAdditionalData();
#endif

        /// <summary>
        /// You should not do it manualy, only if you know what you do (game saves automaticaly onFocus change event)
        /// </summary>
        [Button, LabelText("Save Game")]
        public void SaveData() {
            if (!File.Exists(_savePath)) {
                Debug.LogError("Missing current Save path!");
                return;
            }

            OnDataSave?.Invoke(_data);

            FileStream file = File.Create(_savePath);
            BinarySerializer.Serialize(_data, file, null);
            file.Close();
        }

        [Button, LabelText("Save Settings")]
        public void SaveSettings() {
            if (!File.Exists(_gameSettingsPath)) {
                Debug.LogError("Missing current Settings path!");
                return;
            }

            FileStream file = File.Create(_gameSettingsPath);
            JsonSerializer.Serialize(_gameSettings, file, null);
            file.Close();
        }

        /// <param name="filePath">path to file in <see cref="PathToFiles"/></param>
        /// <param name="serializer">leave null to use binary</param>
        public static void SaveData<T>(T data, string filePath, ISaveSerializer serializer = null) {
            if (serializer == null) {
                serializer = BinarySerializer;
            }

            filePath = Path.Combine(PathToFiles, filePath);

            FileStream file;
            if (!File.Exists(filePath)) {
                var directoryPath = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directoryPath)) {
                    Directory.CreateDirectory(directoryPath);
                }
                file = File.Create(filePath);
                serializer.Serialize(data, file, null);
                file.Close();
                return;
            }

            file = File.Create(filePath);
            serializer.Serialize(data, file, null);
            file.Close();
        }

        public static void ClearFileContent(string pathToFile) {
            FileStream file;
            if (!File.Exists(pathToFile)) {
                return;
            }
            file = File.Create(pathToFile);
            file.SetLength(0);
            file.Close();
        }

        /// <summary>
        /// returns all files with .ftr extention in the Application persistent data path sorted descending
        /// </summary>
        public string[] GetSaveNames() {
            DirectoryInfo saveDir = new DirectoryInfo(PathToFiles);
            FileInfo[] saveFiles = saveDir.GetFiles("*.sqw"); // Getting files

            string[] fileNames = new string[saveFiles.Length];
            for (int i = 0; i < saveFiles.Length; i++) {
                fileNames[i] = saveFiles[saveFiles.Length - 1 - i].Name;
            }

            return fileNames;
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Tools/Delete Save", priority = -999)]
#endif
        [Button, LabelText("Delete Save")]
        private static void DeleteSaveFile() {
            DeleteSaveFile(FileName);
            DeleteSaveFile("Bunker.json");
            DeleteSaveFile("Player.json");
            DeleteSaveFile("Quests.json");
            DeleteSaveFile("World.json");
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Tools/Open Saves in Explorer", priority = -997)]
        private static void OpenFilesLocation() {
            UnityEditor.EditorUtility.RevealInFinder(Path.Combine(PathToFiles, FileName));
        }
#endif

        /// <param name="filePath">path to file in <see cref="PathToFiles"/></param>
        public static void DeleteSaveFile(string filePath) {
            var savePath = Path.Combine(PathToFiles, filePath);
            if (File.Exists(savePath)) {
                File.Delete(savePath);
                Debug.LogWarning($"{filePath} was successfuly deleted");
            }
            else {
                Debug.LogError($"There is no {filePath} to delete");
            }
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Tools/Backup Save", priority = -790)]
        private static void BackupSaveEditor() {
            var savePath = Path.Combine(PathToFiles, FileName);
            if (File.Exists(savePath)) {
                VersionValidator versionValidator;
                if (Application.isPlaying) {
                    versionValidator = new VersionValidator(_instance.Data);
                }
                else {
                    versionValidator = new VersionValidator(LoadData<SaveData>(savePath));
                }

                if (versionValidator.ForceBackupOldSaveEditor()) {
                    Debug.LogWarning("Save file was successfuly backup into Project");
                }
            }
            else {
                Debug.LogError("There is no save file to backup");
            }
        }

        private static void BackupLoadEditor(string version) {
            var backupPath = Path.Combine(Application.dataPath, "SaveBackups", version + " " + FileName);
            if (File.Exists(backupPath)) {
                var logEnabled = Debug.unityLogger.logEnabled;
                Debug.unityLogger.logEnabled = false;
                DeleteSaveFile();
                Debug.unityLogger.logEnabled = logEnabled;

                SaveData save = LoadData<SaveData>(backupPath);
                if (Application.isPlaying) {
                    _instance._data = save;
                    UnityEditor.EditorApplication.isPlaying = false;
                    Debug.LogWarning("Playmode stopped to apply backup changes");
                }

                FileStream file = File.Create(Path.Combine(PathToFiles, FileName));
                BinarySerializer.Serialize(save, file, null);
                file.Close();

                Debug.LogWarning("Backup file was successfuly loaded");
            }
            else {
                Debug.LogError($"There is no backup file for {version} app version");
            }
        }

        [FoldoutGroup("Backup"), Button, LabelText("Backup Save")]
        private void BackupSaveEditorButton() => BackupSaveEditor();

        [FoldoutGroup("Backup"), Button, LabelText("Backup Load")]
        private void BackupLoadEditorButton(string version) => BackupLoadEditor(version);

        [UnityEditor.MenuItem("Tools/Backup Load", priority = -791)]
        private static void BackupLoadEditor() {
            BackupLoadEditor(Application.version);
        }
#endif

        void OnApplicationFocus(bool hasFocus) {
            if (!hasFocus) {
                if (_verboseLogging) {
                    print("data was saved");
                }
                SaveData();
                SaveSettings();
            }
        }
    }
}