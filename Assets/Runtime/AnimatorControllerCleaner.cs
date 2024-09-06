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
		public AnimatorController TargetAnimatorController = null;
		public string[] TargetfileIDs = new string[0];

		private static string AssetFilePath = string.Empty;
		private static string[] AssetFile = new string[0];

		private static List<string> RootAnimatorStateMachinefileIDs = new List<string>();
		private static List<string> ChildAnimatorStateMachinefileIDs = new List<string>();
		private static List<string> AllAnimatorStateMachinefileIDs = new List<string>();
		private static List<string> AllAnimatorStatefileIDs = new List<string>();
		private static List<string> AllVaildAnimatorStatefileIDs = new List<string>();
		private static List<string> AllAnimatorStateTransitionfileIDs = new List<string>();
		private static List<string> AllVaildAnimatorStateTransitionfileIDs = new List<string>();

		private static readonly string StructureStartPattern = $"--- !u!";
		private static readonly string fileIdPattern = @"fileID:\s*(-?\d+)";
		private static readonly string HeaderfileIdPattern = @"&(-?\d+)";

		/// <summary>파일에서 해당 되는 fileID과 연계된 라인들을 모두 지웁니다.</summary>
		public void RemoveStructureByFileID() {
			if (TargetAnimatorController && TargetfileIDs.Length > 0) {
				AssetFilePath = AssetDatabase.GetAssetPath(TargetAnimatorController);
				if (!string.IsNullOrEmpty(AssetFilePath)) {
					AssetFile = File.ReadAllLines(AssetFilePath);
					List<int> RemoveLineIndex = new List<int>();
					foreach (string TargetfileID in TargetfileIDs) {
						RemoveLineIndex.AddRange(GetRemoveLines(TargetfileID));
					}
					for (int Try = 0; Try < 5; Try++) {
						List<int> TryRemoveLineIndex = RemoveLineIndex.ToList();
						foreach (int TargetIndex in TryRemoveLineIndex) {
							if (AssetFile[TargetIndex].Contains("fileID:")) {
								if (!AssetFile[TargetIndex].Contains("guid:") && !AssetFile[TargetIndex].Contains("m_Motion:")) {
									string newTargetfileID = ExtractFileIDFromLine(AssetFile[TargetIndex]);
									if (!string.IsNullOrEmpty(newTargetfileID)) {
										RemoveLineIndex.AddRange(GetRemoveLines(newTargetfileID));
									}
								}
							}
						}
					}
					if (RemoveLineIndex.Count > 0) {
						List<string> newAssetFile = new List<string>(AssetFile);
						int[] RemoveLineIndexs = RemoveLineIndex.Distinct().ToArray();
						Array.Sort(RemoveLineIndexs);
						Array.Reverse(RemoveLineIndexs);
						foreach (int TargetIndex in RemoveLineIndexs) {
							newAssetFile.RemoveAt(TargetIndex);
						}
						File.WriteAllLines(AssetFilePath, newAssetFile.ToArray());
						Debug.LogWarning("[AnimatorControllerCleaner] AnimatorController에서 총 " + RemoveLineIndex.Count + "줄의 데이터가 정리 되었습니다!");
					} else {
						Debug.Log("[AnimatorControllerCleaner] AnimatorController의 모든 구성요소가 유효합니다!");
					}
				}
			}
			return;
		}

		/// <summary>파일을 분석하여 의미 없는 fileID를 찾습니다.</summary>
		public void GetNULLfileID() {
			if (TargetAnimatorController) {
				AssetFilePath = AssetDatabase.GetAssetPath(TargetAnimatorController);
				if (!string.IsNullOrEmpty(AssetFilePath)) {
					AssetFile = File.ReadAllLines(AssetFilePath);
					RootAnimatorStateMachinefileIDs = GetRootAnimatorStateMachine();
					ChildAnimatorStateMachinefileIDs = new List<string>();
					AllAnimatorStateMachinefileIDs = new List<string>();
					AllAnimatorStatefileIDs = GetAllAnimatorStates();
					AllVaildAnimatorStatefileIDs = new List<string>();
					AllAnimatorStateTransitionfileIDs = GetAllAnimatorStateTransitions();
					AllVaildAnimatorStateTransitionfileIDs = new List<string>();
					if (RootAnimatorStateMachinefileIDs.Count > 0) {
						foreach (string TargetfileID in RootAnimatorStateMachinefileIDs) {
							ChildAnimatorStateMachinefileIDs.AddRange(GetChildAnimatorStateMachine(TargetfileID));
						}
						AllAnimatorStateMachinefileIDs.AddRange(RootAnimatorStateMachinefileIDs);
						AllAnimatorStateMachinefileIDs.AddRange(ChildAnimatorStateMachinefileIDs);
						foreach (string TargetfileID in AllAnimatorStateMachinefileIDs) {
							AllVaildAnimatorStatefileIDs.AddRange(GetAnimatorStates(TargetfileID));
						}
						if (AllAnimatorStatefileIDs.Count > 0 && AllVaildAnimatorStatefileIDs.Count > 0) {
							List<string> InvaildfileIDs = AllAnimatorStatefileIDs
								.Where(Item => !AllVaildAnimatorStatefileIDs
								.Exists(VaildItem => Item == VaildItem)).ToList();
							if (InvaildfileIDs.Count > 0) TargetfileIDs = TargetfileIDs.Concat(InvaildfileIDs.ToArray()).Distinct().ToArray();
						}
						foreach (string TargetfileID in AllVaildAnimatorStatefileIDs) {
							AllVaildAnimatorStateTransitionfileIDs.AddRange(GetAnimatorStateTransitions(TargetfileID));
						}
						if (AllAnimatorStateTransitionfileIDs.Count > 0 && AllVaildAnimatorStateTransitionfileIDs.Count > 0) {
							List<string> UnknownInvaildfileIDs = AllAnimatorStateTransitionfileIDs
								.Where(Item => !AllVaildAnimatorStateTransitionfileIDs
								.Exists(VaildItem => Item == VaildItem)).ToList();
							List<string> VaildfileIDs = new List<string>();
							foreach (string TargetfileID in UnknownInvaildfileIDs) {
								if (VerifyAnimatorStateTransitions(TargetfileID)) VaildfileIDs.Add(TargetfileID);
							}
							List<string> InvaildfileIDs = UnknownInvaildfileIDs
								.Where(Item => !VaildfileIDs
								.Exists(VaildItem => Item == VaildItem)).ToList();
							if (InvaildfileIDs.Count > 0) TargetfileIDs = TargetfileIDs.Concat(InvaildfileIDs.ToArray()).Distinct().ToArray();
						}
					}
				}
			}
			return;
		}

		/// <summary>파일에서 유효한 루트 AnimatorStateMachine들을 fileID들을 반환 합니다.</summary>
		/// <returns>유효한 루트 AnimatorStateMachine들의 fileID 리스트</returns>
		private List<string> GetRootAnimatorStateMachine() {
			List<string> RootAnimatorStateMachinefileIDs = new List<string>();
			bool isAnimatorController = false;
			for (int Line = 0; Line < AssetFile.Length; Line++) {
				if (AssetFile[Line].StartsWith("AnimatorController")) {
					isAnimatorController = true;
					continue;
				}
				if (isAnimatorController && AssetFile[Line].StartsWith(StructureStartPattern)) {
					isAnimatorController = false;
					break;
				}
				if (isAnimatorController && AssetFile[Line].Contains("m_StateMachine")) {
					string TargetfileID = ExtractFileIDFromLine(AssetFile[Line]);
					if (!string.IsNullOrEmpty(TargetfileID)) {
						RootAnimatorStateMachinefileIDs.Add(TargetfileID);
					}
				}
			}
			string[] newRootAnimatorStateMachinefileIDs = RootAnimatorStateMachinefileIDs.ToArray();
			if (newRootAnimatorStateMachinefileIDs.Length > 1) {
				Array.Sort(newRootAnimatorStateMachinefileIDs, (a, b) => string.Compare(a, b, StringComparison.Ordinal));
			}
			return newRootAnimatorStateMachinefileIDs.ToList();
		}

		/// <summary>파일에서 유효한 자식 AnimatorStateMachine들의 fileID들을 재귀적으로 반환 합니다.</summary>
		/// <returns>유효한 자식 AnimatorStateMachine들의 fileID 리스트</returns>
		private List<string> GetChildAnimatorStateMachine(string TargetfileID) {
			List<string> ChildAnimatorStateMachinefileIDs = new List<string>();
			bool isAnimatorStateMachine = false;
			for (int Line = 0; Line < AssetFile.Length; Line++) {
				if (AssetFile[Line].StartsWith(StructureStartPattern) && AssetFile[Line].Contains($"&{TargetfileID}")) {
					isAnimatorStateMachine = true;
					continue;
				}
				if (isAnimatorStateMachine && AssetFile[Line].StartsWith(StructureStartPattern) && !AssetFile[Line].Contains($"&{TargetfileID}")) {
					isAnimatorStateMachine = false;
					break;
				}
				if (isAnimatorStateMachine && AssetFile[Line].Contains("m_StateMachine")) {
					string ChildfileID = ExtractFileIDFromLine(AssetFile[Line]);
					if (!string.IsNullOrEmpty(ChildfileID)) {
						ChildAnimatorStateMachinefileIDs.Add(ChildfileID);
						ChildAnimatorStateMachinefileIDs.AddRange(GetChildAnimatorStateMachine(ChildfileID));
					}
				}
			}
			string[] newChildAnimatorStateMachinefileIDs = ChildAnimatorStateMachinefileIDs.ToArray();
			if (newChildAnimatorStateMachinefileIDs.Length > 1) {
				Array.Sort(newChildAnimatorStateMachinefileIDs, (a, b) => string.Compare(a, b, StringComparison.Ordinal));
			}
			return newChildAnimatorStateMachinefileIDs.ToList();
		}

		/// <summary>파일에서 유효한 AnimatorStateMachine의 AnimatorState fileID들을 반환 합니다.</summary>
		/// <returns>유효한 AnimatorState들의 fileID 리스트</returns>
		private List<string> GetAnimatorStates(string TargetfileID) {
			List<string> AnimatorStatefileIDs = new List<string>();
			bool isAnimatorStateMachine = false;
			for (int Line = 0; Line < AssetFile.Length; Line++) {
				if (AssetFile[Line].StartsWith(StructureStartPattern) && AssetFile[Line].Contains($"&{TargetfileID}")) {
					isAnimatorStateMachine = true;
					continue;
				}
				if (isAnimatorStateMachine && AssetFile[Line].StartsWith(StructureStartPattern) && !AssetFile[Line].Contains($"&{TargetfileID}")) {
					isAnimatorStateMachine = false;
					break;
				}
				if (isAnimatorStateMachine && AssetFile[Line].Contains("m_State")) {
					string ChildfileID = ExtractFileIDFromLine(AssetFile[Line]);
					if (!string.IsNullOrEmpty(ChildfileID)) {
						AnimatorStatefileIDs.Add(ChildfileID);
					}
				}
			}
			string[] newAnimatorStatefileIDs = AnimatorStatefileIDs.ToArray();
			if (newAnimatorStatefileIDs.Length > 1) {
				Array.Sort(newAnimatorStatefileIDs, (a, b) => string.Compare(a, b, StringComparison.Ordinal));
			}
			return newAnimatorStatefileIDs.ToList();
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
			string[] newAnimatorStatefileIDs = AnimatorStatefileIDs.ToArray();
			if (newAnimatorStatefileIDs.Length > 1) {
				Array.Sort(newAnimatorStatefileIDs, (a, b) => string.Compare(a, b, StringComparison.Ordinal));
			}
			return newAnimatorStatefileIDs.ToList();
		}

		/// <summary>State에 존재하는 AnimatorStateTransition fileID들을 반환 합니다.</summary>
		/// <returns>State에 존재하는 AnimatorStateTransition들의 fileID 리스트</returns>
		private List<string> GetAnimatorStateTransitions(string TargetfileID) {
			List<string> AnimatorStateTransitionfileIDs = new List<string>();
			bool isAnimatorState = false;
			bool isAnimatorStateTransition = false;
			for (int Line = 0; Line < AssetFile.Length; Line++) {
				if (AssetFile[Line].StartsWith(StructureStartPattern) && AssetFile[Line].Contains($"&{TargetfileID}")) {
					isAnimatorState = true;
					continue;
				}
				if (isAnimatorState && AssetFile[Line].Contains("m_Transitions")) {
					isAnimatorStateTransition = true;
					continue;
				}
				if (isAnimatorState && AssetFile[Line].StartsWith(StructureStartPattern) && !AssetFile[Line].Contains($"&{TargetfileID}")) {
					isAnimatorState = false;
					break;
				}
				if (isAnimatorState && isAnimatorStateTransition && !AssetFile[Line].Contains("- {fileID: ")) {
					isAnimatorStateTransition = false;
					break;
				}
				if (isAnimatorState && isAnimatorStateTransition && AssetFile[Line].Contains("- {fileID: ")) {
					string ChildfileID = ExtractFileIDFromLine(AssetFile[Line]);
					if (!string.IsNullOrEmpty(ChildfileID)) {
						if (AllVaildAnimatorStatefileIDs.Exists(StatefileID => StatefileID == ChildfileID)) {
							AnimatorStateTransitionfileIDs.Add(ChildfileID);
						}
					}
				}
			}
			string[] newAnimatorStateTransitionfileIDs = AnimatorStateTransitionfileIDs.ToArray();
			if (newAnimatorStateTransitionfileIDs.Length > 1) {
				Array.Sort(newAnimatorStateTransitionfileIDs, (a, b) => string.Compare(a, b, StringComparison.Ordinal));
			}
			return newAnimatorStateTransitionfileIDs.ToList();
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
			string[] newAnimatorStateTransitionfileIDs = AnimatorStateTransitionfileIDs.ToArray();
			if (newAnimatorStateTransitionfileIDs.Length > 1) {
				Array.Sort(newAnimatorStateTransitionfileIDs, (a, b) => string.Compare(a, b, StringComparison.Ordinal));
			}
			return newAnimatorStateTransitionfileIDs.ToList();
		}

		/// <summary>유효한 AnimatorStateTransition fileID인지 여부를 반환 합니다.</summary>
		/// <returns>AnimatorStateTransition fileID 유효 여부</returns>
		private bool VerifyAnimatorStateTransitions(string TargetfileID) {
			bool isAnimatorStateTransition = false;
			for (int Line = 0; Line < AssetFile.Length; Line++) {
				if (AssetFile[Line].StartsWith(StructureStartPattern) && AssetFile[Line].Contains($"&{TargetfileID}")) {
					isAnimatorStateTransition = true;
					continue;
				}
				if (isAnimatorStateTransition && AssetFile[Line].StartsWith(StructureStartPattern) && !AssetFile[Line].Contains($"&{TargetfileID}")) {
					isAnimatorStateTransition = false;
					break;
				}
				if (isAnimatorStateTransition && AssetFile[Line].Contains("m_State")) {
					string ChildfileID = ExtractFileIDFromLine(AssetFile[Line]);
					if (!string.IsNullOrEmpty(ChildfileID)) {
						if (AllVaildAnimatorStatefileIDs.Exists(fileID => ChildfileID == fileID)) {
							return true;
						}
					}
				}
				if (isAnimatorStateTransition && AssetFile[Line].Contains("m_IsExit: 1")) {
					return true;
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