using Microsoft.AspNetCore.Mvc;
using scout_api.Services;

namespace scout_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BadgesController : Controller
    {
        private readonly BadgeService badgeService;
        //public IActionResult Index()
        //{
        //    return View();
        //}
    }
}
