#if CONTINUOUS_AVATAR_UPLOADER
using UnityEditor;
using UnityEngine;

using Anatawa12.ContinuousAvatarUploader;

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
			return;
		}
	}
}
#endif