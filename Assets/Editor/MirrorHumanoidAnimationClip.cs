#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/*
 * VRSuya Cleaner
 * Contact : vrsuya@gmail.com // Twitter : https://twitter.com/VRSuya
 */

namespace com.vrsuya.cleaner {

	public class MirrorHumanoidAnimationClip : Editor {

		[MenuItem("Assets/VRSuya/Mirror Humanoid AnimationClip")]
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
					AnimationCurve Curve = AnimationUtility.GetEditorCurve(OriginalAnimationClip, Binding);
					if (Binding.propertyName.Contains("Left-Right")) {
						for (int Index = 0; Index < Curve.keys.Length; Index++) {
							Curve.keys[Index].value = -Curve.keys[Index].value;
						}
						AnimationUtility.SetEditorCurve(MirroredAnimationClip, Binding, Curve);
					} else if (Binding.propertyName.Contains("Left") && !Binding.propertyName.Contains("Left-Right")) {
						EditorCurveBinding newBinding = Binding;
						newBinding.propertyName = Binding.propertyName.Replace("Left", "Right");
						AnimationUtility.SetEditorCurve(MirroredAnimationClip, newBinding, Curve);
					} else if (Binding.propertyName.Contains("Right") && !Binding.propertyName.Contains("Left-Right")) {
						EditorCurveBinding newBinding = Binding;
						newBinding.propertyName = Binding.propertyName.Replace("Right", "Left");
						AnimationUtility.SetEditorCurve(MirroredAnimationClip, newBinding, Curve);
					} else if (Binding.propertyName.Contains("RootT.x") || 
						Binding.propertyName.Contains("RootQ.x") ||
						Binding.propertyName.Contains("RootQ.y") ||
						Binding.propertyName.Contains("RootQ.z") ||
						Binding.propertyName.Contains("RootQ.w")) {
						for (int Index = 0; Index < Curve.keys.Length; Index++) {
							Curve.keys[Index].value = -Curve.keys[Index].value;
						}
						AnimationUtility.SetEditorCurve(MirroredAnimationClip, Binding, Curve);
					} else {
						AnimationUtility.SetEditorCurve(MirroredAnimationClip, Binding, Curve);
					}
				}
				string OriginalAssetPath = AssetDatabase.GetAssetPath(OriginalAnimationClip);
				string NewAssetPath = System.IO.Path.GetDirectoryName(OriginalAssetPath) + "/" + MirroredAnimationClip.name + "_Mirrored.anim";
				AssetDatabase.CreateAsset(MirroredAnimationClip, NewAssetPath);
				AssetDatabase.SaveAssets();
				Debug.Log($"[VRSuya] {MirroredAnimationClip.name} 애니메이션 클립을 생성하였습니다");
			}
			return;
		}
	}
}
#endif