// unused

using Microsoft.AspNetCore.Mvc;

namespace CapstoneWebApp.Pages.Clients
{
    public class Validation : Controller
    {
        [HttpGet]
        public IActionResult Clients()
        {
            Console.WriteLine("Clients no param");
            return View();
        }

        [HttpPost]
        public IActionResult Clients(ClientsModel cmodel) {
            Console.WriteLine("Clients parameter");
            return View();
        }
    }
}
