#if UNITY_EDITOR
using System;
using System.Linq;

using UnityEditor;
using UnityEngine;
using VRC.SDKBase.Midi;

/*
 * VRSuya Animation Cleaner
 * Contact : vrsuya@gmail.com // Twitter : https://twitter.com/VRSuya
 */

namespace com.vrsuya.animationcleaner {

	[ExecuteInEditMode]
	[AddComponentMenu("VRSuya Animation Cleaner")]
	public class AnimationCleaner : MonoBehaviour {

		// 애니메이션 클립 변수
		public AnimationClip[] ArmAnimationClips = new AnimationClip[0];
		public AnimationClip[] ForearmAnimationClips = new AnimationClip[0];
		public AnimationClip[] HandAnimationClips = new AnimationClip[0];

		// 사전 데이터
		private static readonly string[] dictArmMask = new string[] {
			"Left Shoulder Front-Back", "Left Shoulder Down-Up", "Right Shoulder Front-Back", "Right Shoulder Down-Up",
			"Left Forearm Twist In-Out", "Left Forearm Stretch", "Right Forearm Twist In-Out", "Right Forearm Stretch",
			"Left Arm Twist In-Out", "Left Arm Front-Back", "Left Arm Down-Up", "Right Arm Twist In-Out", "Right Arm Front-Back", "Right Arm Down-Up",
			"Left Hand In-Out", "Left Hand Down-Up", "Right Hand In-Out", "Right Hand Down-Up",
			"LeftHandQ.x", "LeftHandQ.y", "LeftHandQ.z", "LeftHandQ.w", "LeftHandT.x", "LeftHandT.y", "LeftHandT.z",
			"RightHandQ.x", "RightHandQ.y", "RightHandQ.z", "RightHandQ.w", "RightHandT.x", "RightHandT.y", "RightHandT.z",
			"LeftHand.Thumb.Spread", "LeftHand.Thumb.1 Stretched", "LeftHand.Thumb.2 Stretched", "LeftHand.Thumb.3 Stretched",
			"LeftHand.Index.Spread", "LeftHand.Index.1 Stretched", "LeftHand.Index.2 Stretched",  "LeftHand.Index.3 Stretched",
			"LeftHand.Middle.Spread", "LeftHand.Middle.1 Stretched", "LeftHand.Middle.2 Stretched",  "LeftHand.Middle.3 Stretched",
			"LeftHand.Ring.Spread", "LeftHand.Ring.1 Stretched", "LeftHand.Ring.2 Stretched",  "LeftHand.Ring.3 Stretched",
			"LeftHand.Little.Spread", "LeftHand.Little.1 Stretched", "LeftHand.Little.2 Stretched",  "LeftHand.Little.3 Stretched",
			"RightHand.Thumb.Spread", "RightHand.Thumb.1 Stretched", "RightHand.Thumb.2 Stretched", "RightHand.Thumb.3 Stretched",
			"RightHand.Index.Spread", "RightHand.Index.1 Stretched", "RightHand.Index.2 Stretched", "RightHand.Index.3 Stretched",
			"RightHand.Middle.Spread", "RightHand.Middle.1 Stretched", "RightHand.Middle.2 Stretched", "RightHand.Middle.3 Stretched",
			"RightHand.Ring.Spread", "RightHand.Ring.1 Stretched", "RightHand.Ring.2 Stretched", "RightHand.Ring.3 Stretched",
			"RightHand.Little.Spread", "RightHand.Little.1 Stretched", "RightHand.Little.2 Stretched", "RightHand.Little.3 Stretched"
		};
		private static readonly string[] dictForearmMask = new string[] {
			"Left Shoulder Front-Back", "Left Shoulder Down-Up", "Right Shoulder Front-Back", "Right Shoulder Down-Up",
			"Left Forearm Twist In-Out", "Left Forearm Stretch", "Right Forearm Twist In-Out", "Right Forearm Stretch",
			"Left Arm Twist In-Out", "Left Arm Front-Back", "Left Arm Down-Up", "Right Arm Twist In-Out", "Right Arm Front-Back", "Right Arm Down-Up",
			"Left Hand In-Out", "Left Hand Down-Up", "Right Hand In-Out", "Right Hand Down-Up",
			"LeftHandQ.x", "LeftHandQ.y", "LeftHandQ.z", "LeftHandQ.w", "LeftHandT.x", "LeftHandT.y", "LeftHandT.z",
			"RightHandQ.x", "RightHandQ.y", "RightHandQ.z", "RightHandQ.w", "RightHandT.x", "RightHandT.y", "RightHandT.z"
		};
		private static readonly string[] dictHandMask = new string[] {
			"LeftHand.Thumb.Spread", "LeftHand.Thumb.1 Stretched", "LeftHand.Thumb.2 Stretched", "LeftHand.Thumb.3 Stretched",
			"LeftHand.Index.Spread", "LeftHand.Index.1 Stretched", "LeftHand.Index.2 Stretched",  "LeftHand.Index.3 Stretched",
			"LeftHand.Middle.Spread", "LeftHand.Middle.1 Stretched", "LeftHand.Middle.2 Stretched",  "LeftHand.Middle.3 Stretched",
			"LeftHand.Ring.Spread", "LeftHand.Ring.1 Stretched", "LeftHand.Ring.2 Stretched",  "LeftHand.Ring.3 Stretched",
			"LeftHand.Little.Spread", "LeftHand.Little.1 Stretched", "LeftHand.Little.2 Stretched",  "LeftHand.Little.3 Stretched",
			"RightHand.Thumb.Spread", "RightHand.Thumb.1 Stretched", "RightHand.Thumb.2 Stretched", "RightHand.Thumb.3 Stretched",
			"RightHand.Index.Spread", "RightHand.Index.1 Stretched", "RightHand.Index.2 Stretched", "RightHand.Index.3 Stretched",
			"RightHand.Middle.Spread", "RightHand.Middle.1 Stretched", "RightHand.Middle.2 Stretched", "RightHand.Middle.3 Stretched",
			"RightHand.Ring.Spread", "RightHand.Ring.1 Stretched", "RightHand.Ring.2 Stretched", "RightHand.Ring.3 Stretched",
			"RightHand.Little.Spread", "RightHand.Little.1 Stretched", "RightHand.Little.2 Stretched", "RightHand.Little.3 Stretched"
		};

