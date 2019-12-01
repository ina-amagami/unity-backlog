/*
unity-backlog

Copyright (c) 2019 ina-amagami (ina@amagamina.jp)

This software is released under the MIT License.
https://opensource.org/licenses/mit-license.php
*/

using UnityEngine;
using UnityEditor;
using System.IO;

namespace Backlog
{
	/// <summary>
	/// Backlog API関連のプロジェクト依存データ
	/// </summary>
	public class BacklogAPIData : ScriptableObject
	{
		/// <summary>
		/// アセットパス
		/// </summary>
		private const string AssetPath = "Backlog/BacklogAPI.asset";

		/// <summary>
		/// リダイレクトURI
		/// </summary>
		[Header("Backlog Developerサイトで登録して下さい")]
		public string RedirectURI;
		/// <summary>
		/// クライアントID
		/// </summary>
		public string ClientId;
		/// <summary>
		/// シークレットID
		/// </summary>
		public string ClientSecretId;

		/// <summary>
		/// スペースキー
		/// </summary>
		[Header("対象プロジェクト")]
		public string SpaceKey;
		/// <summary>
		/// ドメイン
		/// </summary>
		public string Domain;
		/// <summary>
		/// プロジェクトキー
		/// </summary>
		public string ProjectKey;
		/// <summary>
		/// 認証情報のキャッシュデータ保存先
		/// </summary>
		[Header("認証情報のキャッシュファイル名（gitignoreで除外して下さい）")]
		public string CacheFileName = "backlog_oauth2cache.json";

		/// <summary>
		/// APIデータをロード
		/// </summary>
		public static BacklogAPIData Load()
		{
			var asset = EditorGUIUtility.Load(AssetPath);
			if (!asset)
			{
				// 無かったら作成
				CreateAsset();
				asset = EditorGUIUtility.Load(AssetPath);
			}
			return asset as BacklogAPIData;
		}
		
		/// <summary>
		/// アセット作成
		/// </summary>
		public static void CreateAsset()
		{
			var outputPath = "Assets/Editor Default Resources/" + AssetPath;

			var fullDirPath = Path.GetDirectoryName(Application.dataPath.Replace("Assets", outputPath));
			if (!Directory.Exists(fullDirPath))
			{
				Directory.CreateDirectory(fullDirPath);
			}
			
			AssetDatabase.CreateAsset(CreateInstance<BacklogAPIData>(), outputPath);
			AssetDatabase.Refresh();
		}
		
		/// <summary>
		/// SerializedObjectで取得
		/// </summary>
		public static SerializedObject GetSerializedObject()
		{
			return new SerializedObject(Load());
		}
	}
}
