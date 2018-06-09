using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ring;

namespace RingExample.Controllers
{
    [Route("devices")]
    [Authorize]
    public class DeviceController : Controller
    {
        public async Task<IActionResult> Index()
        {
            RingClient ring;

            try
            {
                var authToken = User.Claims.FirstOrDefault(e => e.Type == "auth_token")?.Value;
                ring = await RingClient.CreateAsync(authToken);
            }
            catch
            {
                return Challenge();
            }

            try
            {
                return View(await ring.GetDevicesAsync());
            }
            catch
            {
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}