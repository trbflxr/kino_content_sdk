using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Editor {
	public enum ObjectsType {
		Undefined = 0,
		MapObjects,
		CarExterior,
		CarInterior,
		Misc
	}

	[Serializable]
	public class CustomObjectMeta {
		[Serializable]
		public class Proxy {
			public string FilePath;
		}

		[HideInInspector]
		public string name = "unknown";

		[Tooltip("Part prefab, can't be null")]
		public GameObject Prefab;

		[HideInInspector]
		public string FilePath = string.Empty;

		public void Validate() {
			if (Prefab) {
				name = Prefab.name;
				FilePath = AssetDatabase.GetAssetPath(Prefab);
			}
			else {
				name = "unknown";
			}
		}
	}

	[CreateAssetMenu(fileName = "__objects_meta", menuName = "Kino/Create Custom Objects pack meta", order = 3)]
	public class CustomObjectsMeta : BaseEntryMeta<CustomObjectsMeta> {
		[Serializable]
		public class Proxy {
			public readonly byte[] Magic = {
				0x6B, 0x6E, 0x63, 0x6F
			};

			public ulong Id;
			public int Type;
			public string AuthorName;
			public string Name;
			public string Description;
			public int Version;

			public List<CustomObjectMeta.Proxy> Objects;
		}

		public const string PACK_META_RESERVED_NAME = "__pack_meta.txt";

		[ReadOnly]
		[Tooltip("Objects pack unique ID")]
		public ulong Id = Utils.GenerateUniqueId();

		[Tooltip("[Optional] Type of the object")]
		public ObjectsType Type = ObjectsType.Undefined;

		[TextArea(4, 20)]
		[Tooltip("[Optional] Objects pack description")]
		public string Description = string.Empty;

		[Tooltip("Objects pack version")]
		public int Version = 100;

		public List<CustomObjectMeta> Objects;

		public Proxy GetProxyMeta() {
			var meta = new Proxy {
				Id = Id,
				Type = (int) Type,
				Name = Name,
				Version = Version,
				Description = Description,
				Objects = new List<CustomObjectMeta.Proxy>()
			};

			if (Objects == null) {
				return null;
			}

			foreach (var objectMeta in Objects) {
				var partMeta = new CustomObjectMeta.Proxy {
					FilePath = objectMeta.FilePath
				};

				if (string.IsNullOrWhiteSpace(partMeta.FilePath)) {
					continue;
				}

				meta.Objects.Add(partMeta);
			}

			return meta;
		}

		public override bool Validate() {
			if (Objects == null) {
				return false;
			}

			Debug.Log($"Kino: Validating objects list {Name}");

			if (Objects.Count == 0) {
				Debug.LogWarning($"Kino: No custom objects added");
				return false;
			}

			foreach (var objectMeta in Objects) {
				objectMeta.Validate();

				if (!objectMeta.Prefab) {
					Debug.LogWarning($"Kino: Prefab is not set for object '{objectMeta.name}'");
					return false;
				}
			}

			return true;
		}

		public void RefreshAssets() {
			string rootFolder = GetRoot();
			if (string.IsNullOrWhiteSpace(rootFolder)) {
				return;
			}

			string[] files = Directory.GetFiles(rootFolder, "*.prefab", SearchOption.TopDirectoryOnly);

			Debug.Log($"Kino: Found {files.Length} prefabs, processing");

			int removed = Objects.RemoveAll(meta => !meta.Prefab);
			if (removed != 0) {
				Debug.Log($"Kino: Removed {removed} broken prefabs");
			}

			foreach (var filePath in files) {
				string fileName = Path.GetFileName(filePath);
				string prefabPath = Path.Combine(rootFolder, fileName);

				var objectMeta = new CustomObjectMeta {
					Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath)
				};

				if (!objectMeta.Prefab) {
					Debug.LogWarning($"Kino: Unable to add prefab '{fileName}'");
					continue;
				}

				string prefabName = objectMeta.Prefab.name;

				int index = Objects.FindIndex(s => s.Prefab.name == prefabName);
				if (index == -1) {
					Debug.Log($"Kino: Added new prefab '{prefabName}'");
					Objects.Add(objectMeta);
				}
			}

			EditorUtility.SetDirty(this);
			AssetDatabase.SaveAssets();

			Validate();
		}
	}

	[CustomEditor(typeof(CustomObjectsMeta))]
	public class CustomObjectsMetaEditor : UnityEditor.Editor {
		public override void OnInspectorGUI() {
			var script = (CustomObjectsMeta) target;

			DrawProp("Type");
			DrawProp("Name");
			DrawProp("Description");
			DrawProp("Version");
			DrawProp("Objects");

			if (GUILayout.Button("Refresh assets")) {
				script.RefreshAssets();
			}

			if (GUILayout.Button("Validate all objects")) {
				script.Validate();
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