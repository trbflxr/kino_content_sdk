using UnityEditor;
using UnityEngine;

namespace Editor {
  public class AuthorMeta : ScriptableObject {
    public const string ASSET_NAME = "__author_meta.asset"; 
    private const int SID_SIZE = 17;

    [Tooltip("Author's name")]
    public string Name;

    [Tooltip("Author's SteamID, look for a button if you don't know how to get it")]
    public ulong SteamId;

    [Tooltip("Author's DiscordID, look for a button if you don't know how to get it")]
    public ulong DiscordId;

    public bool Validate() {
      if (string.IsNullOrWhiteSpace(Name)) {
        Debug.LogError("Kino: Author's name can be empty");
        return false;
      }

      if (SteamId == 0 || SteamId.ToString().Length != SID_SIZE) {
        Debug.LogError($"Kino: Invalid SteamID: '{SteamId}', it have to be 64bit 17 digit number");
        return false;
      }

      if (DiscordId == 0) {
        Debug.LogError("Kino: Invalid DiscordID");
        return false;
      }

      return true;
    }

    public static AuthorMeta GetInstance() {
      var guids = AssetDatabase.FindAssets($"t:{nameof(AuthorMeta)}");
      if (guids == null || guids.Length == 0) {
        Debug.LogError("Kino: Unable to locate AuthorMeta asset");
        return null;
      }

      if (guids.Length > 1) {
        Debug.LogError("Kino: Multiple AuthorMeta assets defined. Only one AuthorMeta should be defined in the project");
        return null;
      }

      string path = AssetDatabase.GUIDToAssetPath(guids[0]);
      return AssetDatabase.LoadAssetAtPath<AuthorMeta>(path);
    }
  }

  [CustomEditor(typeof(AuthorMeta))]
  public class AuthorMetaEditor : UnityEditor.Editor {
    public override void OnInspectorGUI() {
      base.OnInspectorGUI();
      var script = (AuthorMeta)target;

      GUILayout.Space(10.0f);

      if (GUILayout.Button("Validate")) {
        if (script.Validate()) {
          Debug.Log("Kino: Author's meta is valid");
        }
      }

      GUILayout.Space(10.0f);

      GUILayout.BeginHorizontal();
      if (GUILayout.Button("How to get SteamID")) {
        Utils.TryOpenLink("https://github.com/trbflxr/kino/blob/master/ContentCreation/GetSteamID.md");
      }

      if (GUILayout.Button("How to get DiscordID")) {
        Utils.TryOpenLink("https://github.com/trbflxr/kino/blob/master/ContentCreation/GetDiscordID.md");
      }

      GUILayout.EndHorizontal();
    }
  }
}