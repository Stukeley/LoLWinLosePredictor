using MatchSerializer;

namespace StartupDebug
{
	internal class Program
	{
		private static void Main()
		{
			string username = "Faulty Carry";
			string region = "Euw";

			Serializer.Connect(username, region);
		}
	}
}
