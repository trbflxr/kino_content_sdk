using System.IO;
using UnityEditor;
using UnityEngine;

namespace Editor {
  public class BuilderMeta : BaseBuilderMeta {
    public const string ASSET_NAME = "__builder_meta.asset";

    public const int TOOL_VERSION = 300;
    public const string PACK_EXT = ".knpp";
    private const string BUILD_DIR = "Build";

    [Tooltip("Specify the folder in which you want to put the compiled packs")]
    public string BuildFolder = string.Empty;

    [ReadOnly]
    public int Version = TOOL_VERSION;

    public override bool Validate() {
      OnValidate();
      return true;
    }

    private void OnValidate() {
      if (string.IsNullOrWhiteSpace(BuildFolder)) {
        BuildFolder = BUILD_DIR;
      }

      if (!Directory.Exists(BuildFolder)) {
        Directory.CreateDirectory(BuildFolder);
      }
    }

    public static BuilderMeta GetInstance() {
      var guids = AssetDatabase.FindAssets($"t:{nameof(BuilderMeta)}");
      if (guids == null || guids.Length == 0) {
        Debug.LogError("Kino: Unable to locate BuilderMeta asset");
        return null;
      }

      if (guids.Length > 1) {
        Debug.LogError("Kino: Multiple BuilderMeta assets defined. Only one BuilderMeta should be defined in the project");
        return null;
      }

      string path = AssetDatabase.GUIDToAssetPath(guids[0]);
      return AssetDatabase.LoadAssetAtPath<BuilderMeta>(path);
    }
  }
}