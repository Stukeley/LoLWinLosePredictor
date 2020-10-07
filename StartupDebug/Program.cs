using MachineLearningModel;

namespace StartupDebug
{
	/// <summary>
	/// A console application used for running the application in debug mode.
	/// </summary>
	internal class Program
	{
		private static void Main()
		{
			////string username = "Faulty Carry";
			////string region = "Euw";
			////string csv = Serializer.Connect(username, region).Result;
			////Console.WriteLine(csv);

			// ML.NET
			ModelBuilder.CreateModel();

			var example = new ModelInput()
			{
				SpecifiedPlayer = "Ekko",
				AllyTeam = new string[] { "TwistedFate", "Soraka", "Fiddlesticks", "Thresh" },
				EnemyTeam = new string[] { "Malzahar", "Jinx", "Malphite", "Mordekaiser", "Fiora" },
			};

			var output = ConsumeModel.Predict(example); // Outcome: true, Score: 0.78
		}
	}
}