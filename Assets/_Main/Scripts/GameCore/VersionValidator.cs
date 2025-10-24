using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Game.Core {

    /// <summary>
    /// Validates game version and changes save data if needed
    /// </summary>
    public class VersionValidator {
        private SaveData _curSave;

        private static bool _newVersion = false;
        /// <summary>
        /// Was game updated on before launch?<br>
        /// Use this property to handle old data in other scripts</br>
        /// </summary>
        public static bool NewVersion => _newVersion;

        public VersionValidator(SaveData currentSave) {
            _curSave = currentSave;

#if UNITY_EDITOR
            OnVersionUpdated();
#endif

            if (_curSave.GameVersion != Application.version) {
                OnVersionUpdated();
            }
        }

        private void OnVersionUpdated() {
            // check and change if needed, depends on game version app dates
            _newVersion = true;

            try {
                BackupOldSave();
            }
            catch (Exception e) {
                Debug.LogException(e);
            }

            //_curSave.Smth = new Smth();
            // update save game version

            _curSave.GameVersion = Application.version;
        }

        #region Backup Old Save
        private void BackupOldSave() {
            if (_curSave.GameVersion == Application.version || string.IsNullOrEmpty(_curSave.GameVersion)) {
                return;
            }

#if UNITY_EDITOR
            var oldSave = new BackupSaveFile(_curSave.GameVersion, "");
            var newSave = new BackupSaveFile(Application.version, "");
            if (newSave.CompareTo(oldSave) < 0) { // don't backup if version downgraded. We should backup only on version increment
                // this means we are testing old save to new transition 90%
                return;
            }

            ForceBackupOldSaveEditor();

            // force save new version so it will not enter it second time in play mode in a row
            _curSave.GameVersion = Application.version;
            SaveSystem.Instance.SaveData();
#endif

            string path = Path.Combine(SaveSystem.PathToFiles, SaveSystem.SaveDataBackupFolderName, _curSave.GameVersion + " " + SaveSystem.FileName);
            SaveSystem.SaveData(_curSave, path, new SaveJSONSerializer()); // JSON to read data by key if save is broken (todo implement read)
        }

#if UNITY_EDITOR
        public bool ForceBackupOldSaveEditor(SaveData save = null) {
            if (save == null) save = _curSave;
            // also save backup file in project folder. It can be used to swap for test then
            var projectPathForSaveBackups = Path.Combine(Application.dataPath, "SaveBackups", save.GameVersion + " " + SaveSystem.FileName);
            if (File.Exists(projectPathForSaveBackups) == false || UnityEditor.EditorUtility.DisplayDialog("Confirm Action",
                    $"Old save backup v{save.GameVersion} already exists (for test purposes).\nDo you want to override it with new save?",
                    "Yes","No")) {

                SaveSystem.SaveData(save, projectPathForSaveBackups);
                UnityEditor.AssetDatabase.Refresh();
                Debug.LogWarning("Save data old version backup...");
                return true;
            }
            return false;
        }
#endif

        private void RemoveAllBackupSaves() {
            var backupSaves = GetBackupSaves();
            if (backupSaves != null) {
                foreach (var save in backupSaves) {
                    File.Delete(save.PathToFile); // delete other backups to have only one old top version
                }
            }
        }

        private BackupSaveFile GetTopVersionBackupSave() {
            var backupSaves = GetBackupSaves();
            if (backupSaves.Count > 0) {
                backupSaves.Sort((x, y) => -x.CompareTo(y));
                return backupSaves[0];
            }
            return null;
        }

        private List<BackupSaveFile> GetBackupSaves() {
            string backupFolder = Path.Combine(SaveSystem.PathToFiles, SaveSystem.SaveDataBackupFolderName);

            if (Directory.Exists(backupFolder) == false) {
                return null;
            }
            string[] files = Directory.GetFiles(backupFolder);
            if (files.Length == 0) return null;

            List<BackupSaveFile> backupVersions = new List<BackupSaveFile>(files.Length);

            foreach (var file in files) {
                backupVersions.Add(new BackupSaveFile() {
                    Version = Path.GetFileName(file).Replace(SaveSystem.FileName, ""),
                    PathToFile = files[0]
                });
            }
            return backupVersions;
        }

        private BackupSaveFile GetBackupSave(string gameVersion) {
            string backupFolder = Path.Combine(SaveSystem.PathToFiles, SaveSystem.SaveDataBackupFolderName);

            if (Directory.Exists(backupFolder) == false) {
                return null;
            }
            string[] files = Directory.GetFiles(backupFolder);
            if (files.Length == 0) return null;

            if (files.Length > 0) {
                foreach (var file in files) {
                    if (file.Contains(gameVersion)) {
                        return new BackupSaveFile(gameVersion, file);
                    }
                }
            }
            return null;
        }

        private class BackupSaveFile : IComparable<BackupSaveFile> {
            public string Version;
            public string PathToFile;

            public BackupSaveFile() { }
            public BackupSaveFile(string version, string pathToFile) {
                Version = version;
                PathToFile = pathToFile;
            }

            public int CompareTo(BackupSaveFile other) {
                // remove a-zA-Z symbols so we can compare only digits
                string meVersion = System.Text.RegularExpressions.Regex.Replace(Version, "[a-zA-Z]", "");
                string otherVerion = System.Text.RegularExpressions.Regex.Replace(other.Version, "[a-zA-Z]", "");

                string[] meNumbers = meVersion.Split(".");
                string[] otherNumbers = otherVerion.Split(".");

                if (meNumbers.Length == otherNumbers.Length || meNumbers.Length < otherNumbers.Length) {
                    return Compare(meNumbers, otherNumbers);
                }
                else {
                    int otherHigher = Compare(otherNumbers, meNumbers);
                    if (otherHigher > 0) return -1;
                    else if (otherHigher < 0) return 1;
                    return 0;
                }
            }

            /// <returns>1 if <paramref name="numbersLessOrEqualGroup"/> higher version and 0 if equal and -1 if lower</returns>
            private int Compare(string[] numbersLessOrEqualGroup, string[] secondGroup) {
                try {
                    for (int i = 0; i < numbersLessOrEqualGroup.Length; i++) {
                        int difference = int.Parse(numbersLessOrEqualGroup[i]) - int.Parse(secondGroup[i]);
                        if (difference != 0) {
                            return difference > 0 ? 1 : -1;
                        }
                    }
                }
                catch {
                    return 0;
                }
                return secondGroup.Length > numbersLessOrEqualGroup.Length ? -1 : 0;
            }

            public override string ToString() {
                return $"Version: {Version}\nFile: {PathToFile}";
            }
        }
        #endregion
    }
}