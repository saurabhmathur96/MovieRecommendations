using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MovieRecommendations.Models;
using MovieRecommendations.Data;

namespace MovieRecommendations.Controllers
{
    public class AccountController : Controller 
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ApplicationDbContext _dbContext;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _dbContext = dbContext;
        }

        [HttpGet]
        public IActionResult Register(string error) 
        {
            ViewData["Error"] = error;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model) 
        {
            if(ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.UserName };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded) 
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction(nameof(HomeController.Index), "Home");
                }
                return RedirectToAction(nameof(AccountController.Register), "Account", new {error= result.Errors.ElementAt(0).Description});
            }
            
            return RedirectToAction(nameof(AccountController.Register), "Account");
        }

        [HttpGet]
        public IActionResult Login(string error) 
        {
            ViewData["Error"] = error;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model) 
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(AccountController.Index), "Account");
                }
            }
            return RedirectToAction(nameof(AccountController.Login), "Account", new {error="UserName and/or Password do not match"});
            
        }

        [HttpGet]
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> LogOff() 
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }


    }
}