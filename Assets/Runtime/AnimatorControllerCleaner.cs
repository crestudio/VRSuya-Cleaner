#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/*
 * VRSuya Cleaner
 * Contact : vrsuya@gmail.com // Twitter : https://twitter.com/VRSuya
 */

namespace VRSuya.Cleaner {

	[ExecuteInEditMode]
	public class AnimatorControllerCleaner : ScriptableObject {

		static string[] TargetRemovefileIDs = new string[0];
		static string AssetFilePath = string.Empty;
		static string[] AssetFile = new string[0];

		static List<string> AllAnimatorStateMachinefileIDs = new List<string>();
		static List<string> AllAnimatorStatefileIDs = new List<string>();
		static List<string> AllAnimatorStateTransitionfileIDs = new List<string>();
		static List<string> AllAnimatorTransitionfileIDs = new List<string>();
		static List<string> AllBlendTreefileIDs = new List<string>();
		static List<string> AllMonoBehaviourfileIDs = new List<string>();

		static List<string> VaildAnimatorStateMachinefileIDs = new List<string>();
		static List<string> VaildAnimatorStatefileIDs = new List<string>();
		static List<string> VaildAnimatorTransitionfileIDs = new List<string>();
		static List<string> VaildAnimatorStateTransitionfileIDs = new List<string>();
		static List<string> VaildBlendTreefileIDs = new List<string>();
		static List<string> VaildMonoBehaviourfileIDs = new List<string>();

		static List<string> InvaildAnimatorStateMachinefileIDs = new List<string>();
		static List<string> InvaildAnimatorStatefileIDs = new List<string>();
		static List<string> InvaildAnimatorTransitionfileIDs = new List<string>();
		static List<string> InvaildAnimatorStateTransitionfileIDs = new List<string>();
		static List<string> InvaildBlendTreefileIDs = new List<string>();
		static List<string> InvaildMonoBehaviourfileIDs = new List<string>();

		static List<int> RemoveLineIndexs = new List<int>();
		static List<int> AdditionRemoveLineIndexs = new List<int>();

		static readonly string StructureStartPattern = $"--- !u!";
		const string HeaderfileIdPattern = @"&(-?\d+)";
		const string LinefileIdPattern = @"fileID:\s*(-?\d+)";

		public bool CleanupAnimatorController(AnimatorController TargetAnimatorController) {
			if (TargetAnimatorController) {
				AssetFilePath = AssetDatabase.GetAssetPath(TargetAnimatorController);
				if (!string.IsNullOrEmpty(AssetFilePath)) {
					AssetFile = File.ReadAllLines(AssetFilePath);
					GetNULLfileIDs(TargetAnimatorController);
					if (TargetRemovefileIDs.Length > 0) {
						RemoveLineIndexs = new List<int>();
						foreach (string TargetfileID in TargetRemovefileIDs) {
							RemoveLineIndexs.AddRange(GetRemoveLineIndexs(TargetfileID));
						}
						for (int Try = 0; Try < 2; Try++) {
							foreach (int TargetIndex in RemoveLineIndexs.ToList()) {
								if (AssetFile[TargetIndex].Contains("fileID:")) {
									if (!AssetFile[TargetIndex].Contains("guid:") && !AssetFile[TargetIndex].Contains("m_Motion:")) {
										string newTargetfileID = ExtractfileIDFromLine(AssetFile[TargetIndex]);
										if (!string.IsNullOrEmpty(newTargetfileID)) {
											if (!Array.Exists(TargetRemovefileIDs, fileID => newTargetfileID == fileID)) {
												if (!VerifyfileID(newTargetfileID)) {
													RemoveLineIndexs.AddRange(GetRemoveLineIndexs(newTargetfileID));
												}
											}
										}
									}
								}
							}
						}
						RemoveLineIndexs.AddRange(AdditionRemoveLineIndexs);
						if (RemoveLineIndexs.Count > 0) {
							List<string> newAssetFile = new List<string>(AssetFile);
							int[] ArrayRemoveLineIndexs = RemoveLineIndexs.Distinct().ToArray();
							Array.Sort(ArrayRemoveLineIndexs);
							Array.Reverse(ArrayRemoveLineIndexs);
							foreach (int TargetIndex in ArrayRemoveLineIndexs) {
								newAssetFile.RemoveAt(TargetIndex);
							}
							File.WriteAllLines(AssetFilePath, newAssetFile.ToArray());
							Debug.LogWarning($"[VRSuya] Cleaned up {ArrayRemoveLineIndexs.Length} lines of unused data from {TargetAnimatorController.name}");
							return true;
						}
					}
				}
			}
			return false;
		}

