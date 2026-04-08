using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

using static VRSuya.Core.Unity;

/*
 * VRSuya Cleaner
 * Contact : vrsuya@gmail.com // Twitter : https://twitter.com/VRSuya
 */

namespace VRSuya.Cleaner {

	[ExecuteInEditMode]
	public class RemoveMissingScript : EditorWindow {

		[MenuItem("Tools/VRSuya/Cleaner/Scene/Select All GameObject of Missing Script", priority = 1000)]
		static void SelectMissingScriptGameObjects() {
			GameObject[] MissingGameObjects = GetAllGameObjectHasMissingScriptComponent();
            if (MissingGameObjects.Length > 0) {
				Selection.objects = MissingGameObjects;
                Debug.Log($"[VRSuya] {MissingGameObjects.Length} of GameObjects have Missing Component Selected");
			} else {
				Debug.Log($"[VRSuya] Not found GameObject has Missing Component");
			}
        }

		[MenuItem("Tools/VRSuya/Cleaner/Scene/Remove All Missing Script Component", priority = 1000)]
		static void RemoveMissingScriptComponents() {
			GameObject[] MissingGameObjects = GetAllGameObjectHasMissingScriptComponent();
			if (MissingGameObjects.Length > 0) {
				string UndoGroupName = "Remove All Missing Component";
				int UndoGroupIndex = InitializeUndoGroup(UndoGroupName);
				int DeletedComponentCount = 0;
				foreach (GameObject TargetGameObject in MissingGameObjects) {
					Undo.RecordObject(TargetGameObject, UndoGroupName);
					DeletedComponentCount = DeletedComponentCount + GameObjectUtility.RemoveMonoBehavioursWithMissingScript(TargetGameObject);
					EditorUtility.SetDirty(TargetGameObject);
					Undo.CollapseUndoOperations(UndoGroupIndex);
				}
				Debug.Log($"[VRSuya] {DeletedComponentCount} of Missing Script Components Removed");
			} else {
				Debug.Log($"[VRSuya] Not found Missing Script Component");
			}
		}

		static GameObject[] GetAllGameObjectHasMissingScriptComponent() {
			List<GameObject> MissingGameObjects = new List<GameObject>();
			Transform[] AllTransforms = SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(gameObject => gameObject.GetComponentsInChildren<Transform>(true)).ToArray();
			foreach (Transform TargetTransform in AllTransforms) {
				if (Array.Exists(TargetTransform.GetComponents<Component>(), TargetComponent => TargetComponent == null)) {
					MissingGameObjects.Add(TargetTransform.gameObject);
				}
			}
            return MissingGameObjects.ToArray();
        }
    }
}