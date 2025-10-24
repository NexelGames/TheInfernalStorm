using UnityEditor;

/// <summary>
/// Changes some imported objects depends on their folder location
/// </summary>
class ProjectFolderPostprocessor : AssetPostprocessor {

	private const string _spritesPath = "Assets/_Main/Sprites";
	private const string _modelsPath = "Assets/_Main/Models";

    // texture asset preprocessor
    private void OnPreprocessTexture() {
		if (assetPath.StartsWith(_spritesPath)) {
			PreprocessSpriteFolder();
		}
	}

    private void PreprocessSpriteFolder() {
		TextureImporter importer = assetImporter as TextureImporter;
		if (importer) {
			importer.textureType = TextureImporterType.Sprite;
		}
	}

    private void OnPreprocessModel() {
        if (assetPath.StartsWith(_modelsPath)) {
			PreprocessModelFolder();
        }
    }

    private void PreprocessModelFolder() {
		ModelImporter importer = assetImporter as ModelImporter;
		if (importer) {
			importer.materialImportMode = ModelImporterMaterialImportMode.None;
        }
	}
}