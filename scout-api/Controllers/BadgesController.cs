using Microsoft.AspNetCore.Mvc;
using scout_api.Repositories;

namespace scout_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BadgesController : Controller
    {
        private readonly BadgeRepository badgeService;
        //public IActionResult Index()
        //{
        //    return View();
        //}
    }
}
