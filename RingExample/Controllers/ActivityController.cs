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

        [Route("recording/{id}")]
        public async Task<IActionResult> Recording(ulong id)
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
                var ding = new Ring.Models.Ding() { Id = id, RecordingIsReady = true };
                var recordingUri = await ring.GetRecordingUriAsync(ding);

                return View(recordingUri);
            }
            catch
            {
                return NotFound();
            }
        }
    }
}