		void GetNULLfileIDs(AnimatorController TargetAnimatorController) {
			TargetRemovefileIDs = new string[0];
			AdditionRemoveLineIndexs = new List<int>();

			AnalyzeAnimatorController();

			VaildAnimatorStateMachinefileIDs = GetVaildAnimatorStateMachines();
			VaildAnimatorStatefileIDs = new List<string>();
			VaildAnimatorTransitionfileIDs = new List<string>();
			VaildAnimatorStateTransitionfileIDs = new List<string>();
			VaildBlendTreefileIDs = new List<string>();
			VaildMonoBehaviourfileIDs = new List<string>();

			InvaildAnimatorStateMachinefileIDs = GetVaildAnimatorStateMachines();
			InvaildAnimatorStatefileIDs = new List<string>();
			InvaildAnimatorTransitionfileIDs = new List<string>();
			InvaildAnimatorStateTransitionfileIDs = new List<string>();
			InvaildBlendTreefileIDs = new List<string>();
			InvaildMonoBehaviourfileIDs = new List<string>();

			foreach (string TargetfileID in VaildAnimatorStateMachinefileIDs.ToArray()) {
				VaildAnimatorStateMachinefileIDs.AddRange(GetChildAnimatorStateMachines(TargetfileID));
			}
			foreach (string TargetfileID in VaildAnimatorStateMachinefileIDs) {
				VaildAnimatorStatefileIDs.AddRange(GetAnimatorStates(TargetfileID));
			}
			foreach (string TargetfileID in VaildAnimatorStateMachinefileIDs) {
				VaildAnimatorTransitionfileIDs.AddRange(GetAnimatorTransitions(TargetfileID));
				VaildAnimatorTransitionfileIDs.AddRange(GetStateMachineTransitions(TargetfileID, "AnimatorTransition"));
				VaildAnimatorStateTransitionfileIDs.AddRange(GetStateMachineTransitions(TargetfileID, "AnimatorStateTransition"));
			}
			foreach (string TargetfileID in VaildAnimatorStatefileIDs) {
				VaildAnimatorStateTransitionfileIDs.AddRange(GetAnimatorStateTransitions(TargetfileID));
				VaildBlendTreefileIDs.AddRange(GetBlendTrees(TargetfileID));
				VaildMonoBehaviourfileIDs.AddRange(GetMonoBehaviours(TargetfileID));
			}

			if (AllAnimatorStateMachinefileIDs.Count > 0) {
				InvaildAnimatorStateMachinefileIDs = AllAnimatorStateMachinefileIDs
					.Where(Item => !VaildAnimatorStateMachinefileIDs
					.Exists(VaildItem => Item == VaildItem)).ToList();
				if (InvaildAnimatorStateMachinefileIDs.Count > 0) TargetRemovefileIDs = TargetRemovefileIDs.Concat(InvaildAnimatorStateMachinefileIDs.ToArray()).Distinct().ToArray();
			}
			if (AllAnimatorStatefileIDs.Count > 0) {
				InvaildAnimatorStatefileIDs = AllAnimatorStatefileIDs
					.Where(Item => !VaildAnimatorStatefileIDs
					.Exists(VaildItem => Item == VaildItem)).ToList();
				if (InvaildAnimatorStatefileIDs.Count > 0) TargetRemovefileIDs = TargetRemovefileIDs.Concat(InvaildAnimatorStatefileIDs.ToArray()).Distinct().ToArray();
			}
			if (AllAnimatorTransitionfileIDs.Count > 0) {
				InvaildAnimatorTransitionfileIDs = AllAnimatorTransitionfileIDs
					.Where(Item => !VaildAnimatorTransitionfileIDs
					.Exists(VaildItem => Item == VaildItem)).ToList();
				if (InvaildAnimatorTransitionfileIDs.Count > 0) TargetRemovefileIDs = TargetRemovefileIDs.Concat(InvaildAnimatorTransitionfileIDs.ToArray()).Distinct().ToArray();
			}
			if (AllAnimatorStateTransitionfileIDs.Count > 0) {
				InvaildAnimatorStateTransitionfileIDs = AllAnimatorStateTransitionfileIDs
					.Where(Item => !VaildAnimatorStateTransitionfileIDs
					.Exists(VaildItem => Item == VaildItem)).ToList();
				if (InvaildAnimatorStateTransitionfileIDs.Count > 0) TargetRemovefileIDs = TargetRemovefileIDs.Concat(InvaildAnimatorStateTransitionfileIDs.ToArray()).Distinct().ToArray();
			}
			if (AllBlendTreefileIDs.Count > 0) {
				InvaildBlendTreefileIDs = AllBlendTreefileIDs
					.Where(Item => !VaildBlendTreefileIDs
					.Exists(VaildItem => Item == VaildItem)).ToList();
				if (InvaildBlendTreefileIDs.Count > 0) TargetRemovefileIDs = TargetRemovefileIDs.Concat(InvaildBlendTreefileIDs.ToArray()).Distinct().ToArray();
			}
			if (AllMonoBehaviourfileIDs.Count > 0) {
				InvaildMonoBehaviourfileIDs = AllMonoBehaviourfileIDs
					.Where(Item => !VaildMonoBehaviourfileIDs
					.Exists(VaildItem => Item == VaildItem)).ToList();
				if (InvaildMonoBehaviourfileIDs.Count > 0) TargetRemovefileIDs = TargetRemovefileIDs.Concat(InvaildMonoBehaviourfileIDs.ToArray()).Distinct().ToArray();
			}

			Debug.Log($"[VRSuya] {TargetAnimatorController.name} 에셋에 존재하는 구성요소 통계\n" +
				$"StateMachine : {VaildAnimatorStateMachinefileIDs.Count} - {InvaildAnimatorStateMachinefileIDs.Count} = {AllAnimatorStateMachinefileIDs.Count}\n" +
				$"State : {VaildAnimatorStatefileIDs.Count} - {InvaildAnimatorStatefileIDs.Count} = {AllAnimatorStatefileIDs.Count}\n" +
				$"AnimatorTransition : {VaildAnimatorTransitionfileIDs.Count} - {InvaildAnimatorTransitionfileIDs.Count} = {AllAnimatorTransitionfileIDs.Count}\n" +
				$"Transition : {VaildAnimatorStateTransitionfileIDs.Count} - {InvaildAnimatorStateTransitionfileIDs.Count} = {AllAnimatorStateTransitionfileIDs.Count}\n" +
				$"BlendTree : {VaildBlendTreefileIDs.Count} - {InvaildBlendTreefileIDs.Count} = {AllBlendTreefileIDs.Count}\n" +
				$"MonoBehaviour : {VaildMonoBehaviourfileIDs.Count} - {InvaildMonoBehaviourfileIDs.Count} = {AllMonoBehaviourfileIDs.Count}");
		}

