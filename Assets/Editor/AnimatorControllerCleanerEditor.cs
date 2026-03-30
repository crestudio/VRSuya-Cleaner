using System.Linq;

using UnityEditor;
using UnityEngine;
using UnityEditor.Animations;

using VRSuya.Core;

/*
 * VRSuya Cleaner
 * Contact : vrsuya@gmail.com // Twitter : https://twitter.com/VRSuya
 */

namespace com.vrsuya.cleaner {

	[ExecuteInEditMode]
	public class AnimatorControllerCleanerEditor : EditorWindow {

		[MenuItem("Assets/VRSuya/Animator/Clean up AnimatorController", true)]
		static bool ValidateAsset() {
			Asset AssetInstance = new Asset();
			return AssetInstance.ContainAnimatorController(Selection.objects);
		}

		[MenuItem("Assets/VRSuya/Animator/Clean up AnimatorController", priority = 1000)]
		static void RequestCleanAnimatorController() {
			AnimatorControllerCleaner CleanerInstance = new AnimatorControllerCleaner();
			Asset AssetInstance = new Asset();
			int ModifiedCount = 0;
			try {
				for (int Index = 0; Index < Selection.objects.Length; Index++) {
					string TargetAssetPath = AssetDatabase.GetAssetPath(Selection.objects[Index]);
					string TargetGUID = AssetDatabase.AssetPathToGUID(TargetAssetPath);
					string TargetAssetName = AssetInstance.GUIDToAssetName(TargetGUID, true);
					EditorUtility.DisplayProgressBar("Cleaning AnimatorController",
						$"Processing : {TargetAssetName}",
						(float)Index / Selection.objects.Length);
					if (TargetAssetName.EndsWith("Original")) continue;
					AnimatorController TargetAnimator = AssetDatabase.LoadAssetAtPath<AnimatorController>(TargetAssetName);
					if (CleanerInstance.CleanupAnimatorController(TargetAnimator)) {
						ModifiedCount++;
					}
				}
			} finally {
				EditorUtility.ClearProgressBar();
				AssetDatabase.Refresh();
			}
			Debug.Log($"[VRSuya] Cleaned up AnimatorController in {ModifiedCount} files");
		}

		[MenuItem("Tools/VRSuya/Cleaner/Animator/Clean up All AnimatorController", priority = 1000)]
		static void RequestCleanAllAnimatorController() {
			string[] AssetGUIDs = AssetDatabase.FindAssets("t:AnimatorController", new[] { "Assets/" });
			if (AssetGUIDs.Length > 0) {
				AnimatorControllerCleaner CleanerInstance = new AnimatorControllerCleaner();
				Asset AssetInstance = new Asset();
				int ModifiedCount = 0;
				try {
					for (int Index = 0; Index < AssetGUIDs.Length; Index++) {
						string TargetAssetName = AssetInstance.GUIDToAssetName(AssetGUIDs[Index], true);
						EditorUtility.DisplayProgressBar("Cleaning AnimatorController",
							$"Processing : {TargetAssetName}",
							(float)Index / AssetGUIDs.Length);
						if (TargetAssetName.EndsWith("Original")) continue;
						AnimatorController TargetAnimator = AssetDatabase.LoadAssetAtPath<AnimatorController>(TargetAssetName);
						if (CleanerInstance.CleanupAnimatorController(TargetAnimator)) {
							ModifiedCount++;
						}
					}
				} finally {
					EditorUtility.ClearProgressBar();
					AssetDatabase.Refresh();
				}
				Debug.Log($"[VRSuya] Cleaned up AnimatorController in {ModifiedCount} files");
			}
		}
	}

	public class AnimatorControllerContentCleaner : EditorWindow {

		[MenuItem("Tools/VRSuya/Cleaner/Animator/Clear FX Layer Mask", priority = 1000)]
		static void ClearAllFXLayerMask() {
			string[] FXLayerGUIDs = AssetDatabase.FindAssets("FX t:AnimatorController", new[] { "Assets/" });
			if (FXLayerGUIDs.Length > 0) {
				Asset AssetInstance = new Asset();
				foreach (string TargetFXLayerGUID in FXLayerGUIDs) {
					if (AssetInstance.GUIDToAssetName(TargetFXLayerGUID, true).EndsWith("Original")) continue;
					AnimatorController TargetFXLayer = AssetDatabase.LoadAssetAtPath<AnimatorController>(AssetDatabase.GUIDToAssetPath(TargetFXLayerGUID));
					if (TargetFXLayer) ClearAnimatorMask(TargetFXLayer);
				}
				AssetDatabase.Refresh();
			}
		}

