using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Events;

namespace Game.EditorTools {
    public static class EditorPackageAdd {
        private static AddRequest _nugetPackageRequest;
        private const string DIALOG_TITLE = "Package Add";

        private static ManifestJson _manifest;
        private static string _manifestPath;
        private static UnityAction _onPackageAdded;
        private const string OpenUPMPackageScope = "package.openupm.com";

        private readonly static string FixConsoleErrorsMessage = "You have compile errors - fix them and try again!";

        //[MenuItem("Packages/Add _package_name_")]
        //private static void Add_package_name_() {
        //    AddPackage("_dependency_");
        //}

        [MenuItem("Packages/Add Unity AI Navigation")]
        private static void AddUnityNavigationAI() {
            AddPackage("com.unity.ai.navigation");
        }

        [MenuItem("Packages/Add Unity Remote Config")]
        private static void AddUnityRemoteConfig() {
            AddPackage("com.unity.remote-config");
        }

        [MenuItem("Packages/Add Soft Mask UGUI")]
        private static void AddSoftMaskUGUI() {
            AddOpenUPMPackage(new OpenUPMPackage("com.coffee.softmask-for-ugui",
                                                 "3.2.2"));
        }

        [MenuItem("Packages/Add UIParticle")]
        private static void AddUIParticle() {
            AddOpenUPMPackage(new OpenUPMPackage("com.coffee.ui-particle",
                                                 "4.10.7"));
        }

        [MenuItem("Packages/Add UIEffect")]
        private static void AddUIEffect() {
            AddOpenUPMPackage(new OpenUPMPackage("com.coffee.ui-effect",
                                                 "5.3.1"));
        }

        [MenuItem("Packages/Add Mackysoft Serializereference Extensions")]
        private static void AddMackysoftSerializereferenceExtensions() {
            AddOpenUPMPackage(new OpenUPMPackage("com.mackysoft.serializereference-extensions",
                                                 "1.6.1"));
        }

        [MenuItem("Packages/Add Unity In App Purchasing")]
        private static void AddUnityInAppPurchasing() {
            _onPackageAdded += () => { 
                AddPackage("com.unity.services.analytics");
                _onPackageAdded = null;
            };
            AddPackage("com.unity.purchasing");
        }

        public static bool IsRegistryAdded(string scopedRegistryName) {
            if (_manifest.scopedRegistries == null || _manifest.scopedRegistries.Count == 0) {
                return false;
            }

            foreach (var scope in _manifest.scopedRegistries) {
                if (scope.name == scopedRegistryName) {
                    return true;
                }
            }
            return false;
        }

        public static bool IsScopePackageAdded(string registryName, string packageScope) {
            if (_manifest.scopedRegistries == null || _manifest.scopedRegistries.Count == 0) {
                return false;
            }

            foreach (var scope in _manifest.scopedRegistries) {
                if (scope.name == registryName) {
                    return ContainsScope(scope, packageScope);
                }
            }
            return false;
        }

        public static bool ContainsScope(ScopedRegistry registry, string scope) {
            if (registry.scopes == null || registry.scopes.Length == 0) {
                return false;
            }

            foreach (string addedScope in registry.scopes) {
                if (addedScope == scope) {
                    return true;
                }
            }
            return false;
        }

        private static void AddScopedRegistry(ScopedRegistry registry) {
            if (IsRegistryAdded(registry.name)) {
                EditorUtility.DisplayDialog(DIALOG_TITLE, $"Registry {registry.name} is already added!", "Ok");
            }

            _manifest.scopedRegistries.Add(registry);
            SaveChangesToManifest();
        }

        private static void AddOpenUPMPackage(OpenUPMPackage package) {
			if (EditorUtility.scriptCompilationFailed) {
				EditorUtility.DisplayDialog(DIALOG_TITLE,
                FixConsoleErrorsMessage, "Ok");
                return;
			}
			
            LoadManifest();
            if (IsRegistryAdded(package.scopedRegistry.name) == false) {
                AddOpenUPMScope();
            }

            if (IsScopePackageAdded(OpenUPMPackageScope, package.scopedRegistry.scopes[0])) {
                EditorUtility.DisplayDialog(DIALOG_TITLE, $"Package {package.dependencyName} is already added!", "Ok");
                return;
            }

            _manifest.dependencies.Add(package.dependencyName, package.dependencyVersion);
            _manifest.AppendScopeToRegistry(package.scopedRegistry.name, package.scopedRegistry.scopes[0]);
            SaveChangesToManifest();

            Client.Resolve();
            _manifest = null;
        }

