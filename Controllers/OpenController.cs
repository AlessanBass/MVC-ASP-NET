using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MvcMovie.Controllers;
[Authorize]
public class OpenController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

}