		static void ClearAnimatorMask(AnimatorController TargetAnimator) {
			if (TargetAnimator.layers.Any(Layer => Layer.avatarMask != null)) {
				AnimatorControllerLayer[] NewAnimatorLayers = new AnimatorControllerLayer[TargetAnimator.layers.Length];
				for (int Index = 0; Index < TargetAnimator.layers.Length; Index++) {
					if (TargetAnimator.layers[Index].avatarMask != null) {
						AnimatorControllerLayer NewAnimatorLayer = TargetAnimator.layers[Index];
						NewAnimatorLayer.avatarMask = null;
						NewAnimatorLayers[Index] = NewAnimatorLayer;
					} else {
						NewAnimatorLayers[Index] = TargetAnimator.layers[Index];
					}
				}
				TargetAnimator.layers = NewAnimatorLayers;
				EditorUtility.SetDirty(TargetAnimator);
				AssetDatabase.SaveAssets();
				Debug.Log($"[VRSuya] {TargetAnimator.name} have been cleared mask successfully");
			}
		}

		[MenuItem("Tools/VRSuya/Cleaner/Animator/Clean up FX Layer Transition", priority = 1000)]
		static void CleanupAllFXLayerTransition() {
			string[] FXLayerGUIDs = AssetDatabase.FindAssets("FX t:AnimatorController", new[] { "Assets/" });
			if (FXLayerGUIDs.Length > 0) {
				Asset AssetInstance = new Asset();
				foreach (string TargetFXLayerGUID in FXLayerGUIDs) {
					if (AssetInstance.GUIDToAssetName(TargetFXLayerGUID, true).EndsWith("Original")) continue;
					AnimatorController TargetFXLayer = AssetDatabase.LoadAssetAtPath<AnimatorController>(AssetDatabase.GUIDToAssetPath(TargetFXLayerGUID));
					if (TargetFXLayer) CleanupFXAnimationTransition(TargetFXLayer);
				}
				AssetDatabase.Refresh();
			}
		}

		[MenuItem("Tools/VRSuya/Cleaner/Animator/Clean up Gesture Layer Transition", priority = 1000)]
		static void CleanupAllGestureLayerTransition() {
			string[] GestureLayerGUIDs = AssetDatabase.FindAssets("Gesture t:AnimatorController", new[] { "Assets/" });
			if (GestureLayerGUIDs.Length > 0) {
				Asset AssetInstance = new Asset();
				foreach (string TargetGestureLayerGUID in GestureLayerGUIDs) {
					if (AssetInstance.GUIDToAssetName(TargetGestureLayerGUID, true).EndsWith("Original")) continue;
					AnimatorController TargetGestureLayer = AssetDatabase.LoadAssetAtPath<AnimatorController>(AssetDatabase.GUIDToAssetPath(TargetGestureLayerGUID));
					if (TargetGestureLayer) CleanupGestureAnimationTransition(TargetGestureLayer);
				}
				AssetDatabase.Refresh();
			}
		}

