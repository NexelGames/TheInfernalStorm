using Alchemy.Inspector;
using UnityEngine;
using UnityEngine.Audio;

namespace Game.Audio {
    [CreateAssetMenu(fileName = "Sound Audio Clip Settings", menuName = "Game/Audio/Sound Audio Clip")]
    public class SoundAudioClip : ScriptableObject {
        public AudioClip AudioClip;
        public AudioMixerGroup MixerGroup;
        [Range(0f, 1f)] public float Volume = 1f;

        [Space]
        [Tooltip("Is sound fully 2D, false means it's 3D sound")]
        public bool Is2D = true;
        [HideIf(nameof(Is2D)), Range(0f, 5f)]public float DopplerLevel = 1;
        [HideIf(nameof(Is2D)), Range(0f, 360f)] public float Spread = 0;
        [HideIf(nameof(Is2D))] public AudioRolloffMode VolumeRolloff;
        [HideIf(nameof(Is2D)), Min(0)] public float MinDistance = 1;
        [HideIf(nameof(Is2D)), Min(0)] public float MaxDistance = 500;

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Assets/Create/Sound Audio Clips Selected", false, 220)]
        private static void CreateSoundAudioClipSO() {
            var selected = UnityEditor.Selection.objects;
            var createPath = EditorTools.EditorAssetManager.GetAssetFolder(selected[0]);
            var mixer = EditorTools.EditorAssetManager.LoadAsset<AudioMixer>("GameMixer", "Assets/_Main/Configs");

            for (int i = 0; i < selected.Length; i++) {
                var soName = ConvertAssetNameToScriptableName(selected[i].name);
                var so = EditorTools.EditorAssetManager.CreateScribtableObject<SoundAudioClip>(createPath, soName);
                if (mixer != null) AutoSetMixerGroup(so, mixer);
                so.AudioClip = (AudioClip)selected[i];
				UnityEditor.EditorUtility.SetDirty(so);
            }

            EditorTools.EditorAssetManager.SaveAndRefreshAssets();
        }

        [UnityEditor.MenuItem("Assets/Create/Sound Audio Clips Selected", true)]
        private static bool CreateSoundAudioClipSOValidation() {
            var selected = UnityEditor.Selection.objects;
            for (int i = 0; i < selected.Length; i++) {
                if (selected[i] is AudioClip == false) {
                    return false;
                }
            }
            return true;
        }

        private static void AutoSetMixerGroup(SoundAudioClip soundAudioClip, AudioMixer mixer) {
            try {
                if (soundAudioClip.name.EndsWith("_SFX")) {
                    soundAudioClip.MixerGroup = mixer.FindMatchingGroups("Sounds")[0];
                }
                else if (soundAudioClip.name.EndsWith("_Music")) {
                    soundAudioClip.MixerGroup = mixer.FindMatchingGroups("Music")[0];
                }
                else {
                    soundAudioClip.MixerGroup = mixer.FindMatchingGroups("Sounds")[0];
                }
            }
            catch (System.Exception e) {
                Debug.LogWarning(e.Message + "\n" + e.StackTrace);
            }
        }

        private static string ConvertAssetNameToScriptableName(string name) {
            string lowerName = name.ToLower();
            if (lowerName.Contains("_music") || lowerName.Contains("_m")) {
                int index = lowerName.LastIndexOf("_music");
                if (index == -1) {
                    index = lowerName.LastIndexOf("_m");
                }
                name = name.Substring(0, index) + "_Music";
            }
            else {
                int index = lowerName.LastIndexOf("_sfx");
                if (index != -1) {
                    name = name.Substring(0, index);
                }
                name = name + "_SFX";
            }
            return name;
        }
#endif
    }
}