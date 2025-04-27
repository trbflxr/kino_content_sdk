using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Editor {
	public class GarageBuilder : BaseBuilder<GarageBuilderMeta, GarageMeta> {
		private const int MAGIC = 0xdedede;

		protected override void BuildPackBundle(BuildTarget target, GarageBuilderMeta builder, AuthorMeta author, GarageMeta garage) {
			Debug.Log($"Kino: Processing garage '{garage.Name}'");

			string fileName = $"{garage.Name}{builder.Ext}";

			var metaString = garage.GetProxyMetaString(author, out var meta);
			if (string.IsNullOrWhiteSpace(metaString)) {
				Debug.LogError($"Kino: Unable to build garage '{garage.Name}', failed to create metadata");
				return;
			}

			AssetDatabase.Refresh();

			var assets = new HashSet<string> {
				meta.ScenePath
			};

			var builds = new[] {
				new AssetBundleBuild {
					assetBundleName = fileName,
					assetNames = assets.ToArray()
				}
			};

			if (!Directory.Exists(builder.DefaultBuildFolder)) {
				Directory.CreateDirectory(builder.DefaultBuildFolder);
			}

			BuildPipeline.BuildAssetBundles(builder.DefaultBuildFolder, builds, BuildAssetBundleOptions.ForceRebuildAssetBundle, target);

			RunPostBuild(fileName, builder, author, garage, metaString);
		}

		protected override bool Validate(AuthorMeta author, IReadOnlyCollection<GarageMeta> entries, out string error) {
			error = string.Empty;

			if (!author.Validate()) {
				error = "See errors above";
				return false;
			}

			if (entries.Count == 0) {
				error = "Nothing to build";
				return false;
			}

			foreach (var map in entries) {
				if (!map.SelectedToBuild) {
					continue;
				}

				if (string.IsNullOrWhiteSpace(map.Name)) {
					error = "One of selected to build garages has empty name";
					return false;
				}

				var regex = new Regex("^[A-Za-z0-9_\\-]+$");
				if (!regex.IsMatch(map.Name)) {
					error = "Garage name contains not allowed characters. Allowed characters: [A-Z,a-z,0-9,-,_]";
					return false;
				}

				if (map.Version <= 0) {
					error = $"Invalid pack version: {map.Name} -> {map.Version}";
					return false;
				}

				if (!map.Validate()) {
					error = "See warnings above";
					return false;
				}
			}

			return true;
		}

		private void RunPostBuild(string fileName, GarageBuilderMeta builder, AuthorMeta author, GarageMeta garage, string metaString) {
			string srcFilePath = Path.Combine(builder.DefaultBuildFolder, fileName);
			string dstFilePath = Path.Combine(builder.BuildFolder, fileName);

			if (!File.Exists(srcFilePath)) {
				Debug.LogError($"Kino: Unable to locate garage bundle at '{srcFilePath}'");
				return;
			}

			var fileContent = File.ReadAllBytes(srcFilePath);

			using var fileStream = new FileStream(dstFilePath, FileMode.Create);
			using var writer = new BinaryWriter(fileStream);

			writer.Write(MAGIC);
			writer.Write(metaString);
			writer.Write(builder.Version);
			writer.Write(garage.Id);
			writer.Write(garage.Name);
			writer.Write(author.Name);
			writer.Write(0);
			writer.Write(fileContent);

			Debug.Log($"Kino: Garage '{garage.Name}' build completed");
		}
	}
}