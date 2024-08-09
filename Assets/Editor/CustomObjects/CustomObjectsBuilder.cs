using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Editor {
	public class CustomObjectsBuilder : BaseBuilder<CustomObjectsBuilderMeta, CustomObjectsMeta> {
		protected override void BuildPackBundle(BuildTarget target, CustomObjectsBuilderMeta builder, AuthorMeta author, CustomObjectsMeta pack) {
			Debug.Log($"Kino: Processing objects pack '{pack.Name}'");

			string packFileName = $"{pack.Name}{builder.Ext}";

			string packRootFolder = pack.GetRoot();

			var proxy = pack.GetProxyMeta();
			if (proxy == null) {
				Debug.LogError($"Kino: Unable to build pack '{pack.Name}', failed to create metadata");
				return;
			}

			proxy.AuthorName = author.Name;

			if (!SaveProxyMetaToTxt(packRootFolder, proxy)) {
				Debug.LogError($"Kino: Unable to build pack '{pack.Name}', failed to create metadata");
				return;
			}

			AssetDatabase.Refresh();

			var assets = new HashSet<string> {
				Path.Combine(packRootFolder, CarPartsMeta.PACK_META_RESERVED_NAME)
			};

			foreach (var objProxy in proxy.Objects) {
				if (string.IsNullOrWhiteSpace(objProxy.FilePath)) {
					Debug.LogWarning($"Kino: Skipped invalid object in '{proxy.Name}'");
					continue;
				}

				assets.Add(objProxy.FilePath);
			}

			var builds = new[] {
				new AssetBundleBuild {
					assetBundleName = packFileName,
					assetNames = assets.ToArray()
				}
			};

			if (!Directory.Exists(builder.DefaultBuildFolder)) {
				Directory.CreateDirectory(builder.DefaultBuildFolder);
			}

			BuildPipeline.BuildAssetBundles(builder.DefaultBuildFolder, builds, BuildAssetBundleOptions.ForceRebuildAssetBundle, target);

			RunPostBuild(packFileName, builder, author, proxy);
		}

		protected override bool Validate(AuthorMeta author, IReadOnlyCollection<CustomObjectsMeta> entries, out string error) {
			error = string.Empty;

			if (!author.Validate()) {
				error = "See errors above";
				return false;
			}

			if (entries.Count == 0) {
				error = "Nothing to build";
				return false;
			}

			foreach (var pack in entries) {
				if (!pack.SelectedToBuild) {
					continue;
				}

				if (string.IsNullOrWhiteSpace(pack.Name)) {
					error = "One of selected to build packs has empty fields";
					return false;
				}

				var regex = new Regex("^[A-Za-z0-9_\\-]+$");
				if (!regex.IsMatch(pack.Name)) {
					error = "Pack name contains not allowed characters. Allowed characters: [A-Z,a-z,0-9,-,_]";
					return false;
				}

				if (pack.Version <= 0) {
					error = $"Invalid pack version: {pack.Name} -> {pack.Version}";
					return false;
				}

				if (!pack.Validate()) {
					error = "See warnings above";
					return false;
				}
			}

			return true;
		}

		private void RunPostBuild(string fileName, CustomObjectsBuilderMeta builder, AuthorMeta author, CustomObjectsMeta.Proxy proxy) {
			string srcFilePath = Path.Combine(builder.DefaultBuildFolder, fileName);
			string dstFilePath = Path.Combine(builder.BuildFolder, fileName);

			if (!File.Exists(srcFilePath)) {
				Debug.LogError($"Kino: Unable to locate pack bundle at '{srcFilePath}'");
				return;
			}

			var fileContent = File.ReadAllBytes(srcFilePath);

			using var fileStream = new FileStream(dstFilePath, FileMode.Create);
			using var writer = new BinaryWriter(fileStream);

			writer.Write(proxy.Magic);
			writer.Write(builder.Version);
			writer.Write(proxy.Version);
			writer.Write(proxy.Type);
			writer.Write(proxy.Id);
			writer.Write(author.SteamId);
			writer.Write(author.DiscordId);
			writer.Write(fileContent);
		}

		private bool SaveProxyMetaToTxt(string packRoot, CustomObjectsMeta.Proxy meta) {
			if (string.IsNullOrWhiteSpace(packRoot) || meta == null) {
				return false;
			}

			try {
				string json = JsonUtility.ToJson(meta, true);

				using var stream = new FileStream(Path.Combine(packRoot, CustomObjectsMeta.PACK_META_RESERVED_NAME), FileMode.Create, FileAccess.Write);
				using var writer = new StreamWriter(stream, Encoding.UTF8);
				writer.Write(json);

				return true;
			}
			catch (Exception e) {
				Debug.LogError($"Kino: Unable to save objects pack meta, exception: {e}");
				return false;
			}
		}
	}
}