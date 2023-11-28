﻿using UnityEditor;
using UnityEngine;

namespace Editor {
  public class ReadOnlyAttribute : PropertyAttribute { }

  [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
  public class ReadOnlyDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
      var guiEnabled = GUI.enabled;
      GUI.enabled = false;
      EditorGUI.PropertyField(position, property, label);
      GUI.enabled = guiEnabled;
    }
  }
}