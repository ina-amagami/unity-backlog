/*
unity-backlog

Copyright (c) 2019 ina-amagami (ina@amagamina.jp)

This software is released under the MIT License.
https://opensource.org/licenses/mit-license.php
*/

using UnityEditor;
using UnityEngine;
#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
#else
using UnityEngine.Experimental.UIElements;
#endif

namespace Backlog
{
	/// <summary>
	/// BacklogAPIの設定ファイルをProjectSettingsから編集できるようにする
	/// </summary>
	public class BacklogAPISettings : SettingsProvider
	{
		private const string Path = "Project/BacklogAPI";

		public BacklogAPISettings(string path, SettingsScope scope) : base(path, scope)
		{
		}

		/// <summary>
		/// ProjectSettingsに項目追加
		/// </summary>
		[SettingsProvider]
		private static SettingsProvider Create()
		{
			var provider = new BacklogAPISettings(Path, SettingsScope.Project)
			{
				// 検索対象のキーワード登録（SerializedObjectから自動で取得）
				keywords = GetSearchKeywordsFromSerializedObject(BacklogAPIData.GetSerializedObject())
			};
			return provider;
		}

		private static SerializedObject so;

		public override void OnActivate(string searchContext, VisualElement rootElement)
		{
			// 設定ファイル取得
			so = BacklogAPIData.GetSerializedObject();
		}

		public override void OnGUI(string searchContext)
		{
			// プロパティの表示
			var iterator = so.GetIterator();
			EditorGUI.BeginChangeCheck();
			while (iterator.NextVisible(true))
			{
				bool isScript = iterator.name.Equals("m_Script");
				if (isScript) { GUI.enabled = false; }
				
				EditorGUILayout.PropertyField(iterator);
				
				if (isScript) { GUI.enabled = true; }
			}
			if (EditorGUI.EndChangeCheck())
			{
				so.ApplyModifiedProperties();
			}
			
			EditorGUILayout.Space();
			
			// データ検証用ボタン
			using (new EditorGUILayout.HorizontalScope())
			{
				if (GUILayout.Button("スペースを開く"))
				{
					var data = BacklogAPIData.Load();
					Application.OpenURL($"https://{data.SpaceKey}.{data.Domain}/projects/{data.ProjectKey}");
				}
				if (GUILayout.Button("認証テスト"))
				{
					var backlogAPI = new BacklogAPI();
					try
					{
						backlogAPI.LoadProjectInfo(() =>
						{
							EditorUtility.DisplayDialog("認証成功", "BacklogAPIの認証に成功しました。", "OK");
						});
					}
					catch (System.Exception e)
					{
						Debug.LogException(e);
					}
				}
			}
		}
	}
}