		void AnalyzeAnimatorController() {
			AllAnimatorStateMachinefileIDs = new List<string>();
			AllAnimatorStatefileIDs = new List<string>();
			AllAnimatorTransitionfileIDs = new List<string>();
			AllAnimatorStateTransitionfileIDs = new List<string>();
			AllBlendTreefileIDs = new List<string>();
			AllMonoBehaviourfileIDs = new List<string>();
			for (int Line = 0; Line < AssetFile.Length; Line++) {
				if (AssetFile[Line].StartsWith(StructureStartPattern)) {
					switch (AssetFile[Line + 1]) {
						case "AnimatorController:":
							break;
						case "AnimatorStateMachine:":
							AllAnimatorStateMachinefileIDs.Add(ExtractfileIDFromHeader(AssetFile[Line]));
							break;
						case "AnimatorState:":
							AllAnimatorStatefileIDs.Add(ExtractfileIDFromHeader(AssetFile[Line]));
							break;
						case "AnimatorTransition:":
							AllAnimatorTransitionfileIDs.Add(ExtractfileIDFromHeader(AssetFile[Line]));
							break;
						case "AnimatorStateTransition:":
							AllAnimatorStateTransitionfileIDs.Add(ExtractfileIDFromHeader(AssetFile[Line]));
							break;
						case "BlendTree:":
							AllBlendTreefileIDs.Add(ExtractfileIDFromHeader(AssetFile[Line]));
							break;
						case "MonoBehaviour:":
							AllMonoBehaviourfileIDs.Add(ExtractfileIDFromHeader(AssetFile[Line]));
							break;
						default:
							Debug.LogError($"[VRSuya] 알 수 없는 타입 : {AssetFile[Line + 1]}");
							break;
					}
				}
			}
		}