		// 컴포넌트 최초 로드시 동작
		void OnEnable() {
			return;
		}

		/// <summary>
		/// 본 프로그램의 주요 애니메이션 로직입니다.
		/// </summary>
		public void CountTargetProperty() {
			foreach (AnimationClip CurrentAnimationClip in ArmAnimationClips) {
				string[] TargetPropertyNames = new string[0];
				if (!CurrentAnimationClip) continue;
				foreach (EditorCurveBinding Binding in AnimationUtility.GetCurveBindings(CurrentAnimationClip)) {
					if (!Array.Exists(dictArmMask, MaskName => Binding.propertyName == MaskName)) {
						TargetPropertyNames = TargetPropertyNames.Concat(new string[] { Binding.propertyName }).ToArray();
					}
				}
				Debug.Log("[AnimationCleaner] 팔 전체 애니메이션은 총 " + TargetPropertyNames.Length + "건이 삭제 됩니다.");
			}
			foreach (AnimationClip CurrentAnimationClip in ForearmAnimationClips) {
				string[] TargetPropertyNames = new string[0];
				if (!CurrentAnimationClip) continue;
				foreach (EditorCurveBinding Binding in AnimationUtility.GetCurveBindings(CurrentAnimationClip)) {
					if (!Array.Exists(dictForearmMask, MaskName => Binding.propertyName == MaskName)) {
						TargetPropertyNames = TargetPropertyNames.Concat(new string[] { Binding.propertyName }).ToArray();
					}
				}
				Debug.Log("[AnimationCleaner] 팔뚝 애니메이션은 총 " + TargetPropertyNames.Length + "건이 삭제 됩니다.");
			}
			foreach (AnimationClip CurrentAnimationClip in HandAnimationClips) {
				string[] TargetPropertyNames = new string[0];
				if (!CurrentAnimationClip) continue;
				foreach (EditorCurveBinding Binding in AnimationUtility.GetCurveBindings(CurrentAnimationClip)) {
					if (!Array.Exists(dictHandMask, MaskName => Binding.propertyName == MaskName)) {
						TargetPropertyNames = TargetPropertyNames.Concat(new string[] { Binding.propertyName }).ToArray();
					}
				}
				Debug.Log("[AnimationCleaner] 손 애니메이션은 총 " + TargetPropertyNames.Length + "건이 삭제 됩니다.");
			}
			return;
		}

