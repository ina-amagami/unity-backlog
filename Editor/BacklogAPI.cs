/*
unity-backlog

Copyright (c) 2019 ina-amagami (ina@amagamina.jp)

This software is released under the MIT License.
https://opensource.org/licenses/mit-license.php
*/

using System.Linq;
using UnityEngine;
using NBacklog;
using NBacklog.DataTypes;
using NBacklog.OAuth2;
using System.Threading.Tasks;

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
		public async void LoadProjectInfo(System.Action onSuccess)
		{
			APIData = BacklogAPIData.Load();

			// 認証
			var client = new BacklogClient(APIData.SpaceKey, APIData.Domain);
			await client.AuthorizeAsync(new OAuth2App()
			{
				ClientId = APIData.ClientId,
				ClientSecret = APIData.ClientSecretId,
				RedirectUri = APIData.RedirectURI,
				CredentialsCachePath = APIData.CacheFileName,
			});

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
			return GetTicketOrRetryByResult(res, ticket);
		}

		/// <summary>
		/// チケットの更新
		/// </summary>
		public Ticket UpdateTicket(Ticket ticket)
		{
			var res = Project.UpdateTicketAsync(ticket).Result;
			return GetTicketOrRetryByResult(res, ticket);
		}

		/// <summary>
		/// トランザクション系のエラーだったら１回だけリトライ
		/// 成功してたらチケットを取得
		/// </summary>
		public Ticket GetTicketOrRetryByResult(BacklogResponse<Ticket> res, Ticket ticket)
		{
			if (!res.IsSuccess && res.Errors.Any(x => x.Message.StartsWith("Deadlock")))
			{
				res = Project.UpdateTicketAsync(ticket).Result;
			}
			if (!res.IsSuccess)
			{
				Debug.LogError(string.Join(", ", res.Errors.Select(x => x.Message)));
				return null;
			}
			return res.Content;
		}

		/// <summary>
		/// Backlogの課題一覧ページを開く
		/// </summary>
		public void OpenBacklog()
		{
			const string OpenURLFormat = "https://{0}.{1}/find/{2}";
			Application.OpenURL(string.Format(OpenURLFormat, Space.Key, APIData.Domain, Project.Key));
		}

		/// <summary>
		/// Backlogのチケットページを開く
		/// </summary>
		public void OpenBacklogTicket(Ticket ticket)
		{
			const string OpenURLFormat = "https://{0}.{1}/view/{2}";
			Application.OpenURL(string.Format(OpenURLFormat, Space.Key, APIData.Domain, ticket.Key));
		}
	}
}