		List<string> GetVaildAnimatorStateMachines() {
			List<string> RootAnimatorStateMachinefileIDs = new List<string>();
			int StartIndex = Array.FindIndex(AssetFile, Line => Line.StartsWith("AnimatorController:"));
			if (StartIndex != -1) {
				for (int Line = StartIndex; Line < AssetFile.Length; Line++) {
					if (AssetFile[Line].StartsWith(StructureStartPattern)) {
						break;
					}
					if (AssetFile[Line].Contains("m_StateMachine:")) {
						string TargetfileID = ExtractfileIDFromLine(AssetFile[Line]);
						if (!string.IsNullOrEmpty(TargetfileID)) {
							int ChildStartIndex = Array.FindIndex(AssetFile, Line => Line.StartsWith(StructureStartPattern) && Line.Contains($"&{TargetfileID}"));
							if (ChildStartIndex != -1) RootAnimatorStateMachinefileIDs.Add(TargetfileID);
						}
					}
				}
			}
			return RootAnimatorStateMachinefileIDs;
		}

		List<string> GetChildAnimatorStateMachines(string TargetfileID) {
			List<string> ChildAnimatorStateMachinefileIDs = new List<string>();
			int StartIndex = Array.FindIndex(AssetFile, Line => Line.StartsWith(StructureStartPattern) && Line.Contains($"&{TargetfileID}"));
			if (StartIndex != -1) {
				for (int Line = StartIndex; Line < AssetFile.Length; Line++) {
					if (AssetFile[Line].StartsWith(StructureStartPattern) && !AssetFile[Line].Contains($"&{TargetfileID}")) {
						break;
					}
					if (AssetFile[Line].Contains("m_StateMachine:")) {
						string ChildfileID = ExtractfileIDFromLine(AssetFile[Line]);
						if (!string.IsNullOrEmpty(ChildfileID)) {
							int ChildStartIndex = Array.FindIndex(AssetFile, Line => Line.StartsWith(StructureStartPattern) && Line.Contains($"&{ChildfileID}"));
							if (ChildStartIndex != -1) {
								ChildAnimatorStateMachinefileIDs.Add(ChildfileID);
								ChildAnimatorStateMachinefileIDs.AddRange(GetChildAnimatorStateMachines(ChildfileID));
							}
						}
					}
				}
			}
			return ChildAnimatorStateMachinefileIDs;
		}

		List<string> GetAnimatorStates(string TargetfileID) {
			List<string> AnimatorStatefileIDs = new List<string>();
			int StartIndex = Array.FindIndex(AssetFile, Line => Line.StartsWith(StructureStartPattern) && Line.Contains($"&{TargetfileID}"));
			if (StartIndex != -1) {
				for (int Line = StartIndex; Line < AssetFile.Length; Line++) {
					if (AssetFile[Line].StartsWith(StructureStartPattern) && !AssetFile[Line].Contains($"&{TargetfileID}")) {
						break;
					}
					if (AssetFile[Line].Contains("m_State:")) {
						string ChildfileID = ExtractfileIDFromLine(AssetFile[Line]);
						if (!string.IsNullOrEmpty(ChildfileID)) {
							int ChildStartIndex = Array.FindIndex(AssetFile, Line => Line.StartsWith(StructureStartPattern) && Line.Contains($"&{ChildfileID}"));
							if (ChildStartIndex != -1) AnimatorStatefileIDs.Add(ChildfileID);
						}
					}
				}
			}
			return AnimatorStatefileIDs;
		}

