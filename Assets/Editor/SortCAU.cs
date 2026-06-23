#if CONTINUOUS_AVATAR_UPLOADER
using System;
using System.Linq;

using UnityEditor;
using UnityEngine;

using VRSuya.Core;

using Anatawa12.ContinuousAvatarUploader.Editor;

/*
 * VRSuya Cleaner
 * Contact : vrsuya@gmail.com // Twitter : https://twitter.com/VRSuya
 * Forked from https://forum.unity.com/threads/get-animator-in-editor-mode.461838/
 */

namespace VRSuya.Cleaner {

	public class SortCAU : EditorWindow {

		[MenuItem("Assets/VRSuya/Asset/Sort CAU", true)]
		static bool ValidateCAU() {
			return Asset.ContainAsset(Selection.objects);
		}

		[MenuItem("Assets/VRSuya/Asset/Sort CAU", priority = 1000)]
		static void RequestSortCAU() {
			string[] AssetGUIDs = Selection.objects.Select(Item => AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(Item))).ToArray();
			if (AssetGUIDs.Length > 0) {
				SortCAUAssets(AssetGUIDs);
			}
		}

		[MenuItem("Tools/VRSuya/Cleaner/Asset/Sort CAU", priority = 1000)]
		static void RequestAllSortCAU() {
			string[] AssetGUIDs = AssetDatabase.FindAssets("t:AvatarUploadSettingGroup", new[] { "Assets/" });
			if (AssetGUIDs.Length > 0) {
				SortCAUAssets(AssetGUIDs);
			}
		}

		static void SortCAUAssets(string[] TargetGUIDs) {
			int ModifiedCount = 0;
			try {
				for (int Index = 0; Index < TargetGUIDs.Length; Index++) {
					string TargetAssetName = Asset.GUIDToAssetName(TargetGUIDs[Index], true);
					EditorUtility.DisplayProgressBar("Sorting Continuous Avatar Uploader",
						$"Processing : {TargetAssetName}",
						(float)Index / TargetGUIDs.Length);
					if (TargetAssetName.EndsWith("Original")) continue;
					string TargetAssetPath = AssetDatabase.GUIDToAssetPath(TargetGUIDs[Index]);
					AvatarUploadSettingGroup TargetCAU = AssetDatabase.LoadAssetAtPath<AvatarUploadSettingGroup>(TargetAssetPath);
					if (SortAvatar(TargetCAU)) {
						ModifiedCount++;
					}
				}
			} finally {
				EditorUtility.ClearProgressBar();
				if (ModifiedCount > 0) {
					AssetDatabase.SaveAssets();
					AssetDatabase.Refresh();
				}
			}
			Debug.Log($"[VRSuya] Sorted {ModifiedCount} Continuous Avatar Uploaders");
		}

		static bool SortAvatar(AvatarUploadSettingGroup TargetCAU) {
			if (TargetCAU && TargetCAU is AvatarUploadSettingGroup) {
				if (TargetCAU.avatars.Length > 1) {
					AvatarUploadSetting[] NewAvatarUploadSetting = TargetCAU.avatars
						.OrderBy(Item => Item.avatarName)
						.ToArray();
					if (!TargetCAU.avatars.SequenceEqual(NewAvatarUploadSetting)) {
						TargetCAU.avatars = NewAvatarUploadSetting;
						EditorUtility.SetDirty(TargetCAU);
						return true;
					}
				}
			}
			return false;
		}
	}
}
#endif