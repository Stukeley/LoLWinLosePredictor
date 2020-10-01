using RiotSharp;
using RiotSharp.Endpoints.MatchEndpoint;
using RiotSharp.Misc;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MatchSerializer
{
	/// <summary>
	/// Fetches match history data for the specified SummonerName and Region, and creates a .CSV file based on it.
	/// Only takes into account normal and ranked 5v5 games.
	/// </summary>
	public static class Serializer
	{
		private static RiotApi _api;
		private static string _apiKey;
		private static string _apiKeyPath = @"E:\Programowanie\Stukeley\LoLWinLosePredictor\MatchSerializer\ApiKey.txt";
		private static string _outputFilePath = @"E:\Programowanie\Stukeley\LoLWinLosePredictor\MatchSerializer\outputData.csv";
		private static int _numberOfRequests = 0;
		public static int NumberOfRequests
		{
			get
			{
				return _numberOfRequests;
			}
			set
			{
				_numberOfRequests = value;
				if (_numberOfRequests == 100)
				{
					Debug.WriteLine("Reached a 2min rate limit.");
					Thread.Sleep(120000);
				}
				else if (_numberOfRequests % 20 == 0)
				{
					Debug.WriteLine("Reached a 1s rate limit.");
					Thread.Sleep(1000);
				}
			}
		}
		public static string Username { get; private set; }
		public static Region Region { get; private set; }

		static Serializer()
		{
			using (var reader = new StreamReader(_apiKeyPath))
			{
				_apiKey = reader.ReadLine();
			}
		}

		public static async Task<string> Connect(string username, string region)
		{
			string result = "";
			Debug.WriteLine("Connecting!");

			try
			{
				var parsedRegion = (Region)Enum.Parse(typeof(Region), region);

				Username = username;
				Region = parsedRegion;

				_api = RiotApi.GetDevelopmentInstance(_apiKey);

				var summoner = await _api.Summoner.GetSummonerByNameAsync(Region, Username);
				var accId = summoner.AccountId;

				var matchHistory = await _api.Match.GetMatchListAsync(Region, accId);

				Debug.WriteLine("Got match history");
				NumberOfRequests += 2;

				result = await SerializeToCsv(matchHistory);
			}
			catch (Exception e)
			{
				Debug.WriteLine(e.Message);
			}

			return result;
		}

		private static async Task<string> SerializeToCsv(MatchList matchHistory)
		{
			var countedQueueIds = new int[] { 2, 4, 6, 14, 400, 420, 430, 440 };
			var csv = new StringBuilder();

			int participantId;

			csv.AppendLine("SpecifiedPlayer,Ally1,Ally2,Ally3,Ally4,Enemy1,Enemy2,Enemy3,Enemy4,Enemy5,Outcome");

			foreach (var match in matchHistory.Matches)
			{
				// Only count the game if it's a non-bot non-tutorial game (QueueID: 2,4,6,14,400,420,430,440)
				if (!countedQueueIds.Contains(match.Queue))
				{
					continue;
				}

				// Get match details by id
				var id = match.GameId;
				var detail = await _api.Match.GetMatchAsync(Region, id);
				NumberOfRequests += 1;

				participantId = detail.ParticipantIdentities.Where(x => x.Player.SummonerName == Username).FirstOrDefault().ParticipantId;

				string newLine = "";

				// Get the team the specified player was on
				var teamId = detail.Participants.FirstOrDefault(x => x.ParticipantId == participantId).TeamId;

				// Get champions and save them to CSV

				var playersChampion = detail.Participants.Where(x => x.ParticipantId == participantId).FirstOrDefault().ChampionId;

				newLine += $"{ChampionIdNamePairs.ChampionPairs[playersChampion]},";

				var allyTeamChampions = detail.Participants.Where(x => x.TeamId == teamId && x.ParticipantId != participantId).Select(x => x.ChampionId);

				foreach (var champ in allyTeamChampions)
				{
					newLine += $"{ChampionIdNamePairs.ChampionPairs[champ]},";
				}

				var enemyTeamChampions = detail.Participants.Where(x => x.TeamId != teamId).Select(x => x.ChampionId);

				foreach (var champ in enemyTeamChampions)
				{
					newLine += $"{ChampionIdNamePairs.ChampionPairs[champ]},";
				}

				// Check if the match was won by the specified player or not
				var outcome = detail.Participants.Where(x => x.TeamId == teamId).FirstOrDefault()?.Stats.Winner;

				// 1 means win, 0 means loss (relative to the specified player)
				newLine += $"{(outcome == true ? 1 : 0)}";

				csv.AppendLine(newLine);
			}

			File.WriteAllText(_outputFilePath, csv.ToString());
			return csv.ToString();
		}
	}
}