		List<string> GetAnimatorTransitions(string TargetfileID) {
			List<string> AnimatorTransitionfileIDs = new List<string>();
			bool isAnimatorTransition = false;
			int StartIndex = Array.FindIndex(AssetFile, Line => Line.StartsWith(StructureStartPattern) && Line.Contains($"&{TargetfileID}"));
			if (StartIndex != -1) {
				for (int Line = StartIndex; Line < AssetFile.Length; Line++) {
					if (AssetFile[Line].Contains("m_StateMachineTransitions:")) {
						isAnimatorTransition = true;
						continue;
					}
					if (AssetFile[Line].StartsWith(StructureStartPattern) && !AssetFile[Line].Contains($"&{TargetfileID}")) {
						break;
					}
					if (AssetFile[Line].Contains("m_StateMachineBehaviours:") || AssetFile[Line].Contains("m_DefaultState:")) {
						break;
					}
					if (isAnimatorTransition && AssetFile[Line].Contains("fileID:")) {
						string ChildfileID = ExtractfileIDFromLine(AssetFile[Line]);
						if (!string.IsNullOrEmpty(ChildfileID)) {
							int ChildStartIndex = Array.FindIndex(AssetFile, Line => Line.StartsWith(StructureStartPattern) && Line.Contains($"&{ChildfileID}"));
							if (ChildStartIndex != -1) {
								if (AssetFile[ChildStartIndex + 1] == "AnimatorTransition:") {
									if (VerifyAnimatorTransition(ChildfileID)) {
										AnimatorTransitionfileIDs.Add(ChildfileID);
									}
								}
							}
						}
					}
				}
			}
			return AnimatorTransitionfileIDs;
		}

		List<string> GetStateMachineTransitions(string TargetfileID, string TargetType) {
			List<string> StateMachineTransitionfileIDs = new List<string>();
			bool isAnimatorStateTransition = false;
			string SearchString = (TargetType == "AnimatorTransition") ? "m_EntryTransitions:" : "m_AnyStateTransitions:";
			int StartIndex = Array.FindIndex(AssetFile, Line => Line.StartsWith(StructureStartPattern) && Line.Contains($"&{TargetfileID}"));
			if (StartIndex != -1) {
				for (int Line = StartIndex; Line < AssetFile.Length; Line++) {
					if (AssetFile[Line].Contains(SearchString)) {
						isAnimatorStateTransition = true;
						continue;
					}
					if (AssetFile[Line].StartsWith(StructureStartPattern) && !AssetFile[Line].Contains($"&{TargetfileID}")) {
						break;
					}
					if (isAnimatorStateTransition && !AssetFile[Line].Contains("fileID:")) {
						break;
					}
					if (isAnimatorStateTransition && AssetFile[Line].Contains("fileID:")) {
						string ChildfileID = ExtractfileIDFromLine(AssetFile[Line]);
						if (!string.IsNullOrEmpty(ChildfileID)) {
							if (TargetType == "AnimatorTransition") {
								if (VerifyAnimatorTransition(ChildfileID)) {
									StateMachineTransitionfileIDs.Add(ChildfileID);
								} else {
									AdditionRemoveLineIndexs.Add(Line);
								}
							} else {
								if (VerifyAnimatorStateTransition(ChildfileID)) {
									StateMachineTransitionfileIDs.Add(ChildfileID);
								} else {
									AdditionRemoveLineIndexs.Add(Line);
								}
							}
						}
					}
				}
			}
			return StateMachineTransitionfileIDs;
		}

		List<string> GetAnimatorStateTransitions(string TargetfileID) {
			List<string> AnimatorStateTransitionfileIDs = new List<string>();
			bool isAnimatorStateTransition = false;
			int StartIndex = Array.FindIndex(AssetFile, Line => Line.StartsWith(StructureStartPattern) && Line.Contains($"&{TargetfileID}"));
			if (StartIndex != -1) {
				for (int Line = StartIndex; Line < AssetFile.Length; Line++) {
					if (AssetFile[Line].Contains("m_Transitions:")) {
						isAnimatorStateTransition = true;
						continue;
					}
					if (AssetFile[Line].StartsWith(StructureStartPattern) && !AssetFile[Line].Contains($"&{TargetfileID}")) {
						break;
					}
					if (isAnimatorStateTransition && !AssetFile[Line].Contains("fileID:")) {
						break;
					}
					if (isAnimatorStateTransition && AssetFile[Line].Contains("fileID:")) {
						string ChildfileID = ExtractfileIDFromLine(AssetFile[Line]);
						if (!string.IsNullOrEmpty(ChildfileID)) {
							if (VerifyAnimatorStateTransition(ChildfileID)) {
								AnimatorStateTransitionfileIDs.Add(ChildfileID);
							} else {
								AdditionRemoveLineIndexs.Add(Line);
							}
						}
					}
				}
			}
			return AnimatorStateTransitionfileIDs;
		}

