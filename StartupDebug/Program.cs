using MatchSerializer;
using System;

namespace StartupDebug
{
	/// <summary>
	/// A console application used for running the application in debug mode.
	/// </summary>
	internal class Program
	{
		private static void Main()
		{
			string username = "Faulty Carry";
			string region = "Euw";

			string csv = Serializer.Connect(username, region).Result;

			Console.WriteLine(csv);
		}
	}
}