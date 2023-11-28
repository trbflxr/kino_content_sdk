using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace Editor {
  public enum PartType {
    Wheels = 1,
    // SteeringWheel = 2,
    // Spoiler = 3
  }

  [Serializable]
  public class PartMeta {
    [Serializable]
    public class Proxy {
      public int Type;
      public int Id;
      public int ReplacementId;
      public string FilePath;
      public string IconPath;
    }

    [HideInInspector]
    public string name = "unknown";

    [Tooltip("Type of the part, have to match with the pack type")]
    public PartType Type;

    [Tooltip("Part prefab, can't be null")]
    public GameObject Prefab;

    [Tooltip("[Optional] Part icon. Will be displayed on part card in the game")]
    public Texture2D Icon;

    [HideInInspector]
    public string FilePath = string.Empty;

    [Tooltip("[Optional] Default CarX part ID that will be displayed for all players who don't have this pack")]
    public int ReplacementId;

    [ReadOnly]
    [Tooltip("Part 'unique' ID")]
    public int Id;

    public void Validate(bool forceRegenerateId) {
      if (Prefab) {
        name = Prefab.name;
        FilePath = AssetDatabase.GetAssetPath(Prefab);
      }

      name = Prefab ? Prefab.name : "unknown";
      if (forceRegenerateId || Id == 0) {
        Id = Utils.GetId();
      }
    }
  }

  [CreateAssetMenu(fileName = "__meta", menuName = "Kino/Create car parts pack meta", order = 1)]
  public class PartPackMeta : ScriptableObject {
    [Serializable]
    public class Proxy {
      public ulong Id;
      public int Type;
      public string AuthorName;
      public string Name;
      public string CategoryName;
      public string Description;
      public string PackIcon;
      public int Version;

      public List<PartMeta.Proxy> Parts;
    }

    public const string PACK_META_RESERVED_NAME = "__pack_meta.txt";

    public bool SelectedToBuild = true;

    [ReadOnly]
    [Tooltip("Pack unique ID")]
    public ulong Id = Utils.GenerateUniqueId();

    [Tooltip("Pack parts type")]
    public PartType Type = PartType.Wheels;

    [Tooltip("Pack name. Have to be lowercase with only Latin letters and digits, without any spaces")]
    public string PackName = "parts_pack";

    [Tooltip("[Optional] Category name. Will be displayed on the pack card in the game")]
    public string CategoryName = string.Empty;

    [TextArea(4, 20)]
    [Tooltip("Pack description")]
    public string Description = string.Empty;

    [Tooltip("[Optional] Part icon. Will be displayed on pack card in the game")]
    public Texture2D PackIcon = null;

    [Tooltip("Pack version")]
    public int Version = 100;

    public List<PartMeta> Parts;

    private bool valid_;

    public Proxy GetProxyMeta() {
      var meta = new Proxy {
        Id = Id,
        Type = (int)Type,
        Name = PackName,
        CategoryName = CategoryName,
        Version = Version,
        Description = Description,
        PackIcon = string.Empty,
        Parts = new List<PartMeta.Proxy>()
      };

      if (Parts == null) {
        return null;
      }

      if (PackIcon) {
        meta.PackIcon = AssetDatabase.GetAssetPath(PackIcon);
      }

      foreach (var part in Parts) {
        if (part.Type != Type) {
          Debug.LogError($"Kino: Unable to add part {part.name} ({part.Type}), because pack type is different: {Type}");
          continue;
        }

        var partMeta = new PartMeta.Proxy {
          Type = (int)part.Type,
          FilePath = part.FilePath,
          IconPath = string.Empty,
          Id = part.Id,
          ReplacementId = part.ReplacementId
        };

        if (string.IsNullOrWhiteSpace(partMeta.FilePath)) {
          continue;
        }

        if (part.Icon) {
          partMeta.IconPath = AssetDatabase.GetAssetPath(part.Icon);
        }

        meta.Parts.Add(partMeta);
      }

      return meta;
    }

    public bool ForceValidate() {
      if (Parts == null) {
        return false;
      }

      Debug.Log($"Kino: Validating parts list {PackName} ({Type})");

      OnValidate();

      return valid_;
    }

    public string GetRoot() {
      string assetPath = AssetDatabase.GetAssetPath(this);
      var rootFolder = Path.GetDirectoryName(assetPath);
      if (string.IsNullOrWhiteSpace(rootFolder)) {
        Debug.LogError($"Kino: Unable to get {PackName} ({Type}) asset directory");
        return null;
      }

      return rootFolder;
    }

    public static IEnumerable<PartPackMeta> GetAllInstances() {
      var guids = AssetDatabase.FindAssets($"t:{nameof(PartPackMeta)}");

      var packsList = new List<PartPackMeta>();

      foreach (var guid in guids) {
        string path = AssetDatabase.GUIDToAssetPath(guid);
        var meta = AssetDatabase.LoadAssetAtPath<PartPackMeta>(path);

        if (meta) {
          packsList.Add(meta);
        }
      }

      return packsList;
    }

    private void OnValidate() {
      if (Parts == null) {
        valid_ = false;
        return;
      }

      valid_ = true;

      // really dirty way to generate id
      for (var i = 0; i < Parts.Count; ++i) {
        var part = Parts[i];
        part.Validate(false);

        int index = Parts.FindIndex(p => p.Id == part.Id);
        if (index != -1 && index != i) {
          part.Validate(true);
          if (part.Id == Parts[index].Id) {
            Thread.Sleep(1000);
            part.Validate(true);
          }
        }

        if (part.Type != Type) {
          Debug.LogWarning($"Kino: Part {part.name} ({part.Type}) is incompatible with pack {PackName} ({Type}), because of type difference");
          valid_ = false;
        }

        if (!part.Prefab) {
          Debug.LogWarning($"Kino: Prefab is not set for part {part.Id} ({part.Type})");
          valid_ = false;
        }
      }
    }
  }

  [CustomEditor(typeof(PartPackMeta))]
  public class PartPackMetaEditor : UnityEditor.Editor {
    public override void OnInspectorGUI() {
      base.OnInspectorGUI();
      var script = (PartPackMeta)target;

      if (GUILayout.Button("Validate all parts")) {
        script.ForceValidate();
      }
    }
  }
}