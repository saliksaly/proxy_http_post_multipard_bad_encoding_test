using Microsoft.AspNetCore.Mvc;

namespace proxy_http_post_multipard_bad_encoding_test.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
