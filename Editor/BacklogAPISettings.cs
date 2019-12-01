/*
unity-backlog

Copyright (c) 2019 ina-amagami (ina@amagamina.jp)

This software is released under the MIT License.
https://opensource.org/licenses/mit-license.php
*/

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Backlog
{
	/// <summary>
	/// BacklogAPIの設定ファイルをProjectSettingsから編集できるようにする
	/// </summary>
	public class BacklogAPISettings : SettingsProvider
	{
		private const string Path = "Project/BacklogAPI";

		public BacklogAPISettings(string path, SettingsScope scope) : base(path, scope) { }
		
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
				if (iterator.name.Equals("m_Script"))
				{
					continue;
				}
				EditorGUILayout.PropertyField(iterator);
			}
			if (EditorGUI.EndChangeCheck())
			{
				so.ApplyModifiedProperties();
			}
		}
	}
}