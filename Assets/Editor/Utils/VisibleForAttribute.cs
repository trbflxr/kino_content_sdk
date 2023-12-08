using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Editor {
  [AttributeUsage(AttributeTargets.Field)]
  public abstract class VisibleForAttribute : PropertyAttribute {
    public PartType Type { get; }

    protected VisibleForAttribute(PartType type) {
      Type = type;
    }
  }

  [AttributeUsage(AttributeTargets.Field)]
  public class RangeVisibleForAttribute : VisibleForAttribute {
    public float Min { get; }
    public float Max { get; }

    public RangeVisibleForAttribute(float min, float max, PartType type) : base(type) {
      Min = min;
      Max = max;
    }
  }

  [CustomPropertyDrawer(typeof(RangeVisibleForAttribute))]
  public class VisibleForDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
      var attrib = (RangeVisibleForAttribute)attribute;

      var rootPath = Path.GetFileNameWithoutExtension(property.propertyPath);
      var typePath = $"{rootPath}.Type";

      var objType = property.serializedObject.FindProperty(typePath);

      var holderType = PartType.Undefined;
      if (objType != null) {
        holderType = (PartType)objType.intValue;
      }

      if (attrib.Type != holderType) {
        return;
      }

      EditorGUI.Slider(position, property, attrib.Min, attrib.Max, label);
    }
  }
}