		List<string> GetBlendTrees(string TargetfileID) {
			List<string> BlendTreefileIDs = new List<string>();
			int StartIndex = Array.FindIndex(AssetFile, Line => Line.StartsWith(StructureStartPattern) && Line.Contains($"&{TargetfileID}"));
			if (StartIndex != -1) {
				for (int Line = StartIndex; Line < AssetFile.Length; Line++) {
					if (AssetFile[Line].StartsWith(StructureStartPattern) && !AssetFile[Line].Contains($"&{TargetfileID}")) {
						break;
					}
					if (AssetFile[Line].Contains("m_Motion:")) {
						string ChildfileID = ExtractfileIDFromLine(AssetFile[Line]);
						if (!string.IsNullOrEmpty(ChildfileID)) {
							int ChildStartIndex = Array.FindIndex(AssetFile, Line => Line.StartsWith(StructureStartPattern) && Line.Contains($"&{ChildfileID}"));
							if (ChildStartIndex != -1) {
								if (AssetFile[ChildStartIndex + 1] == "BlendTree:") {
									BlendTreefileIDs.Add(ChildfileID);
									BlendTreefileIDs.AddRange(GetBlendTrees(ChildfileID));
								}
							}
						}
					}
				}
			}
			return BlendTreefileIDs;
		}

		List<string> GetMonoBehaviours(string TargetfileID) {
			List<string> MonoBehaviourfileIDs = new List<string>();
			bool isMonoBehaviour = false;
			int StartIndex = Array.FindIndex(AssetFile, Line => Line.StartsWith(StructureStartPattern) && Line.Contains($"&{TargetfileID}"));
			if (StartIndex != -1) {
				for (int Line = StartIndex; Line < AssetFile.Length; Line++) {
					if (AssetFile[Line].Contains("m_StateMachineBehaviours:")) {
						isMonoBehaviour = true;
						continue;
					}
					if (AssetFile[Line].StartsWith(StructureStartPattern) && !AssetFile[Line].Contains($"&{TargetfileID}")) {
						break;
					}
					if (isMonoBehaviour && !AssetFile[Line].Contains("fileID:")) {
						break;
					}
					if (isMonoBehaviour && AssetFile[Line].Contains("fileID:")) {
						string ChildfileID = ExtractfileIDFromLine(AssetFile[Line]);
						if (!string.IsNullOrEmpty(ChildfileID)) {
							int ChildStartIndex = Array.FindIndex(AssetFile, Line => Line.StartsWith(StructureStartPattern) && Line.Contains($"&{ChildfileID}"));
							if (ChildStartIndex != -1) MonoBehaviourfileIDs.Add(ChildfileID);
						}
					}
				}
			}
			return MonoBehaviourfileIDs;
		}

		bool VerifyfileID(string TargetfileID) {
			int Index = Array.FindIndex(AssetFile, Line => Line.StartsWith(StructureStartPattern) && Line.Contains($"&{TargetfileID}"));
			if (Index != -1) {
				switch (AssetFile[Index + 1]) {
					case "AnimatorStateMachine:":
						if (VaildAnimatorStateMachinefileIDs.Exists(fileID => TargetfileID == fileID)) return true;
						break;
					case "AnimatorState:":
						if (VaildAnimatorStatefileIDs.Exists(fileID => TargetfileID == fileID)) return true;
						break;
					case "AnimatorStateTransition:":
						return VerifyAnimatorStateTransition(TargetfileID);
					case "BlendTree:":
						if (VaildBlendTreefileIDs.Exists(fileID => TargetfileID == fileID)) return true;
						break;
					case "MonoBehaviour:":
						if (VaildMonoBehaviourfileIDs.Exists(fileID => TargetfileID == fileID)) return true;
						break;
				}
			}
			return false;
		}

