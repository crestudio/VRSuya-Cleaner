#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/*
 * VRSuya AnimationMirrorer
 * Contact : vrsuya@gmail.com // Twitter : https://twitter.com/VRSuya
 */

namespace com.vrsuya.animationcleaner {

	public class AnimationMirrorer : Editor {

		[MenuItem("Assets/VRSuya/Mirror Animation")]
		public static void MirrorSelectedAnimation() {
			foreach (var SelectedObject in Selection.objects) {
				if (!SelectedObject || !(SelectedObject is AnimationClip)) {
					return;
				}
				AnimationClip OriginalAnimationClip = SelectedObject as AnimationClip;
				AnimationClip MirroredAnimationClip = new AnimationClip();
				MirroredAnimationClip.name = OriginalAnimationClip.name + "_Mirrored";
				EditorUtility.CopySerialized(OriginalAnimationClip, MirroredAnimationClip);
				EditorCurveBinding[] Bindings = AnimationUtility.GetCurveBindings(MirroredAnimationClip);
				foreach (EditorCurveBinding Binding in Bindings) {
					AnimationCurve NewCurve = AnimationUtility.GetEditorCurve(MirroredAnimationClip, Binding);
					for (int Index = 0; Index < NewCurve.keys.Length; Index++) {
						Keyframe OriginalKey = NewCurve.keys[Index];
						OriginalKey.value = -OriginalKey.value;
						NewCurve.MoveKey(Index, OriginalKey);
					}
					AnimationUtility.SetEditorCurve(MirroredAnimationClip, Binding, NewCurve);
				}
				string OriginalAssetPath = AssetDatabase.GetAssetPath(OriginalAnimationClip);
				string NewAssetPath = System.IO.Path.GetDirectoryName(OriginalAssetPath) + "/" + MirroredAnimationClip.name + "_Mirrored.anim";
				AssetDatabase.CreateAsset(MirroredAnimationClip, NewAssetPath);
				AssetDatabase.SaveAssets();
				Debug.Log("[VRSuya] " + NewAssetPath + " 애니메이션 클립을 생성하였습니다.");
			}
			return;
		}
	}
}
#endif