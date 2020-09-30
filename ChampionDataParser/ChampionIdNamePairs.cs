using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace MatchSerializer
{
	/// <summary>
	/// Class responsible for making a Dictionary of Champion Ids and Names and serializing it to a json file.
	/// This is only called to create and update the file containing champion info.
	/// We're origianlly using the champion.json file with a lot more info, but we only really need the Id and Name.
	/// </summary>
	public static class ChampionIdNamePairs
	{
		private static string _championFileName = "champion2.json";
		private static string _outputFileName = "championSerialized.json";
		public static Dictionary<int, string> ChampionPairs;

		static ChampionIdNamePairs()
		{
			try
			{
				ParseChampionData();
			}
			catch (Exception e)
			{
				Debug.WriteLine(e.Message);
			}
		}

		private static void ParseChampionData()
		{
			ChampionPairs = new Dictionary<int, string>();

			// Load champion names and Ids from the file
			JObject rss = JObject.Parse(File.ReadAllText(_championFileName));

			foreach (var item in rss.Children().Children())
			{
				var key = (int)item["key"];
				var id = (string)item["id"];

				ChampionPairs.Add(key, id);
			}

			// Serialize Ids and Names to a new json file
			//? Might not be used at all - remove?
			string serialized = JsonConvert.SerializeObject(ChampionPairs, Formatting.Indented);

			using (var writer = new StreamWriter(_outputFileName, append: false))
			{
				writer.Write(serialized);
			}
		}
	}
}