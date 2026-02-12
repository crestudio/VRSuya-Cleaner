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

namespace com.vrsuya.cleaner {

	[ExecuteInEditMode]
	public class RemoveMissingScript : EditorWindow {

		/// <summary>Scene에 존재하는 Missing Script 컴포넌트를 포함한 GameObject를 선택합니다.</summary>
		[MenuItem("Tools/VRSuya/Cleaner/Select All GameObject of Missing Script", priority = 1200)]
		public static void SelectMissingScriptGameObjects() {
			GameObject[] MissingGameObjects = GetAllGameObjectHasMissingScriptComponent();
            if (MissingGameObjects.Length > 0) {
				Selection.objects = MissingGameObjects;
                Debug.Log("[VRSuya] " + MissingGameObjects.Length + " of GameObjects have Missing Component Selected");
			} else {
				Debug.Log("[VRSuya] Not found GameObject has Missing Component");
			}
        }

		/// <summary>Scene에 존재하는 Missing Script 컴포넌트들을 삭제합니다.</summary>
		[MenuItem("Tools/VRSuya/Cleaner/Remove All Missing Script Component", priority = 1200)]
		public static void RemoveMissingScriptComponents() {
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
				Debug.Log("[VRSuya] " + DeletedComponentCount + " of Missing Script Components Removed");
			} else {
				Debug.Log("[VRSuya] Not found Missing Script Component");
			}
		}

		/// <summary>Scene에 존재하는 Missing Script 컴포넌트를 가지고 있는 GameObject 배열을 반환합니다.</summary>
		/// <returns>Scene에 존재하는 모든 Missing Script 컴포넌트를 가지고 있는 GameObject 배열</returns>
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