		static void CleanupFXAnimationTransition(AnimatorController TargetAnimator) {
			bool IsDirty = false;
			string[] NeedTimeTransitionLayerName = new string[] { "Left Hand", "Right Hand", "Mouth" };
			foreach (AnimatorControllerLayer TargetLayer in TargetAnimator.layers) {
				if (!NeedTimeTransitionLayerName.Any(LayerName => TargetLayer.name.Contains(LayerName))) {
					AnimatorStateTransition[] NewAnyStateTransition = TargetLayer.stateMachine.anyStateTransitions;
					foreach (AnimatorStateTransition TargetTransition in NewAnyStateTransition) {
						if (TargetTransition.canTransitionToSelf) {
							TargetTransition.canTransitionToSelf = false;
							IsDirty = true;
						}
						if (TargetTransition.duration != 0f) {
							TargetTransition.duration = 0f;
							IsDirty = true;
						}
						if (TargetTransition.exitTime != 0f) {
							TargetTransition.exitTime = 0f;
							IsDirty = true;
						}
						if (TargetTransition.hasExitTime) {
							TargetTransition.hasExitTime = false;
							IsDirty = true;
						}
						if (!TargetTransition.hasFixedDuration) {
							TargetTransition.hasFixedDuration = true;
							IsDirty = true;
						}
					}
					TargetLayer.stateMachine.anyStateTransitions = NewAnyStateTransition;
					foreach (ChildAnimatorState TargetState in TargetLayer.stateMachine.states) {
						AnimatorStateTransition[] NewStateTransition = TargetState.state.transitions;
						foreach (AnimatorStateTransition TargetTransition in NewStateTransition) {
							if (TargetTransition.canTransitionToSelf) {
								TargetTransition.canTransitionToSelf = false;
								IsDirty = true;
							}
							if (TargetTransition.duration != 0f) {
								TargetTransition.duration = 0f;
								IsDirty = true;
							}
							if (TargetTransition.exitTime != 0f) {
								TargetTransition.exitTime = 0f;
								IsDirty = true;
							}
							if (TargetTransition.hasExitTime) {
								TargetTransition.hasExitTime = false;
								IsDirty = true;
							}
							if (!TargetTransition.hasFixedDuration) {
								TargetTransition.hasFixedDuration = true;
								IsDirty = true;
							}
						}
						TargetState.state.transitions = NewStateTransition;
					}
				} else {
					AnimatorStateTransition[] NewAnyStateTransition = TargetLayer.stateMachine.anyStateTransitions;
					foreach (AnimatorStateTransition TargetTransition in NewAnyStateTransition) {
						if (TargetTransition.canTransitionToSelf) {
							TargetTransition.canTransitionToSelf = false;
							IsDirty = true;
						}
						if (TargetTransition.duration != 0.1f) {
							TargetTransition.duration = 0.1f;
							IsDirty = true;
						}
						if (TargetTransition.exitTime != 0f) {
							TargetTransition.exitTime = 0f;
							IsDirty = true;
						}
						if (TargetTransition.hasExitTime) {
							TargetTransition.hasExitTime = false;
							IsDirty = true;
						}
						if (!TargetTransition.hasFixedDuration) {
							TargetTransition.hasFixedDuration = true;
							IsDirty = true;
						}
					}
					TargetLayer.stateMachine.anyStateTransitions = NewAnyStateTransition;
					foreach (ChildAnimatorState TargetState in TargetLayer.stateMachine.states) {
						AnimatorStateTransition[] NewStateTransition = TargetState.state.transitions;
						foreach (AnimatorStateTransition TargetTransition in NewStateTransition) {
							if (TargetTransition.canTransitionToSelf) {
								TargetTransition.canTransitionToSelf = false;
								IsDirty = true;
							}
							if (TargetTransition.duration != 0.1f) {
								TargetTransition.duration = 0.1f;
								IsDirty = true;
							}
							if (TargetTransition.exitTime != 0f) {
								TargetTransition.exitTime = 0f;
								IsDirty = true;
							}
							if (TargetTransition.hasExitTime) {
								TargetTransition.hasExitTime = false;
								IsDirty = true;
							}
							if (!TargetTransition.hasFixedDuration) {
								TargetTransition.hasFixedDuration = true;
								IsDirty = true;
							}
						}
						TargetState.state.transitions = NewStateTransition;
					}
				}
			}
			if (IsDirty) {
				EditorUtility.SetDirty(TargetAnimator);
				AssetDatabase.SaveAssets();
				Debug.Log($"[VRSuya] {TargetAnimator.name} have been cleared mask successfully");
			}
		}

		static void CleanupGestureAnimationTransition(AnimatorController TargetAnimator) {
			bool IsDirty = false;
			foreach (AnimatorControllerLayer TargetLayer in TargetAnimator.layers) {
				AnimatorStateTransition[] NewAnyStateTransition = TargetLayer.stateMachine.anyStateTransitions;
				foreach (AnimatorStateTransition TargetTransition in NewAnyStateTransition) {
					if (TargetTransition.canTransitionToSelf) {
						TargetTransition.canTransitionToSelf = false;
						IsDirty = true;
					}
					if (TargetTransition.duration != 0.1f) {
						TargetTransition.duration = 0.1f;
						IsDirty = true;
					}
					if (TargetTransition.exitTime != 0f) {
						TargetTransition.exitTime = 0f;
						IsDirty = true;
					}
					if (TargetTransition.hasExitTime) {
						TargetTransition.hasExitTime = false;
						IsDirty = true;
					}
					if (!TargetTransition.hasFixedDuration) {
						TargetTransition.hasFixedDuration = true;
						IsDirty = true;
					}
				}
				TargetLayer.stateMachine.anyStateTransitions = NewAnyStateTransition;
				foreach (ChildAnimatorState TargetState in TargetLayer.stateMachine.states) {
					AnimatorStateTransition[] NewStateTransition = TargetState.state.transitions;
					foreach (AnimatorStateTransition TargetTransition in NewStateTransition) {
						if (TargetTransition.canTransitionToSelf) {
							TargetTransition.canTransitionToSelf = false;
							IsDirty = true;
						}
						if (TargetTransition.duration != 0.1f) {
							TargetTransition.duration = 0.1f;
							IsDirty = true;
						}
						if (TargetTransition.exitTime != 0f) {
							TargetTransition.exitTime = 0f;
							IsDirty = true;
						}
						if (TargetTransition.hasExitTime) {
							TargetTransition.hasExitTime = false;
							IsDirty = true;
						}
						if (!TargetTransition.hasFixedDuration) {
							TargetTransition.hasFixedDuration = true;
							IsDirty = true;
						}
					}
					TargetState.state.transitions = NewStateTransition;
				}
				
			}
			if (IsDirty) {
				EditorUtility.SetDirty(TargetAnimator);
				AssetDatabase.SaveAssets();
				Debug.Log($"[VRSuya] {TargetAnimator.name} have been cleared mask successfully");
			}
		}
	}
}