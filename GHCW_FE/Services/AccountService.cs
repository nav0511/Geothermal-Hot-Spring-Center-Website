﻿using Microsoft.AspNetCore.Mvc;

namespace GHCW_FE.Services
{
    public class AccountService : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}