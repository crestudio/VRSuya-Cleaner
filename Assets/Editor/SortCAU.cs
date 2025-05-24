#if CONTINUOUS_AVATAR_UPLOADER
using System;
using System.Linq;

using UnityEditor;
using UnityEngine;

using Anatawa12.ContinuousAvatarUploader.Editor;

/*
 * VRSuya Cleaner
 * Contact : vrsuya@gmail.com // Twitter : https://twitter.com/VRSuya
 * Forked from https://forum.unity.com/threads/get-animator-in-editor-mode.461838/
 */

namespace com.vrsuya.cleaner {

	[ExecuteInEditMode]
	public class SortCAU : EditorWindow {

		[MenuItem("Tools/VRSuya/Cleaner/Sort CAU", priority = 1000)]
		public static void SortCAUAssets() {
			string[] AssetGUIDs = AssetDatabase.FindAssets("glob:\"*.asset\"", new[] { "Assets/" });
			if (AssetGUIDs.Length > 0) {
				foreach (string TargetAssetGUID in AssetGUIDs) {
					AvatarUploadSettingGroup TargetCAU = AssetDatabase.LoadAssetAtPath<AvatarUploadSettingGroup>(AssetDatabase.GUIDToAssetPath(TargetAssetGUID));
					if (TargetCAU) {
						if (TargetCAU.avatars.Length > 1) {
							AvatarUploadSetting[] NewAvatarUploadSetting = TargetCAU.avatars
								.OrderBy(Item => Item.avatarName)
								.ToArray();
							if (!TargetCAU.avatars.SequenceEqual(NewAvatarUploadSetting)) {
								TargetCAU.avatars = NewAvatarUploadSetting;
								EditorUtility.SetDirty(TargetCAU);
								AssetDatabase.SaveAssets();
								Debug.Log($"[VRSuya] {TargetCAU.name} have been sorted successfully");
							}
						}
					}
				}
				AssetDatabase.Refresh();
			}
			return;
		}
	}
}
#endif