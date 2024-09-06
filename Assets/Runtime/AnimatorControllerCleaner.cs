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
					List<string> RootAnimatorStateMachinefileIDs = GetRootAnimatorStateMachine();
					List<string> ChildAnimatorStateMachinefileIDs = new List<string>();
					List<string> AllAnimatorStateMachinefileIDs = new List<string>();
					List<string> AllAnimatorStatefileIDs = GetAllAnimatorStates();
					List<string> AllVaildAnimatorStatefileIDs = new List<string>();
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
					}
				}
			}
			return;
		}

		/// <summary>파일에서 유효한 루트 AnimatorStateMachine들을 fileID들을 반환 합니다.</summary>
		/// <returns>유효한 루트 AnimatorStateMachine들의 fileID 리스트</returns>
		private List<string> GetRootAnimatorStateMachine() {
			List<string> RootAnimatorStateMachinefileID = new List<string>();
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
						RootAnimatorStateMachinefileID.Add(TargetfileID);
					}
				}
			}
			string[] newRootAnimatorStateMachinefileID = RootAnimatorStateMachinefileID.ToArray();
			if (newRootAnimatorStateMachinefileID.Length > 1) {
				Array.Sort(newRootAnimatorStateMachinefileID, (a, b) => string.Compare(a, b, StringComparison.Ordinal));
			}
			return newRootAnimatorStateMachinefileID.ToList();
		}

		/// <summary>파일에서 유효한 자식 AnimatorStateMachine들의 fileID들을 재귀적으로 반환 합니다.</summary>
		/// <returns>유효한 자식 AnimatorStateMachine들의 fileID 리스트</returns>
		private List<string> GetChildAnimatorStateMachine(string TargetfileID) {
			List<string> ChildAnimatorStateMachinefileID = new List<string>();
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
						ChildAnimatorStateMachinefileID.Add(ChildfileID);
						ChildAnimatorStateMachinefileID.AddRange(GetChildAnimatorStateMachine(ChildfileID));
					}
				}
			}
			string[] newChildAnimatorStateMachinefileID = ChildAnimatorStateMachinefileID.ToArray();
			if (newChildAnimatorStateMachinefileID.Length > 1) {
				Array.Sort(newChildAnimatorStateMachinefileID, (a, b) => string.Compare(a, b, StringComparison.Ordinal));
			}
			return newChildAnimatorStateMachinefileID.ToList();
		}

		/// <summary>파일에서 유효한 AnimatorStateMachine의 AnimatorState fileID들을 반환 합니다.</summary>
		/// <returns>유효한 AnimatorState들의 fileID 리스트</returns>
		private List<string> GetAnimatorStates(string TargetfileID) {
			List<string> AnimatorStatesfileID = new List<string>();
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
						AnimatorStatesfileID.Add(ChildfileID);
					}
				}
			}
			string[] newAnimatorStatesfileID = AnimatorStatesfileID.ToArray();
			if (newAnimatorStatesfileID.Length > 1) {
				Array.Sort(newAnimatorStatesfileID, (a, b) => string.Compare(a, b, StringComparison.Ordinal));
			}
			return newAnimatorStatesfileID.ToList();
		}

		/// <summary>파일에서AnimatorState fileID들을 반환 합니다.</summary>
		/// <returns>AnimatorState들의 fileID 리스트</returns>
		private List<string> GetAllAnimatorStates() {
			List<string> AnimatorStatesfileID = new List<string>();
			for (int Line = 0; Line < AssetFile.Length; Line++) {
				if (AssetFile[Line].StartsWith(StructureStartPattern)) {
					if (AssetFile[Line + 1].Contains("AnimatorState:")) {
						AnimatorStatesfileID.Add(ExtractFileIDFromHeader(AssetFile[Line]));
					}
				}
			}
			string[] newAnimatorStatesfileID = AnimatorStatesfileID.ToArray();
			if (newAnimatorStatesfileID.Length > 1) {
				Array.Sort(newAnimatorStatesfileID, (a, b) => string.Compare(a, b, StringComparison.Ordinal));
			}
			return newAnimatorStatesfileID.ToList();
		}

		/// <summary>파일에서 fileID에 해당되는 라인 인덱스들을 반환 합니다.</summary>
		/// <returns>삭제해야 될 Int 형태의 Index 리스트</returns>
		private List<int> GetRemoveLines(string TargetfileID) {
			List<int> RemoveLineIndex = new List<int>();
			bool isDeleting = false;
			for (int Line = 0; Line < AssetFile.Length; Line++) {
				if (AssetFile[Line].StartsWith(StructureStartPattern) && AssetFile[Line].Contains($"&{TargetfileID}")) {
					isDeleting = true;
					RemoveLineIndex.Add(Line);
					continue;
				}
				if (isDeleting && AssetFile[Line].StartsWith(StructureStartPattern) && !AssetFile[Line].Contains($"&{TargetfileID}")) {
					isDeleting = false;
					break;
				}
				if (isDeleting) {
					if (AssetFile[Line].Contains("fileID:")) {
						if (!AssetFile[Line].Contains("guid:")) {
							string newTargetfileID = ExtractFileIDFromLine(AssetFile[Line]);
							if (!string.IsNullOrEmpty(newTargetfileID)) {
								RemoveLineIndex.AddRange(GetRemoveLines(newTargetfileID));
							}
						}
					}
					RemoveLineIndex.Add(Line);
				}
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