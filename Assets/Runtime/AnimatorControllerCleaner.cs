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

		private static List<string> RootAnimatorStateMachinefileIDs = new List<string>();
		private static List<string> ChildAnimatorStateMachinefileIDs = new List<string>();
		private static List<string> AllAnimatorStateMachinefileIDs = new List<string>();
		private static List<string> AllAnimatorStatefileIDs = new List<string>();
		private static List<string> AllVaildAnimatorStatefileIDs = new List<string>();
		private static List<string> AllAnimatorStateTransitionfileIDs = new List<string>();
		private static List<string> AllVaildAnimatorStateTransitionfileIDs = new List<string>();

		private static List<int> NeedRemoveLineIndex = new List<int>();

		private static readonly string StructureStartPattern = $"--- !u!";
		private static readonly string fileIdPattern = @"fileID:\s*(-?\d+)";
		private static readonly string HeaderfileIdPattern = @"&(-?\d+)";

		/// <summary>파일에서 해당 되는 fileID과 연계된 라인들을 모두 지웁니다.</summary>
		public void RemoveStructureByFileID() {
			foreach (AnimatorController TargetAnimatorController in TargetAnimatorControllers) {
				if (TargetAnimatorController) {
					AssetFilePath = AssetDatabase.GetAssetPath(TargetAnimatorController);
					if (!string.IsNullOrEmpty(AssetFilePath)) {
						AssetFile = File.ReadAllLines(AssetFilePath);
						GetNULLfileID(TargetAnimatorController);
						TargetRemovefileIDs = TargetRemovefileIDs.Concat(TargetUserRemovefileIDs).ToArray();
						if (TargetRemovefileIDs.Length > 0) {
							List<int> RemoveLineIndex = new List<int>();
							foreach (string TargetfileID in TargetRemovefileIDs) {
								RemoveLineIndex.AddRange(GetRemoveLines(TargetfileID));
							}
							for (int Try = 0; Try < 3; Try++) {
								List<int> TryRemoveLineIndex = RemoveLineIndex.ToList();
								foreach (int TargetIndex in TryRemoveLineIndex) {
									if (AssetFile[TargetIndex].Contains("fileID:")) {
										if (!AssetFile[TargetIndex].Contains("guid:") && !AssetFile[TargetIndex].Contains("m_Motion:")) {
											string newTargetfileID = ExtractFileIDFromLine(AssetFile[TargetIndex]);
											if (!string.IsNullOrEmpty(newTargetfileID)) {
												if (!VerifyfileID(newTargetfileID)) RemoveLineIndex.AddRange(GetRemoveLines(newTargetfileID));
											}
										}
									}
								}
							}
							RemoveLineIndex.AddRange(NeedRemoveLineIndex);
							if (RemoveLineIndex.Count > 0) {
								List<string> newAssetFile = new List<string>(AssetFile);
								int[] RemoveLineIndexs = RemoveLineIndex.Distinct().ToArray();
								Array.Sort(RemoveLineIndexs);
								Array.Reverse(RemoveLineIndexs);
								foreach (int TargetIndex in RemoveLineIndexs) {
									newAssetFile.RemoveAt(TargetIndex);
								}
								File.WriteAllLines(AssetFilePath, newAssetFile.ToArray());
								Debug.LogWarning("[AnimatorControllerCleaner] " + TargetAnimatorController.name + "에서 총 " + RemoveLineIndex.Count + "줄의 데이터가 정리 되었습니다!");
							} else {
								Debug.Log("[AnimatorControllerCleaner] " + TargetAnimatorController.name + "의 모든 구성요소가 유효합니다!");
							}
						} else {
							Debug.Log("[AnimatorControllerCleaner] " + TargetAnimatorController.name + "에는 수정사항이 없습니다!");
						}
					}
				}
			}
			return;
		}

		/// <summary>파일을 분석하여 의미 없는 fileID를 찾습니다.</summary>
		public void GetNULLfileID(AnimatorController TargetAnimatorController) {
			TargetRemovefileIDs = new string[0];
			RootAnimatorStateMachinefileIDs = GetRootAnimatorStateMachine();
			ChildAnimatorStateMachinefileIDs = new List<string>();
			AllAnimatorStateMachinefileIDs = new List<string>();
			AllAnimatorStatefileIDs = GetAllAnimatorStates();
			AllVaildAnimatorStatefileIDs = new List<string>();
			AllAnimatorStateTransitionfileIDs = GetAllAnimatorStateTransitions();
			AllVaildAnimatorStateTransitionfileIDs = new List<string>();
			NeedRemoveLineIndex = new List<int>();
			if (RootAnimatorStateMachinefileIDs.Count > 0) {
				Debug.Log("[AnimatorControllerCleaner] 루트 상태 머신 갯수 : " + RootAnimatorStateMachinefileIDs.Count);
				foreach (string TargetfileID in RootAnimatorStateMachinefileIDs) {
					ChildAnimatorStateMachinefileIDs.AddRange(GetChildAnimatorStateMachine(TargetfileID));
				}
				AllAnimatorStateMachinefileIDs.AddRange(RootAnimatorStateMachinefileIDs);
				AllAnimatorStateMachinefileIDs.AddRange(ChildAnimatorStateMachinefileIDs);
				Debug.Log("[AnimatorControllerCleaner] 모든 상태 머신 갯수 : " + AllAnimatorStateMachinefileIDs.Count);
				Debug.Log("[AnimatorControllerCleaner] 파일에 존재하는 상태 갯수 : " + AllAnimatorStatefileIDs.Count);
				foreach (string TargetfileID in AllAnimatorStateMachinefileIDs) {
					AllVaildAnimatorStatefileIDs.AddRange(GetAnimatorStates(TargetfileID));
				}
				Debug.Log("[AnimatorControllerCleaner] 유효한 상태 갯수 : " + AllVaildAnimatorStatefileIDs.Count);
				if (AllAnimatorStatefileIDs.Count > 0) {
					List<string> InvaildStatefileIDs = AllAnimatorStatefileIDs
						.Where(Item => !AllVaildAnimatorStatefileIDs
						.Exists(VaildItem => Item == VaildItem)).ToList();
					Debug.Log("[AnimatorControllerCleaner] 잘못된 상태 갯수 : " + InvaildStatefileIDs.Count);
					if (InvaildStatefileIDs.Count > 0) TargetRemovefileIDs = TargetRemovefileIDs.Concat(InvaildStatefileIDs.ToArray()).Distinct().ToArray();
				}
				Debug.Log("[AnimatorControllerCleaner] 파일에 존재하는 트랜지션 갯수 : " + AllAnimatorStateTransitionfileIDs.Count);
				foreach (string TargetfileID in AllVaildAnimatorStatefileIDs) {
					AllVaildAnimatorStateTransitionfileIDs.AddRange(GetAnimatorStateTransitions(TargetfileID));
				}
				Debug.Log("[AnimatorControllerCleaner] 상태에 존재하는 트랜지션 갯수 : " + AllVaildAnimatorStateTransitionfileIDs.Count);
				if (AllAnimatorStateTransitionfileIDs.Count > 0) {
					List<string> UnknownTransitionfileIDs = AllAnimatorStateTransitionfileIDs
						.Where(Item => !AllVaildAnimatorStateTransitionfileIDs
						.Exists(VaildItem => Item == VaildItem)).ToList();
					Debug.Log("[AnimatorControllerCleaner] 검사가 필요한 트랜지션 갯수 : " + UnknownTransitionfileIDs.Count);
					List<string> VaildTransitionfileIDs = new List<string>();
					foreach (string TargetfileID in UnknownTransitionfileIDs) {
						if (VerifyAnimatorStateTransitions(TargetfileID)) VaildTransitionfileIDs.Add(TargetfileID);
					}
					Debug.Log("[AnimatorControllerCleaner] 유효한 트랜지션 갯수 : " + VaildTransitionfileIDs.Count);
					List<string> InvaildTransitionfileIDs = UnknownTransitionfileIDs
						.Where(Item => !VaildTransitionfileIDs
						.Exists(VaildItem => Item == VaildItem)).ToList();
					Debug.Log("[AnimatorControllerCleaner] 잘못된 트랜지션 갯수 : " + InvaildTransitionfileIDs.Count);
					if (InvaildTransitionfileIDs.Count > 0) TargetRemovefileIDs = TargetRemovefileIDs.Concat(InvaildTransitionfileIDs.ToArray()).Distinct().ToArray();
				}
			}
			return;
		}

		/// <summary>에셋에서 AnimatorController들을 가져옵니다.</summary>
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

		/// <summary>파일에서 유효한 루트 AnimatorStateMachine들을 fileID들을 반환 합니다.</summary>
		/// <returns>유효한 루트 AnimatorStateMachine들의 fileID 리스트</returns>
		private List<string> GetRootAnimatorStateMachine() {
			List<string> RootAnimatorStateMachinefileIDs = new List<string>();
			int StartIndex = Array.FindIndex(AssetFile, Line => Line.StartsWith("AnimatorController:"));
			if (StartIndex != -1) {
				for (int Line = StartIndex; Line < AssetFile.Length; Line++) {
					if (AssetFile[Line].StartsWith(StructureStartPattern)) {
						break;
					}
					if (AssetFile[Line].Contains("m_StateMachine")) {
						string TargetfileID = ExtractFileIDFromLine(AssetFile[Line]);
						if (!string.IsNullOrEmpty(TargetfileID)) {
							RootAnimatorStateMachinefileIDs.Add(TargetfileID);
						}
					}
				}
			}
			return RootAnimatorStateMachinefileIDs;
		}

		/// <summary>파일에서 유효한 자식 AnimatorStateMachine들의 fileID들을 재귀적으로 반환 합니다.</summary>
		/// <returns>유효한 자식 AnimatorStateMachine들의 fileID 리스트</returns>
		private List<string> GetChildAnimatorStateMachine(string TargetfileID) {
			List<string> ChildAnimatorStateMachinefileIDs = new List<string>();
			int StartIndex = Array.FindIndex(AssetFile, Line => Line.StartsWith(StructureStartPattern) && Line.Contains($"&{TargetfileID}"));
			if (StartIndex != -1) {
				for (int Line = StartIndex; Line < AssetFile.Length; Line++) {
					if (AssetFile[Line].StartsWith(StructureStartPattern) && !AssetFile[Line].Contains($"&{TargetfileID}")) {
						break;
					}
					if (AssetFile[Line].Contains("m_StateMachine:")) {
						string ChildfileID = ExtractFileIDFromLine(AssetFile[Line]);
						if (!string.IsNullOrEmpty(ChildfileID)) {
							ChildAnimatorStateMachinefileIDs.Add(ChildfileID);
							ChildAnimatorStateMachinefileIDs.AddRange(GetChildAnimatorStateMachine(ChildfileID));
						}
					}
				}
			}
			return ChildAnimatorStateMachinefileIDs;
		}

		/// <summary>파일에서 유효한 AnimatorStateMachine의 AnimatorState fileID들을 반환 합니다.</summary>
		/// <returns>유효한 AnimatorState들의 fileID 리스트</returns>
		private List<string> GetAnimatorStates(string TargetfileID) {
			List<string> AnimatorStatefileIDs = new List<string>();
			int StartIndex = Array.FindIndex(AssetFile, Line => Line.StartsWith(StructureStartPattern) && Line.Contains($"&{TargetfileID}"));
			if (StartIndex != -1) {
				for (int Line = StartIndex; Line < AssetFile.Length; Line++) {
					if (AssetFile[Line].StartsWith(StructureStartPattern) && !AssetFile[Line].Contains($"&{TargetfileID}")) {
						break;
					}
					if (AssetFile[Line].Contains("m_State:")) {
						string ChildfileID = ExtractFileIDFromLine(AssetFile[Line]);
						if (!string.IsNullOrEmpty(ChildfileID)) {
							AnimatorStatefileIDs.Add(ChildfileID);
						}
					}
				}
			}
			return AnimatorStatefileIDs;
		}

		/// <summary>파일에서 AnimatorState fileID들을 반환 합니다.</summary>
		/// <returns>AnimatorState들의 fileID 리스트</returns>
		private List<string> GetAllAnimatorStates() {
			List<string> AnimatorStatefileIDs = new List<string>();
			for (int Line = 0; Line < AssetFile.Length; Line++) {
				if (AssetFile[Line].StartsWith(StructureStartPattern)) {
					if (AssetFile[Line + 1].Contains("AnimatorState:")) {
						AnimatorStatefileIDs.Add(ExtractFileIDFromHeader(AssetFile[Line]));
					}
				}
			}
			return AnimatorStatefileIDs;
		}

		/// <summary>State에 존재하는 AnimatorStateTransition fileID들을 반환 합니다.</summary>
		/// <returns>State에 존재하는 AnimatorStateTransition들의 fileID 리스트</returns>
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
						string ChildfileID = ExtractFileIDFromLine(AssetFile[Line]);
						if (!string.IsNullOrEmpty(ChildfileID)) {
							if (VerifyAnimatorStateTransitions(ChildfileID)) {
								AnimatorStateTransitionfileIDs.Add(ChildfileID);
							} else {
								NeedRemoveLineIndex.Add(Line);
							}
						}
					}
				}
			}
			return AnimatorStateTransitionfileIDs;
		}

		/// <summary>파일에서 AnimatorStateTransition fileID들을 반환 합니다.</summary>
		/// <returns>AnimatorStateTransition들의 fileID 리스트</returns>
		private List<string> GetAllAnimatorStateTransitions() {
			List<string> AnimatorStateTransitionfileIDs = new List<string>();
			for (int Line = 0; Line < AssetFile.Length; Line++) {
				if (AssetFile[Line].StartsWith(StructureStartPattern)) {
					if (AssetFile[Line + 1].Contains("AnimatorStateTransition:")) {
						AnimatorStateTransitionfileIDs.Add(ExtractFileIDFromHeader(AssetFile[Line]));
					}
				}
			}
			return AnimatorStateTransitionfileIDs;
		}

		/// <summary>유효한 AnimatorStateTransition fileID인지 여부를 반환 합니다.</summary>
		/// <returns>AnimatorStateTransition fileID 유효 여부</returns>
		private bool VerifyAnimatorStateTransitions(string TargetfileID) {
			int StartIndex = Array.FindIndex(AssetFile, Line => Line.StartsWith(StructureStartPattern) && Line.Contains($"&{TargetfileID}"));
			if (StartIndex != -1) {
				for (int Line = StartIndex; Line < AssetFile.Length; Line++) {
					if (AssetFile[Line].StartsWith(StructureStartPattern) && !AssetFile[Line].Contains($"&{TargetfileID}")) {
						break;
					}
					if (AssetFile[Line].Contains("m_DstState:")) {
						string ChildfileID = ExtractFileIDFromLine(AssetFile[Line]);
						if (!string.IsNullOrEmpty(ChildfileID)) {
							if (AllVaildAnimatorStatefileIDs.Exists(fileID => ChildfileID == fileID)) {
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

		/// <summary>유효한 fileID인지 여부를 반환 합니다.</summary>
		/// <returns>fileID 유효 여부</returns>
		private bool VerifyfileID(string TargetfileID) {
			int Index = Array.FindIndex(AssetFile, Line => Line.StartsWith(StructureStartPattern) && Line.Contains($"&{TargetfileID}"));
			if (Index != -1) {
				if (AssetFile[Index + 1].Contains("AnimatorState:")) {
					if (AllVaildAnimatorStatefileIDs.Exists(fileID => TargetfileID == fileID)) return true;
				}
				if (AssetFile[Index + 1].Contains("AnimatorStateTransition:")) {
					return VerifyAnimatorStateTransitions(TargetfileID);
				}
			}
			return false;
		}

		/// <summary>파일에서 fileID에 해당되는 라인 인덱스들을 반환 합니다.</summary>
		/// <returns>삭제해야 될 Int 형태의 Index 리스트</returns>
		private List<int> GetRemoveLines(string TargetfileID) {
			List<int> RemoveLineIndex = new List<int>();
			int StartIndex = Array.FindIndex(AssetFile, Line => Line.StartsWith(StructureStartPattern) && Line.Contains($"&{TargetfileID}"));
			if (StartIndex != -1) {
				int EndIndex = AssetFile.Length;
				for (int Index = StartIndex; Index < EndIndex; Index++) {
					if (AssetFile[Index].StartsWith(StructureStartPattern) && !AssetFile[Index].Contains($"&{TargetfileID}")) {
						EndIndex = Index;
						break;
					}
				}
				var Indexs = Enumerable.Range(StartIndex, EndIndex - StartIndex);
				RemoveLineIndex.AddRange(Indexs);
			}
			return RemoveLineIndex;
		}

		/// <summary>해당 라인에 유효한 fileID가 있다면 해당 값을 반환합니다.</summary>
		/// <returns>String 형태의 fileID</returns>
		private string ExtractFileIDFromLine(string Line) {
			Match fileIDMatch = Regex.Match(Line, fileIdPattern);
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

		/// <summary>해당 헤더에 유효한 fileID가 있다면 해당 값을 반환합니다.</summary>
		/// <returns>String 형태의 fileID</returns>
		private string ExtractFileIDFromHeader(string Line) {
			Match fileIDMatch = Regex.Match(Line, HeaderfileIdPattern);
			if (fileIDMatch.Success) {
				return fileIDMatch.Groups[1].Value;
			} else {
				return string.Empty;
			}
		}
	}
}
#endif