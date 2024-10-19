using UnityEditor;
using UnityEngine;

namespace Editor {
	[CreateAssetMenu(fileName = "__map_meta", menuName = "Kino/Create Map meta", order = 4)]
	public class MapMeta : BaseEntryMeta<MapMeta> {
		[ReadOnly]
		[Tooltip("Objects pack unique ID")]
		public ulong Id = Utils.GenerateUniqueId();

		[Tooltip("Map scene")]
		public SceneAsset Scene;

		[Tooltip("Load screen. Only .PNG / .JPG")]
		public Texture2D LoadScreen;

		[Tooltip("Map version")]
		public int Version = 100;

		[HideInInspector]
		public string ScenePath = string.Empty;

		[HideInInspector]
		public string LoadScreenPath = string.Empty;

		public override bool Validate() {
			if (string.IsNullOrWhiteSpace(Name)) {
				Debug.LogError("Kino: Map name is not set");
				return false;
			}

			if (!Scene) {
				Debug.LogError($"Kino: Map scene for {Name} is not set");
				return false;
			}

			if (!LoadScreen) {
				Debug.LogError($"Kino: Map loadscreen for {Name} is not set");
				return false;
			}

			ScenePath = AssetDatabase.GetAssetPath(Scene);
			LoadScreenPath = AssetDatabase.GetAssetPath(LoadScreen);

			Debug.Log($"Kino: Map {Name} is valid");

			return true;
		}
	}

	[CustomEditor(typeof(MapMeta))]
	public class MapMetaEditor : BaseMetaEditor<MapMeta> {
		public MapMetaEditor() : base(false) { }

		public override void OnInspectorGUI() {
			var script = (MapMeta) target;

			EditorGUILayout.LabelField("Each field has a tooltip. Hold the cursor over it to see the tooltip.", EditorStyles.boldLabel);

			DrawProp("Id");
			DrawProp("Name");
			DrawProp("Scene");
			DrawProp("Version");

			script.LoadScreen = EditorGUILayout.ObjectField(new GUIContent("Load screen (.png / .jpg)"), script.LoadScreen, typeof(Texture2D), false) as Texture2D;

			base.OnInspectorGUI();
		}
	}
}