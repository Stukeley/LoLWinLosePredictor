using Microsoft.AspNetCore.Mvc;
using System;
using System.Text;

namespace LoLPredictorWeb.Controllers
{
	public class GenerateController : Controller
	{
		public IActionResult Generate()
		{
			return View();
		}

		[HttpPost]
		public IActionResult InfoSubmitted(string usernameData, string region)
		{
			string csv = MatchSerializer.Serializer.Connect(usernameData, region).Result;

			// Save file prompt
			return File(Encoding.UTF8.GetBytes(csv), "text/csv", usernameData + " " + DateTime.Now.ToString("HH mm") + ".csv");
		}
	}
}
