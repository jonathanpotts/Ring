using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ring;

namespace RingExample.Controllers
{
    [Route("activity")]
    [Authorize]
    public class ActivityController : Controller
    {
        public async Task<IActionResult> Index()
        {
            try
            {
                var authToken = User.Claims.FirstOrDefault(e => e.Type == "auth_token")?.Value;
                var ring = await RingClient.CreateAsync(authToken);

                return View(await ring.GetDingsAsync());
            }
            catch
            {
                return Challenge();
            }
        }
    }
}