        private static void AddPackage(string dependency) {
			if (EditorUtility.scriptCompilationFailed) {
				EditorUtility.DisplayDialog(DIALOG_TITLE,
                FixConsoleErrorsMessage, "Ok");
                return;
			}
			
            LoadManifest();

            if (_manifest.IsPackageAdded(dependency)) {
                EditorUtility.DisplayDialog(DIALOG_TITLE, $"dependency is already added!", "Ok");
                _onPackageAdded?.Invoke();
                _manifest = null;
                return;
            }
            // Add a package to the project
            _nugetPackageRequest = Client.Add(dependency);
            EditorApplication.update += PackageLoadProgress;
            _manifest = null;
        }

        private static void PackageLoadProgress() {
            if (_nugetPackageRequest == null) {
                EditorApplication.update -= PackageLoadProgress;
            }

            if (_nugetPackageRequest.IsCompleted) {
                if (_nugetPackageRequest.Status == StatusCode.Success) {
                    Debug.Log("EditorTools: Package Installed: " + _nugetPackageRequest.Result.packageId);
                }
                else if (_nugetPackageRequest.Status >= StatusCode.Failure) {
                    Debug.Log(_nugetPackageRequest.Error.message);
                }

                EditorApplication.update -= PackageLoadProgress;
                _nugetPackageRequest = null;
                _onPackageAdded?.Invoke();
            }
        }

        private static void LoadManifest() {
            if (_manifest == null) {
                _manifestPath = Path.Combine(Application.dataPath, "..", "Packages/manifest.json");
                _manifest = JsonConvert.DeserializeObject<ManifestJson>(File.ReadAllText(_manifestPath));
            }

            if (_manifest == null) {
                Debug.LogError("Failed to find manifest.json!");
                return;
            }
        }

        private static void SaveChangesToManifest() {
            File.WriteAllText(_manifestPath, JsonConvert.SerializeObject(_manifest, Formatting.Indented));
        }

        private static void AddOpenUPMScope() {
            AddScopedRegistry(new OpenUPMPackage().scopedRegistry);
        }

        public class OpenUPMPackage {
            public ScopedRegistry scopedRegistry = new ScopedRegistry() {
                name = "package.openupm.com",
                url = "https://package.openupm.com",
                scopes = new string[0],
            };
            public string dependencyName;
            public string dependencyVersion;

            public OpenUPMPackage() { }

            public OpenUPMPackage(string packageName, string version) {
                scopedRegistry.scopes = new string[] { packageName };
                this.dependencyName = packageName;
                this.dependencyVersion = version;
            }
        }

        public class ScopedRegistry {
            public string name;
            public string url;
            public string[] scopes;
        }

        public class ManifestJson {
            public Dictionary<string, string> dependencies = new Dictionary<string, string>();
            public List<ScopedRegistry> scopedRegistries = new List<ScopedRegistry>();

            /// <returns>true if success</returns>
            public bool AppendScopeToRegistry(string registryName, string scope) {
                if (IsScopePackageAdded(registryName, scope)) {
                    return false;
                }
                foreach (var registry in scopedRegistries) {
                    if (registry.name == registryName) {
                        if (registry.scopes == null) {
                            registry.scopes = new string[] { scope };
                            return true;
                        }

                        var newScopes = new string[registry.scopes.Length + 1];
                        for (int i = 0; i < newScopes.Length - 1; i++) {
                            newScopes[i] = registry.scopes[i];
                        }
                        newScopes[newScopes.Length - 1] = scope;
                        registry.scopes = newScopes;
                    }
                }
                return true;
            }

            public bool IsPackageAdded(string packageDependencyName) {
                return dependencies.ContainsKey(packageDependencyName);
            }
        }
    }
}