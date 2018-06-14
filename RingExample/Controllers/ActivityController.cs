using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ring;
using RingExample.Models;

namespace RingExample.Controllers
{
    [Route("activity")]
    [Authorize]
    public class ActivityController : Controller
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
                return View(await ring.GetDingsAsync());
            }
            catch
            {
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        [Route("recording/{id}/{type}/{deviceName}/{createdAt}")]
        public async Task<IActionResult> Recording(ulong id, string type, string deviceName, int createdAt)
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
                RecordingViewModel model = new RecordingViewModel();

                var ding = new Ring.Models.Ding() { Id = id, RecordingIsReady = true };
                model.RecordingUri = await ring.GetRecordingUriAsync(ding);
                model.Type = type;
                model.DeviceName = deviceName;
                model.CreatedAt = new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(createdAt);

                return View(model);
            }
            catch
            {
                return NotFound();
            }
        }
    }
}