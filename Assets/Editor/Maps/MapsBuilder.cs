using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Editor {
	public class MapsBuilder : BaseBuilder<MapsBuilderMeta, MapMeta> {
		private const string FILE_HEADER = " _   _______   \n"
		                                   + "| | / /_   _\\  \n"
		                                   + "| |/ /  | |    \n"
		                                   + "|    \\  | |    \n"
		                                   + "| |\\  \\_| |_   \n"
		                                   + "\\_| \\_/\\___/   \n"
		                                   + " _   _ _____   \n"
		                                   + "| \\ | |  _  |  \n"
		                                   + "|  \\| | | | |  \n"
		                                   + "| . ` | | | |  \n"
		                                   + "| |\\  \\ \\_/ /  \n"
		                                   + "\\_| \\_/\\___/   ";

		protected override void BuildPackBundle(BuildTarget target, MapsBuilderMeta builder, AuthorMeta author, MapMeta map) {
			Debug.Log($"Kino: Processing map '{map.Name}'");

			string mapFileName = $"{map.Name}{builder.Ext}";

			AssetDatabase.Refresh();

			var assets = new HashSet<string> {
				map.ScenePath
			};

			var builds = new[] {
				new AssetBundleBuild {
					assetBundleName = mapFileName,
					assetNames = assets.ToArray()
				}
			};

			BuildPipeline.BuildAssetBundles(builder.DefaultBuildFolder, builds, BuildAssetBundleOptions.ForceRebuildAssetBundle, target);

			RunPostBuild(mapFileName, builder, author, map);
		}

		protected override bool Validate(AuthorMeta author, IReadOnlyCollection<MapMeta> entries, out string error) {
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
					error = "One of selected to build maps has empty name";
					return false;
				}

				var regex = new Regex("^[A-Za-z0-9_\\-]+$");
				if (!regex.IsMatch(map.Name)) {
					error = "Map name contains not allowed characters. Allowed characters: [A-Z,a-z,0-9,-,_]";
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

		private void RunPostBuild(string fileName, MapsBuilderMeta builder, AuthorMeta author, MapMeta map) {
			string srcFilePath = Path.Combine(builder.DefaultBuildFolder, fileName);
			string dstFilePath = Path.Combine(builder.BuildFolder, fileName);

			if (!File.Exists(srcFilePath)) {
				Debug.LogError($"Kino: Unable to locate map bundle at '{srcFilePath}'");
				return;
			}

			var fileContent = File.ReadAllBytes(srcFilePath);

			if (!File.Exists(map.LoadScreenPath)) {
				Debug.LogError($"Kino: Unable to process map '{map.Name}', loadScreen file '{map.LoadScreenPath}' doesn't exists on the disc");
				return;
			}

			var loadScreenData = File.ReadAllBytes(map.LoadScreenPath);

			using var fileStream = new FileStream(dstFilePath, FileMode.Create);
			using var writer = new BinaryWriter(fileStream);

			writer.Write(FILE_HEADER);
			writer.Write(builder.Version);
			writer.Write(map.Id);
			writer.Write(map.Name);
			writer.Write(author.Name);
			writer.Write(0);
			writer.Write(loadScreenData.Length);
			writer.Write(loadScreenData);
			writer.Write(fileContent);

			Debug.Log($"Kino: Map '{map.Name}' build completed");
		}
	}
}