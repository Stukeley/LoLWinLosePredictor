using Microsoft.AspNetCore.Mvc;

namespace LoLPredictorWeb.Controllers
{
	public class PredictController : Controller
	{
		public IActionResult Predict()
		{
			return View();
		}
	}
}
