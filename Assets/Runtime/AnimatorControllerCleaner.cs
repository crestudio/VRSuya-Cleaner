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
 * VRSuya AnimatorControllerCleaner
 * Contact : vrsuya@gmail.com // Twitter : https://twitter.com/VRSuya
 */

namespace com.vrsuya.animationcleaner {

	[ExecuteInEditMode]
	public class AnimatorControllerCleaner : ScriptableObject {

		[SerializeField]
		public string TargetFolderPath = string.Empty;
		public AnimatorController[] TargetAnimatorControllers = new AnimatorController[0];
		public string[] TargetUserRemovefileIDs = new string[0];

		private static string[] TargetRemovefileIDs = new string[0];
		private static string AssetFilePath = string.Empty;
		private static string[] AssetFile = new string[0];

		private static List<string> AllAnimatorStateMachinefileIDs = new List<string>();
		private static List<string> AllAnimatorStatefileIDs = new List<string>();
		private static List<string> AllAnimatorStateTransitionfileIDs = new List<string>();
		private static List<string> AllAnimatorTransitionfileIDs = new List<string>();
		private static List<string> AllBlendTreefileIDs = new List<string>();
		private static List<string> AllMonoBehaviourfileIDs = new List<string>();

		private static List<string> VaildAnimatorStateMachinefileIDs = new List<string>();
		private static List<string> VaildAnimatorStatefileIDs = new List<string>();
		private static List<string> VaildAnimatorTransitionfileIDs = new List<string>();
		private static List<string> VaildAnimatorStateTransitionfileIDs = new List<string>();
		private static List<string> VaildBlendTreefileIDs = new List<string>();
		private static List<string> VaildMonoBehaviourfileIDs = new List<string>();

		private static List<string> InvaildAnimatorStateMachinefileIDs = new List<string>();
		private static List<string> InvaildAnimatorStatefileIDs = new List<string>();
		private static List<string> InvaildAnimatorTransitionfileIDs = new List<string>();
		private static List<string> InvaildAnimatorStateTransitionfileIDs = new List<string>();
		private static List<string> InvaildBlendTreefileIDs = new List<string>();
		private static List<string> InvaildMonoBehaviourfileIDs = new List<string>();

		private static List<int> RemoveLineIndexs = new List<int>();
		private static List<int> AdditionRemoveLineIndexs = new List<int>();

		private static readonly string StructureStartPattern = $"--- !u!";
		private static readonly string HeaderfileIdPattern = @"&(-?\d+)";
		private static readonly string LinefileIdPattern = @"fileID:\s*(-?\d+)";

		/*
		 * 프로그램의 메인 메소드
		 */

