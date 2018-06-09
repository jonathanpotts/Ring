using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Ring;
using RingExample.Models;

namespace RingExample.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet("login")]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            return View();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var ring = await RingClient.CreateAsync(model.EmailAddress, model.Password);

                    var claims = new List<Claim>
                    {
                        new Claim("auth_token", ring.AuthToken)
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties { IsPersistent = model.StayLoggedIn };

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                    if (!string.IsNullOrEmpty(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    return RedirectToAction("Index", "Activity");
                }
                catch (SecurityException)
                {
                    ModelState.AddModelError("", "The provided email address and/or password are incorrect.");
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "The Ring service failed to login.");
                }
            }

            ViewData["ReturnUrl"] = returnUrl;

            return View(model);
        }

        [Route("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Home");
        }
    }
}