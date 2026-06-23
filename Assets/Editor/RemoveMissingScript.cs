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

	public class RemoveMissingScript : EditorWindow {

		const string UndoGroupName = "VRSuya RemoveMissingScript";

		[MenuItem("Tools/VRSuya/Cleaner/Scene/Select All GameObject of Missing Script", priority = 1000)]
		static void SelectMissingScriptGameObjects() {
			GameObject[] MissingGameObjects = GetAllGameObjectHasMissingScriptComponent();
            if (MissingGameObjects.Length > 0) {
				Selection.objects = MissingGameObjects;
                Debug.LogWarning($"[VRSuya] {MissingGameObjects.Length} of GameObjects have Missing Component Selected");
			} else {
				Debug.Log($"[VRSuya] Not found GameObject has Missing Component");
			}
        }

		[MenuItem("Tools/VRSuya/Cleaner/Scene/Remove All Missing Script Component", priority = 1000)]
		static void RemoveMissingScriptComponents() {
			GameObject[] MissingGameObjects = GetAllGameObjectHasMissingScriptComponent();
			if (MissingGameObjects.Length > 0) {
				int UndoGroupIndex = InitializeUndoGroup(UndoGroupName);
				int DeletedComponentCount = 0;
				foreach (GameObject TargetGameObject in MissingGameObjects) {
					Undo.RecordObject(TargetGameObject, UndoGroupName);
					DeletedComponentCount = DeletedComponentCount + GameObjectUtility.RemoveMonoBehavioursWithMissingScript(TargetGameObject);
					EditorUtility.SetDirty(TargetGameObject);
					Undo.CollapseUndoOperations(UndoGroupIndex);
				}
				Debug.LogWarning($"[VRSuya] {DeletedComponentCount} of Missing Script Components Removed");
			} else {
				Debug.Log($"[VRSuya] Not found Missing Script Component");
			}
		}

		static GameObject[] GetAllGameObjectHasMissingScriptComponent() {
			return SceneManager.GetActiveScene().GetRootGameObjects()
				.SelectMany(Item => Item.GetComponentsInChildren<Transform>(true))
				.Where(Item => Item.GetComponents<Component>()
					.Any(Item => Item == null)
				)
				.Select(Item => Item.gameObject)
				.ToArray();
		}
    }
}