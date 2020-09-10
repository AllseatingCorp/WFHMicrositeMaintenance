using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using WFHMicrositeMaintenance.Models;

namespace WFHMicrositeMaintenance.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(Login model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (var adContext = new PrincipalContext(ContextType.Domain, "ALLSEATING.COM"))
            {
                if (adContext.ValidateCredentials(model.Username, model.Password))
                {
                    UserPrincipal userPrincipal = UserPrincipal.FindByIdentity(adContext, model.Username);
                    //var role = userPrincipal.GetGroups();
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, userPrincipal.Name),
                        new Claim(ClaimTypes.Email, userPrincipal.EmailAddress)
                    };
                    var roles = userPrincipal.GetGroups();
                    foreach (var role in roles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role.Name));
                    }
                    HttpContext.SignInAsync(new ClaimsPrincipal(new ClaimsIdentity(claims, "User Identity")));
                    return RedirectToAction("Index", "Home");
                }
            }

            ModelState.AddModelError("", "Login Failed");
            return View(model);
        }
    }
}
