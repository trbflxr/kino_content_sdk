﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace Editor {
  public enum PackType {
    Undefined = 0,
    Wheels,
    InteriorParts,
    CarParts
  }

  public enum PartType {
    Undefined = 0,
    Wheel,
    SteeringWheel,
    Handbrake,
    ShifterSequential,
    ShifterHPattern,
    SeatLeft,
    SeatRight,
    BumperFront,
    BumperRear,
    Skirts,
    Doors,
    Mirrors,
    Bonnet,
    Trunk,
    Spoiler,
    Roof,
    Exhaust,
    Cage,
    // LightsFront,
    // LightsRear
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

      public float SteeringWheelSize;
    }

    [HideInInspector]
    public string name = "unknown";

    [Tooltip("Type of the part, have to match with the pack type")]
    public PartType Type = PartType.Undefined;

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

    [Tooltip("Steering wheel size. Needed for correct driver animation")]
    [RangeVisibleFor(0.05f, 0.4f, PartType.SteeringWheel)]
    public float SteeringWheelSize = 0.3f;

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
      public int TargetCarId;
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
    public PackType Type = PackType.Undefined;

    [HideInInspector]
    [Tooltip("Specify the car the part is intended for")]
    public int TargetCarId = -1;

    [Tooltip("Pack name. Have to be lowercase with only Latin letters and digits, without any spaces")]
    public string PackName = "parts_pack";

    [Tooltip("[Optional] Category name. Will be displayed on the pack card in the game")]
    public string CategoryName = "kino";

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
        TargetCarId = TargetCarId,
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
        if (Type == PackType.Wheels && part.Type != PartType.Wheel) {
          Debug.LogError($"Kino: Unable to add part {part.name} ({part.Type}), because pack type is different: {Type}");
          continue;
        }

        var partMeta = new PartMeta.Proxy {
          Type = (int)part.Type,
          FilePath = part.FilePath,
          IconPath = string.Empty,
          Id = part.Id,
          ReplacementId = part.ReplacementId,
          SteeringWheelSize = part.SteeringWheelSize
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
      valid_ = false;

      if (Type == PackType.Undefined) {
        Debug.LogWarning($"Kino: Invalid pack type selected: {Type}");
        return;
      }

      if (Type == PackType.CarParts && TargetCarId <= 0) {
        Debug.LogWarning($"Kino: Invalid target car ID selected: {TargetCarId}");
        return;
      }

      if (Parts == null) {
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

        if (part.Type == PartType.Undefined) {
          Debug.LogWarning($"Kino: Part {part.name} has {part.Type} type selected");
          valid_ = false;
        }

        bool isWheel = part.Type == PartType.Wheel;
        bool isInterior = IsInteriorPart(part.Type);
        bool isExterior = IsExteriorPart(part.Type);

        if ((Type == PackType.Wheels && !isWheel)
            || (Type == PackType.InteriorParts && !isInterior)
            || (Type == PackType.CarParts && !isExterior)) {
          Debug.LogWarning($"Kino: Part {part.name} ({part.Type}) is incompatible with pack {PackName} ({Type}), because of type difference");
          valid_ = false;
        }

        if (!part.Prefab) {
          Debug.LogWarning($"Kino: Prefab is not set for part {part.Id} ({part.Type})");
          valid_ = false;
        }
      }
    }

    private bool IsExteriorPart(PartType type) {
      return type != PartType.Wheel && !IsInteriorPart(type);
    }

    private bool IsInteriorPart(PartType type) {
      switch (type) {
        case PartType.SteeringWheel:
        case PartType.Handbrake:
        case PartType.ShifterSequential:
        case PartType.ShifterHPattern:
        case PartType.SeatLeft:
        case PartType.SeatRight:
          return true;
      }

      return false;
    }
  }

  [CustomEditor(typeof(PartPackMeta))]
  public class PartPackMetaEditor : UnityEditor.Editor {
    public override void OnInspectorGUI() {
      var script = (PartPackMeta)target;

      DrawProp("Id");
      DrawProp("Type");

      if (script.Type == PackType.CarParts) {
        DrawProp("TargetCarId", "Target car ID");
      }

      DrawProp("PackName");
      DrawProp("CategoryName");
      DrawProp("Description");
      DrawProp("PackIcon");
      DrawProp("Version");
      DrawProp("Parts");

      if (GUILayout.Button("Validate all parts")) {
        script.ForceValidate();
      }

      serializedObject.ApplyModifiedProperties();
    }

    private void DrawProp(string propName, string propText = null) {
      var prop = serializedObject.FindProperty(propName);
      if (prop == null) {
        return;
      }

      if (string.IsNullOrWhiteSpace(propText)) {
        propText = propName;
      }

      EditorGUILayout.PropertyField(prop, new GUIContent(propText, prop.tooltip));
    }
  }
}