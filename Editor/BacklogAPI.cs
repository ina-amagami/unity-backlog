/*
unity-backlog

Copyright (c) 2019 ina-amagami (ina@amagamina.jp)

This software is released under the MIT License.
https://opensource.org/licenses/mit-license.php
*/

using System;
using System.Linq;
using UnityEngine;
using NBacklog;
using NBacklog.DataTypes;
using NBacklog.OAuth2;
using System.IO;
using UnityEditor;

namespace Backlog
{
	/// <summary>
	/// BacklogAPI
	/// </summary>
	public class BacklogAPI
	{
		/// <summary>
		/// APIデータ
		/// </summary>
		public BacklogAPIData APIData { get; private set; }
		/// <summary>
		/// スペース
		/// </summary>
		public NBacklog.DataTypes.Space Space { get; private set; }
		/// <summary>
		/// プロジェクト
		/// </summary>
		public Project Project { get; private set; }

		/// <summary>
		/// プロジェクト情報
		/// </summary>
		public class ProjectData
		{
			/// <summary>
			/// チケットタイプ
			/// </summary>
			public TicketType[] TicketTypes;
			/// <summary>
			/// 優先度
			/// </summary>
			public Priority[] Priorities;
			/// <summary>
			/// カテゴリ
			/// </summary>
			public Category[] Categories;
			/// <summary>
			/// マイルストーン
			/// </summary>
			public Milestone[] Milestones;
			/// <summary>
			/// ユーザー
			/// </summary>
			public User[] Users;
		}
		public ProjectData Data { get; } = new ProjectData();

		/// <summary>
		/// プロジェクト情報の取得
		/// </summary>
		public async void LoadProjectInfo(Action onSuccess = null)
		{
			APIData = BacklogAPIData.Load();

			// エディタが再生中かつ一時停止中だと認証時にawaitで止まってしまうので、キャッシュがない時は一時停止を解除する
			bool isPaused = EditorApplication.isPaused;
			if (EditorApplication.isPlaying)
			{
				bool isCached = File.Exists($"{Application.dataPath}/../{APIData.CacheFileName}");
				if (!isCached)
				{
					EditorApplication.isPaused = false;
				}
			}

			// 認証
			var client = new BacklogClient(APIData.SpaceKey, APIData.Domain);
			await client.AuthorizeAsync(new OAuth2App()
			{
				ClientId = APIData.ClientId,
				ClientSecret = APIData.ClientSecretId,
				RedirectUri = APIData.RedirectURI,
				CredentialsCachePath = APIData.CacheFileName,
			});

			EditorApplication.isPaused = isPaused;

			// 各種データ取得
			Space = client.GetSpaceAsync().Result.Content;
			Project = client.GetProjectAsync(APIData.ProjectKey).Result.Content;
			Data.TicketTypes = Project.GetTicketTypesAsync().Result.Content;
			Data.Priorities = client.GetPriorityTypesAsync().Result.Content;
			Data.Categories = Project.GetCategoriesAsync().Result.Content;
			Data.Milestones = Project.GetMilestonesAsync().Result.Content;
			Data.Users = Project.GetUsersAsync().Result.Content;

			onSuccess?.Invoke();
		}

		/// <summary>
		/// キーからチケットを取得
		/// </summary>
		public Ticket GetTicketByKey(string key)
		{
			return Project.GetTicketAsync(key).Result.Content;
		}

		/// <summary>
		/// チケットを追加
		/// </summary>
		public Ticket AddTicket(Ticket ticket)
		{
			var res = Project.AddTicketAsync(ticket).Result;
			if (CheckIsRetry(res))
			{
				// トランザクション系のエラーだったらリトライ
				res = Project.AddTicketAsync(ticket).Result;
			}
			return GetResult(res);
		}

		/// <summary>
		/// チケットの更新
		/// </summary>
		public Ticket UpdateTicket(Ticket ticket)
		{
			var res = Project.UpdateTicketAsync(ticket).Result;
			if (CheckIsRetry(res))
			{
				// トランザクション系のエラーだったらリトライ
				res = Project.UpdateTicketAsync(ticket).Result;
			}
			return GetResult(res);
		}

		/// <summary>
		/// スペースに添付ファイルを追加
		/// </summary>
		public Attachment AddAttachment(string filePath)
		{
			var fileInfo = new System.IO.FileInfo(filePath);
			var res = Space.AddAttachment(fileInfo).Result;
			if (CheckIsRetry(res))
			{
				// トランザクション系のエラーだったらリトライ
				res = Space.AddAttachment(fileInfo).Result;
			}
			return GetResult(res);
		}

		/// <summary>
		/// トランザクション系のエラーで失敗しているかチェック
		/// </summary>
		public bool CheckIsRetry<T>(BacklogResponse<T> res)
		{
			return !res.IsSuccess && res.Errors.Any(x => x.Message.StartsWith("Deadlock"));
		}

		/// <summary>
		/// レスポンスの結果を取得
		/// </summary>
		public T GetResult<T>(BacklogResponse<T> res) where T : BacklogItem
		{
			if (res.IsSuccess)
			{
				return res.Content;
			}
			Debug.LogError(string.Join(", ", res.Errors.Select(x => x.Message)));
			return null;
		}

		/// <summary>
		/// Backlogの課題一覧ページを開く
		/// </summary>
		public void OpenBacklog()
		{
			Application.OpenURL($"https://{Space.Key}.{APIData.Domain}/find/{Project.Key}");
		}

		/// <summary>
		/// Backlogのチケットページを開く
		/// </summary>
		public void OpenBacklogTicket(Ticket ticket)
		{
			Application.OpenURL($"https://{Space.Key}.{APIData.Domain}/view/{ticket.Key}");
		}
	}
}