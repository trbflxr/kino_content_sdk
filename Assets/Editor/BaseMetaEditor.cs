using UnityEditor;
using UnityEngine;

namespace Editor {
	public abstract class BaseMetaEditor<T> : UnityEditor.Editor where T : BaseEntryMeta<T> {
		private readonly bool canRefresh_;

		public BaseMetaEditor(bool canRefresh) {
			canRefresh_ = canRefresh;
		}

		public override void OnInspectorGUI() {
			var script = (T) target;

			if (canRefresh_) {
				if (GUILayout.Button("Refresh assets")) {
					script.RefreshEditor();
				}
			}

			if (GUILayout.Button("Validate all")) {
				script.Validate();
			}

			serializedObject.ApplyModifiedProperties();
		}

		protected void DrawProp(string propName, string propText = null) {
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