		bool VerifyAnimatorTransition(string TargetfileID) {
			int StartIndex = Array.FindIndex(AssetFile, Line => Line.StartsWith(StructureStartPattern) && Line.Contains($"&{TargetfileID}"));
			if (StartIndex != -1) {
				for (int Line = StartIndex; Line < AssetFile.Length; Line++) {
					if (AssetFile[Line].StartsWith(StructureStartPattern) && !AssetFile[Line].Contains($"&{TargetfileID}")) {
						break;
					}
					if (AssetFile[Line].Contains("m_DstStateMachine:")) {
						string ChildfileID = ExtractfileIDFromLine(AssetFile[Line]);
						if (!string.IsNullOrEmpty(ChildfileID)) {
							if (VaildAnimatorStateMachinefileIDs.Exists(fileID => ChildfileID == fileID)) {
								return true;
							}
						}
					}
					if (AssetFile[Line].Contains("m_DstState:")) {
						string ChildfileID = ExtractfileIDFromLine(AssetFile[Line]);
						if (!string.IsNullOrEmpty(ChildfileID)) {
							if (VaildAnimatorStatefileIDs.Exists(fileID => ChildfileID == fileID)) {
								return true;
							}
						}
					}
					if (AssetFile[Line].Contains("m_IsExit: 1")) {
						return true;
					}
				}
			}
			return false;
		}

		bool VerifyAnimatorStateTransition(string TargetfileID) {
			int StartIndex = Array.FindIndex(AssetFile, Line => Line.StartsWith(StructureStartPattern) && Line.Contains($"&{TargetfileID}"));
			if (StartIndex != -1) {
				for (int Line = StartIndex; Line < AssetFile.Length; Line++) {
					if (AssetFile[Line].StartsWith(StructureStartPattern) && !AssetFile[Line].Contains($"&{TargetfileID}")) {
						break;
					}
					if (AssetFile[Line].Contains("m_DstStateMachine:")) {
						string ChildfileID = ExtractfileIDFromLine(AssetFile[Line]);
						if (!string.IsNullOrEmpty(ChildfileID)) {
							if (VaildAnimatorStateMachinefileIDs.Exists(fileID => ChildfileID == fileID)) {
								return true;
							}
						}
					}
					if (AssetFile[Line].Contains("m_DstState:")) {
						string ChildfileID = ExtractfileIDFromLine(AssetFile[Line]);
						if (!string.IsNullOrEmpty(ChildfileID)) {
							if (VaildAnimatorStatefileIDs.Exists(fileID => ChildfileID == fileID)) {
								return true;
							}
						}
					}
					if (AssetFile[Line].Contains("m_IsExit: 1")) {
						return true;
					}
				}
			}
			return false;
		}

		List<int> GetRemoveLineIndexs(string TargetfileID) {
			List<int> RemoveLineIndexs = new List<int>();
			int StartIndex = Array.FindIndex(AssetFile, Line => Line.StartsWith(StructureStartPattern) && Line.Contains($"&{TargetfileID}"));
			if (StartIndex != -1) {
				int EndIndex = AssetFile.Length;
				for (int Index = StartIndex; Index < EndIndex; Index++) {
					if (AssetFile[Index].StartsWith(StructureStartPattern) && !AssetFile[Index].Contains($"&{TargetfileID}")) {
						EndIndex = Index;
						break;
					}
				}
				IEnumerable<int> LineIndexs = Enumerable.Range(StartIndex, EndIndex - StartIndex);
				RemoveLineIndexs.AddRange(LineIndexs);
			}
			return RemoveLineIndexs;
		}

		string ExtractfileIDFromHeader(string Line) {
			Match fileIDMatch = Regex.Match(Line, HeaderfileIdPattern);
			if (fileIDMatch.Success) {
				return fileIDMatch.Groups[1].Value;
			} else {
				return string.Empty;
			}
		}

		string ExtractfileIDFromLine(string Line) {
			Match fileIDMatch = Regex.Match(Line, LinefileIdPattern);
			if (fileIDMatch.Success) {
				if (fileIDMatch.Groups[1].Value != "0") {
					return fileIDMatch.Groups[1].Value;
				} else {
					return string.Empty;
				}
			} else {
				return string.Empty;
			}
		}
	}
}
#endif