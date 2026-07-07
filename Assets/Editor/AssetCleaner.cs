using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using UnityEditor;
using UnityEngine;

using VRSuya.Core;

/*
 * VRSuya Cleaner
 * Contact : vrsuya@gmail.com // Twitter : https://twitter.com/VRSuya
 */

namespace VRSuya.Cleaner {
	
    public class AssetCleaner : EditorWindow {

		const bool TargetStreamingMipmaps = true;
		const TextureImporterMipFilter TargetMipmap = TextureImporterMipFilter.KaiserFilter;
		const FilterMode TargetFilterMode = FilterMode.Trilinear;
		const int TargetAnisoLevel = 16;
		const TextureImporterCompression TargetTextureCompression = TextureImporterCompression.CompressedHQ;
		const bool TargetCrunchedCompression = false;
		const int TargetCompressionQuality = 50;

		[MenuItem("Assets/VRSuya/Asset/Standardize Texture2D", true)]
		static bool ValidateTexture2D() {
			return Selection.objects
				.Select(Item => AssetDatabase.GetAssetPath(Item))
				.Any(Item => HasTextureImporter(Item));
		}

		[MenuItem("Assets/VRSuya/Asset/Standardize Texture2D", priority = 1000)]
		static void RequestStandardizeTexture2D() {
			string[] AssetGUIDs = Selection.objects.Select(Item => AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(Item))).ToArray();
			if (AssetGUIDs.Length > 0) {
				int ModifiedCount = 0;
				try {
					for (int Index = 0; Index < AssetGUIDs.Length; Index++) {
						string TargetAssetName = AssetUtility.GUIDToAssetName(AssetGUIDs[Index], true);
						EditorUtility.DisplayProgressBar("Standardizing Texture2D",
							$"Processing : {TargetAssetName}",
							(float)Index / AssetGUIDs.Length);
						if (TargetAssetName.EndsWith("Original")) continue;
						string TargetAssetPath = AssetDatabase.GUIDToAssetPath(AssetGUIDs[Index]);
						Texture2D TargetTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(TargetAssetPath);
						if (StandardizeTexture(TargetTexture)) {
							ModifiedCount++;
						}
					}
				} finally {
					EditorUtility.ClearProgressBar();
					AssetDatabase.Refresh();
				}
				Debug.Log($"[VRSuya] Standardized of {ModifiedCount} Textures");
			}
		}

		[MenuItem("Tools/VRSuya/Cleaner/Asset/Standardize Texture2Ds", priority = 1000)]
		static void RequestAllStandardizeTexture2D() {
			string[] Texture2DGUIDs = GetTexture2Ds();
			if (Texture2DGUIDs.Length > 0) {
				int ModifiedCount = 0;
				try {
					for (int Index = 0; Index < Texture2DGUIDs.Length; Index++) {
						string TargetAssetName = AssetUtility.GUIDToAssetName(Texture2DGUIDs[Index], true);
						EditorUtility.DisplayProgressBar("Standardizing Texture2D",
							$"Processing : {TargetAssetName}",
							(float)Index / Texture2DGUIDs.Length);
						if (TargetAssetName.EndsWith("Original")) continue;
						string TargetAssetPath = AssetDatabase.GUIDToAssetPath(Texture2DGUIDs[Index]);
						Texture2D TargetTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(TargetAssetPath);
						if (StandardizeTexture(TargetTexture)) {
							ModifiedCount++;
						}
					}
				} finally {
					EditorUtility.ClearProgressBar();
					AssetDatabase.Refresh();
				}
				Debug.Log($"[VRSuya] Standardized of {ModifiedCount} Textures");
			}
		}

		static string[] GetTexture2Ds() {
			List<string> Texture2DGUIDList = new List<string>();
			string[] TextureGUIDs = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets" });
			foreach (string TargetTextureGUID in TextureGUIDs) {
				if (HasTextureImporter(AssetDatabase.GUIDToAssetPath(TargetTextureGUID))) Texture2DGUIDList.Add(TargetTextureGUID);
			}
			return Texture2DGUIDList.ToArray();
		}

