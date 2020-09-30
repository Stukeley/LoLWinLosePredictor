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
	/// </summary>
	public static class Serializer
	{
		private static RiotApi _api;
		private static string _apiKey;
		private static string _apiKeyPath = "ApiKey.txt";
		private static string _outputFilePath = "outputData.csv";
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

		public static void Connect(string username, string region)
		{
			try
			{
				var parsedRegion = (Region)Enum.Parse(typeof(Region), region);
				Connect(username, parsedRegion).Wait();
			}
			catch (Exception e)
			{
				Debug.WriteLine(e.Message);
			}
		}

		public static async Task Connect(string username, Region region)
		{
			Debug.WriteLine("Connecting!");

			Username = username;
			Region = region;

			_api = RiotApi.GetDevelopmentInstance(_apiKey);

			try
			{
				var summoner = await _api.Summoner.GetSummonerByNameAsync(Region, Username);
				var accId = summoner.AccountId;

				var matchHistory = await _api.Match.GetMatchListAsync(Region, accId);

				Debug.WriteLine("Got match history");
				NumberOfRequests += 2;

				await SerializeToCsv(matchHistory);
			}
			catch (Exception e)
			{
				Debug.WriteLine(e.Message);
			}
		}

		private static async Task SerializeToCsv(MatchList matchHistory)
		{
			var csv = new StringBuilder();

			int participantId;

			csv.AppendLine("AllyTeam(Champion1, Champion2, Champion3, Champion4, Champion5), EnemyTeam(Champion6, Champion7, Champion8, Champion9, Champion10), Outcome");

			foreach (var match in matchHistory.Matches)
			{
				// Get match details by id
				var id = match.GameId;
				var detail = await _api.Match.GetMatchAsync(Region, id);
				NumberOfRequests += 1;

				participantId = detail.ParticipantIdentities.Where(x => x.Player.SummonerName == Username).FirstOrDefault().ParticipantId;

				string newLine = "";

				// Get the team the specified player was on
				var teamId = detail.Participants.FirstOrDefault(x => x.ParticipantId == participantId).TeamId;

				// Get champions and save them to CSV
				// TODO: divide matches into normal/ranked/etc vs bot/tutorial/etc

				var allyTeamChampions = detail.Participants.Where(x => x.TeamId == teamId).Select(x => x.ChampionId);

				newLine += "(";

				foreach (var champ in allyTeamChampions)
				{
					newLine += $"{ChampionIdNamePairs.ChampionPairs[champ]}, ";
				}

				newLine += "), ";

				var enemyTeamChampions = detail.Participants.Where(x => x.TeamId != teamId).Select(x => x.ChampionId);

				newLine += "(";

				foreach (var champ in enemyTeamChampions)
				{
					newLine += $"{ChampionIdNamePairs.ChampionPairs[champ]}, ";
				}

				newLine += "), ";

				// Check if the match was won by the specified player or not
				var outcome = detail.Participants.Where(x => x.TeamId == teamId).FirstOrDefault()?.Stats.Winner;

				// 1 means win, 0 means loss (relative to the specified player)
				newLine += $"{(outcome == true ? 1 : 0)}";

				csv.AppendLine(newLine);
			}

			// TODO: download prompt
			File.WriteAllText(_outputFilePath, csv.ToString());
		}
	}
}