		/// <summary>AnimatorController 에셋을 분석하여 의미 없는 fileID과 연계된 라인들을 모두 지웁니다.</summary>
		public void RemoveStructureByFileID() {
			foreach (AnimatorController TargetAnimatorController in TargetAnimatorControllers) {
				if (TargetAnimatorController) {
					AssetFilePath = AssetDatabase.GetAssetPath(TargetAnimatorController);
					if (!string.IsNullOrEmpty(AssetFilePath)) {
						AssetFile = File.ReadAllLines(AssetFilePath);
						GetNULLfileIDs(TargetAnimatorController);
						TargetRemovefileIDs = TargetRemovefileIDs.Concat(TargetUserRemovefileIDs).Distinct().ToArray();
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
								Debug.LogWarning("[AnimatorControllerCleaner] " + TargetAnimatorController.name + "에서 총 " + ArrayRemoveLineIndexs.Length + "줄의 데이터가 정리 되었습니다!");
							}
						} else {
							Debug.Log("[AnimatorControllerCleaner] " + TargetAnimatorController.name + "에는 모든 fileID가 유효합니다!");
						}
					}
				}
			}
			return;
		}

		/// <summary>AnimatorController 에셋을 분석하여 의미 없는 fileID를 찾습니다.</summary>
		private void GetNULLfileIDs(AnimatorController TargetAnimatorController) {
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

			Debug.Log("[AnimatorControllerCleaner] " + TargetAnimatorController.name + " 에셋에 존재하는 구성요소 통계\r\n" +
				"StateMachine : " + VaildAnimatorStateMachinefileIDs.Count + " - " + InvaildAnimatorStateMachinefileIDs.Count + " = " + AllAnimatorStateMachinefileIDs.Count + "\r\n" +
				"State : " + VaildAnimatorStatefileIDs.Count + " - " + InvaildAnimatorStatefileIDs.Count + " = " + AllAnimatorStatefileIDs.Count + "\r\n" +
				"AnimatorTransition : " + VaildAnimatorTransitionfileIDs.Count + " - " + InvaildAnimatorTransitionfileIDs.Count + " = " + AllAnimatorTransitionfileIDs.Count + "\r\n" +
				"Transition : " + VaildAnimatorStateTransitionfileIDs.Count + " - " + InvaildAnimatorStateTransitionfileIDs.Count + " = " + AllAnimatorStateTransitionfileIDs.Count + "\r\n" +
				"BlendTree : " + VaildBlendTreefileIDs.Count + " - " + InvaildBlendTreefileIDs.Count + " = " + AllBlendTreefileIDs.Count + "\r\n" +
				"MonoBehaviour : " + VaildMonoBehaviourfileIDs.Count + " - " + InvaildMonoBehaviourfileIDs.Count + " = " + AllMonoBehaviourfileIDs.Count);
			return;
		}

		/// <summary>에셋 라이브러리에서 AnimatorController 에셋들을 가져와서 추가합니다.</summary>
		public void AddAnimatorControllers() {
			List<AnimatorController> ListAnimatorController = new List<AnimatorController>();
			string[] AnimatorControllerGUIDs = AssetDatabase.FindAssets("t:AnimatorController", new[] { "Assets\\" + TargetFolderPath });
			foreach (string TargetAnimatorControllerGUID in AnimatorControllerGUIDs) {
				AnimatorController TargetAnimatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>(AssetDatabase.GUIDToAssetPath(TargetAnimatorControllerGUID));
				if (TargetAnimatorController is AnimatorController) {
					ListAnimatorController.Add(TargetAnimatorController);
				}
			}
			if (ListAnimatorController.Count > 0) {
				AnimatorController[] newAnimatorControllers = ListAnimatorController.ToArray();
				Array.Sort(newAnimatorControllers, (a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal));
				TargetAnimatorControllers = TargetAnimatorControllers.Concat(newAnimatorControllers).ToArray();
			}
			return;
		}

		/*
		 * 파일에 존재하는 모든 Type을 반환 받는 메소드
		 */

		/// <summary>파일에 존재하는 모든 fileID들을 분석하여 Type에 맞게 변수에 추가합니다.</summary>
		private void AnalyzeAnimatorController() {
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
							Debug.LogError("[AnimatorControllerCleaner] 알 수 없는 타입 : " + AssetFile[Line + 1]);
							break;
					}
				}
			}
			return;
		}

		/*
		 * 유효한 모든 Type을 반환 받는 메소드
		 */

		/// <summary>파일에서 유효한 루트 StateMachine들의 fileID들을 반환합니다.</summary>
		/// <returns>유효한 루트 StateMachine들의 fileID 리스트</returns>
		private List<string> GetVaildAnimatorStateMachines() {
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

		/// <summary>fileID에서 자식 StateMachine들의 fileID들을 재귀적으로 반환합니다.</summary>
		/// <returns>모든 자식 StateMachine들의 fileID 리스트</returns>
		private List<string> GetChildAnimatorStateMachines(string TargetfileID) {
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

		/// <summary>fileID에서 StateMachine의 State fileID들을 반환합니다.</summary>
		/// <returns>모든 State들의 fileID 리스트</returns>
		private List<string> GetAnimatorStates(string TargetfileID) {
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

		/// <summary>fileID에서 StateMachine의 AnimatorTransition fileID들을 반환합니다.</summary>
		/// <returns>모든 AnimatorTransition들의 fileID 리스트</returns>
		private List<string> GetAnimatorTransitions(string TargetfileID) {
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

		/// <summary>fileID에서 StateMachine의 Transition fileID들을 반환합니다.</summary>
		/// <returns>모든 Transition들의 fileID 리스트</returns>
		private List<string> GetStateMachineTransitions(string TargetfileID, string TargetType) {
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

		/// <summary>fileID에서 State의 Transition fileID들을 반환합니다.</summary>
		/// <returns>모든 Transition들의 fileID 리스트</returns>
		private List<string> GetAnimatorStateTransitions(string TargetfileID) {
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

		/// <summary>fileID에서 State의 BlendTree fileID들을 재귀적으로 반환합니다.</summary>
		/// <returns>모든 BlendTree들의 fileID 리스트</returns>
		private List<string> GetBlendTrees(string TargetfileID) {
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

		/// <summary>fileID에서 State의 MonoBehaviour fileID들을 반환합니다.</summary>
		/// <returns>모든 MonoBehaviour들의 fileID 리스트</returns>
		private List<string> GetMonoBehaviours(string TargetfileID) {
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

		/*
		 * fileID가 유효한지 판단하는 메소드
		 */

		/// <summary>유효한 fileID인지 여부를 반환 합니다.</summary>
		/// <returns>fileID 유효 여부</returns>
		private bool VerifyfileID(string TargetfileID) {
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

		/// <summary>유효한 AnimatorTransition fileID인지 여부를 반환 합니다.</summary>
		/// <returns>AnimatorTransition fileID 유효 여부</returns>
		private bool VerifyAnimatorTransition(string TargetfileID) {
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

		/// <summary>유효한 AnimatorStateTransition fileID인지 여부를 반환 합니다.</summary>
		/// <returns>AnimatorStateTransition fileID 유효 여부</returns>
		private bool VerifyAnimatorStateTransition(string TargetfileID) {
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

		/*
		 * 클래스가 작동하는데 필요한 메소드
		 */

		/// <summary>파일에서 fileID에 해당되는 라인 인덱스들을 반환 합니다.</summary>
		/// <returns>삭제해야 될 Int 형태의 Index 리스트</returns>
		private List<int> GetRemoveLineIndexs(string TargetfileID) {
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

		/// <summary>해당 헤더에 유효한 fileID가 있다면 해당 값을 반환합니다.</summary>
		/// <returns>String 형태의 fileID</returns>
		private string ExtractfileIDFromHeader(string Line) {
			Match fileIDMatch = Regex.Match(Line, HeaderfileIdPattern);
			if (fileIDMatch.Success) {
				return fileIDMatch.Groups[1].Value;
			} else {
				return string.Empty;
			}
		}

		/// <summary>해당 라인에 유효한 fileID가 있다면 해당 값을 반환합니다.</summary>
		/// <returns>String 형태의 fileID</returns>
		private string ExtractfileIDFromLine(string Line) {
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