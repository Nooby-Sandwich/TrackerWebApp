using Microsoft.AspNetCore.Mvc;

public class TravelController : Controller
{
    // GET: /Travel
    public IActionResult Index()
    {
        return View("ComingSoon");
    }
}
