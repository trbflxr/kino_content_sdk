using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Editor {
	[CreateAssetMenu(fileName = "__garage_meta", menuName = "Kino/Create Garage meta", order = 5)]
	public class GarageMeta : BaseEntryMeta<GarageMeta> {
		[Serializable]
		public class Proxy {
			public ulong Id;
			public string AuthorName;
			public string Name;
			public int Version;

			public string ScenePath;
		}

		[ReadOnly]
		[Tooltip("Objects pack unique ID")]
		public ulong Id = Utils.GenerateUniqueId();

		[Tooltip("Garage version")]
		public int Version = 100;

		[Tooltip("Garage scene")]
		public SceneAsset Scene;

		public override bool Validate() {
			if (string.IsNullOrWhiteSpace(Name)) {
				Debug.LogError("Kino: Garage name is not set");
				return false;
			}

			if (!Scene) {
				Debug.LogError($"Kino: Garage scene for '{Name}' is not set");
				return false;
			}

			string scenePath = AssetDatabase.GetAssetPath(Scene);

			var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
			if (!sceneAsset) {
				Debug.LogError($"Kino: Garage scene not found at path: '{scenePath}'");
				return false;
			}

			var currentScene = SceneManager.GetActiveScene();
			string currentScenePath = currentScene.path;

			var garageScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

			string cameraContainerName = string.Empty;

			var roots = garageScene.GetRootGameObjects();
			foreach (var root in roots) {
				if (root.GetComponentInChildren<Camera>(true)) {
					cameraContainerName = root.name;
					break;
				}
			}

			EditorSceneManager.CloseScene(garageScene, true);

			if (!string.IsNullOrEmpty(currentScenePath)) {
				EditorSceneManager.OpenScene(currentScenePath, OpenSceneMode.Single);
			}

			if (!string.IsNullOrWhiteSpace(cameraContainerName)) {
				Debug.LogError($"Kino: The garage scene contains a camera at: '{cameraContainerName}', delete it before starting the build");
				return false;
			}

			Debug.Log($"Kino: Garage '{Name}' is valid");
			return true;
		}

		public string GetProxyMetaString(AuthorMeta author, out Proxy meta) {
			meta = new Proxy {
				Id = Id,
				Name = Name,
				AuthorName = author.Name,
				Version = Version,
				ScenePath = AssetDatabase.GetAssetPath(Scene),
			};

			return JsonUtility.ToJson(meta, false);
		}
	}

	[CustomEditor(typeof(GarageMeta))]
	public class GarageMetaEditor : BaseMetaEditor<GarageMeta> {
		public GarageMetaEditor() : base(false) { }

		public override void OnInspectorGUI() {
			var script = (GarageMeta) target;

			EditorGUILayout.LabelField("Each field has a tooltip. Hold the cursor over it to see the tooltip.", EditorStyles.boldLabel);

			DrawProp("Id");
			DrawProp("Name");
			DrawProp("Version");
			DrawProp("Scene");

			base.OnInspectorGUI();
		}
	}
}