		static bool HasTextureImporter(string TargetAssetPath) {
			if (string.IsNullOrEmpty(TargetAssetPath)) return false;
			TextureImporter TargetTextureImporter = AssetImporter.GetAtPath(TargetAssetPath) as TextureImporter;
			if (TargetTextureImporter) {
				return true;
			} else {
				return false;
			}
		}

		public static bool StandardizeTexture(Texture2D TargetTexture) {
			if (TargetTexture && TargetTexture is Texture2D) {
				string TargetAssetPath = AssetDatabase.GetAssetPath(TargetTexture);
				TextureImporter TargetTextureImporter = AssetImporter.GetAtPath(TargetAssetPath) as TextureImporter;
				if (TargetTextureImporter) {
					bool IsDrity = false;
					if (TargetTextureImporter.streamingMipmaps != TargetStreamingMipmaps) { TargetTextureImporter.streamingMipmaps = TargetStreamingMipmaps; IsDrity = true; }
					if (TargetTextureImporter.mipmapFilter != TargetMipmap) { TargetTextureImporter.mipmapFilter = TargetMipmap; IsDrity = true; }
					if (TargetTextureImporter.filterMode != TargetFilterMode) { TargetTextureImporter.filterMode = TargetFilterMode; IsDrity = true; }
					if (TargetTextureImporter.anisoLevel != TargetAnisoLevel) { TargetTextureImporter.anisoLevel = TargetAnisoLevel; IsDrity = true; }
					if (TargetTextureImporter.textureCompression != TargetTextureCompression) { TargetTextureImporter.textureCompression = TargetTextureCompression; IsDrity = true; }
					if (TargetTextureImporter.crunchedCompression != TargetCrunchedCompression) { TargetTextureImporter.crunchedCompression = TargetCrunchedCompression; IsDrity = true; }
					if (TargetTextureImporter.compressionQuality != TargetCompressionQuality) { TargetTextureImporter.compressionQuality = TargetCompressionQuality; IsDrity = true; }
					if (IsDrity) {
						EditorUtility.SetDirty(TargetTexture);
						TargetTextureImporter.SaveAndReimport();
						Debug.Log($"[VRSuya] Updated {TargetTexture.name} texture properties");
						return true;
					}
				}
			}
			return false;
		}

		[MenuItem("Tools/VRSuya/Cleaner/Asset/Get Missing GUIDs", priority = 1000)]
		static void RequestGetMissingGUIDs() {
			string[] AssetGUIDs = AssetDatabase.FindAssets("glob:\"*.*\"", new[] { "Assets" });
			string[] MissingGUIDs = GetMissingGUIDs(AssetGUIDs);
			if (MissingGUIDs.Length > 0) {
				EditorUtility.DisplayDialog(
					"VRSuya Cleaner",
					$"Found {MissingGUIDs.Length} missing GUID references, Please check Unity Console",
					"Okay"
				);
				Debug.Log($"[VRSuya] Missing GUID List\n{string.Join("\n", MissingGUIDs)}");
			} else {
				EditorUtility.DisplayDialog(
					"VRSuya Cleaner",
					"No missing GUID references found",
					"Okay"
				);
			}
		}

		static string[] GetMissingGUIDs(string[] TargetGUIDs) {
			string Pattern = @"guid:\s*([a-fA-F0-9]{32})";
			List<string> NewMissingGUIDs = new List<string>();
			foreach (string TargetGUID in TargetGUIDs) {
				string TargetAssetPath = AssetDatabase.GUIDToAssetPath(TargetGUID);
				string TargetAssetName = AssetUtility.GetAssetName(TargetAssetPath, false);
				string[] RawAssetLines = File.ReadAllLines(TargetAssetPath)
					.Where(Item => Item.Contains("guid:"))
					.ToArray();
				foreach (string RawAssetLine in RawAssetLines) {
					Match RegexMatch = Regex.Match(RawAssetLine, Pattern);
					if (!RegexMatch.Success) continue;
					if (string.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(RegexMatch.Groups[1].Value))) {
						NewMissingGUIDs.Add($"{TargetAssetName}\t{RegexMatch.Groups[1].Value}");
					}
				}
			}
			return NewMissingGUIDs.Distinct().OrderBy(Item => Item).ToArray();
		}
	}
}