		/// <summary>
		/// 본 프로그램의 실제 애니메이션 업데이트 로직입니다.
		/// </summary>
		public void UpdateAnimations() {
			int StatusDeletedCount = 0;
			foreach (AnimationClip CurrentAnimationClip in ArmAnimationClips) {
				if (!CurrentAnimationClip) continue;
				foreach (EditorCurveBinding Binding in AnimationUtility.GetCurveBindings(CurrentAnimationClip)) {
					if (!Array.Exists(dictArmMask, MaskName => Binding.propertyName == MaskName)) {
						CurrentAnimationClip.SetCurve(Binding.path, Binding.type, Binding.propertyName, null);
						StatusDeletedCount++;
					}
				}
			}
			foreach (AnimationClip CurrentAnimationClip in ForearmAnimationClips) {
				if (!CurrentAnimationClip) continue;
				foreach (EditorCurveBinding Binding in AnimationUtility.GetCurveBindings(CurrentAnimationClip)) {
					if (!Array.Exists(dictForearmMask, MaskName => Binding.propertyName == MaskName)) {
						CurrentAnimationClip.SetCurve(Binding.path, Binding.type, Binding.propertyName, null);
						StatusDeletedCount++;
					}
				}
			}
			foreach (AnimationClip CurrentAnimationClip in HandAnimationClips) {
				if (!CurrentAnimationClip) continue;
				foreach (EditorCurveBinding Binding in AnimationUtility.GetCurveBindings(CurrentAnimationClip)) {
					if (!Array.Exists(dictHandMask, MaskName => Binding.propertyName == MaskName)) {
						CurrentAnimationClip.SetCurve(Binding.path, Binding.type, Binding.propertyName, null);
						StatusDeletedCount++;
					}
				}
			}
			Debug.LogWarning("[AnimationCleaner] 애니메이션에서 불필요한 속성이 총 " + StatusDeletedCount + "건이 삭제 되었습니다!");
			return;
		}

		/// <summary>
		/// 본 프로그램의 애니메이션 파일을 1 프레임 포즈 애니메이션으로 만드는 로직입니다.
		/// </summary>
		public void MakePoseAnimations() {
			foreach (AnimationClip CurrentAnimationClip in ArmAnimationClips) {
				if (!CurrentAnimationClip) continue;
				RemoveKeyframes(CurrentAnimationClip);
			}
			foreach (AnimationClip CurrentAnimationClip in ForearmAnimationClips) {
				if (!CurrentAnimationClip) continue;
				RemoveKeyframes(CurrentAnimationClip);
			}
			foreach (AnimationClip CurrentAnimationClip in HandAnimationClips) {
				if (!CurrentAnimationClip) continue;
				RemoveKeyframes(CurrentAnimationClip);
			}
			return;
		}

		private void RemoveKeyframes(AnimationClip CurrentAnimationClip) {
			AnimationCurve[] CurrentCurves = AnimationUtility.GetCurveBindings(CurrentAnimationClip).Select(curveBinding => AnimationUtility.GetEditorCurve(CurrentAnimationClip, curveBinding)).ToArray();
			foreach (var Curve in CurrentCurves) {
				Keyframe[] NewKeyframes = Curve.keys;
				NewKeyframes = NewKeyframes.Where(Keyframe => Keyframe.time == 0f).ToArray();
				Curve.keys = NewKeyframes;
			}
			for (int Index = 0; Index < CurrentCurves.Length; Index++) {
				AnimationUtility.SetEditorCurve(CurrentAnimationClip, AnimationUtility.GetCurveBindings(CurrentAnimationClip)[Index], CurrentCurves[Index]);
			}
			Debug.LogWarning("[AnimationCleaner] 애니메이션이 포즈 애니메이션으로 변환 되었습니다!");
			return;
		}
	}
}
#endif