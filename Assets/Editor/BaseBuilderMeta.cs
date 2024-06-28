using System.IO;
using UnityEditor;
using UnityEngine;

namespace Editor {
	public abstract class BaseMeta<T> : ScriptableObject where T : Object {
		public abstract string AssetName { get; }
		public abstract string BaseFolder { get; }

		public abstract bool Validate();

		public static T GetInstance() {
			string typeName = typeof(T).Name;

			var guids = AssetDatabase.FindAssets($"t:{typeName}");
			if (guids == null || guids.Length == 0) {
				Debug.LogError($"Kino: Unable to locate {typeName} asset");
				return default;
			}

			if (guids.Length > 1) {
				Debug.LogError($"Kino: Multiple {typeName} assets defined. Only one {typeName} should be defined in the project");
				return default;
			}

			string path = AssetDatabase.GUIDToAssetPath(guids[0]);
			return (T) AssetDatabase.LoadAssetAtPath(path, typeof(T));
		}
	}

	public abstract class BaseBuilderMeta<T> : BaseMeta<T> where T : Object {
		public virtual int Version => 300;
		public abstract string Ext { get; }
		public abstract string DefaultBuildFolder { get; }

		[Tooltip("Specify the folder in which you want to put the build result")]
		public string BuildFolder = string.Empty;

		public override bool Validate() {
			if (string.IsNullOrWhiteSpace(BuildFolder)) {
				BuildFolder = DefaultBuildFolder;
			}

			if (!Directory.Exists(BuildFolder)) {
				Directory.CreateDirectory(BuildFolder);
			}

			return true;
		}
	}
}