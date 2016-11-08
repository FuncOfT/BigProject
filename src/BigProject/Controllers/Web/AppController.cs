using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BigProject.ViewModels;
using BigProject.Services;
using Microsoft.Extensions.Configuration;
using BigProject.Models;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

namespace BigProject.Controllers.Web
{
    public class AppController : Controller
    {
        private IMailService _mailService;
        private IConfigurationRoot _config;
        private readonly IWorldRepository _repository;
        private readonly ILogger<AppController> _logger;

        public AppController(IMailService mailService, 
            IConfigurationRoot config,
            IWorldRepository context,
            ILogger<AppController> logger)
        {
            _mailService = mailService;
            _config = config;
            _repository = context;
            _logger = logger;
        }


        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult Trips()
        {
            try
            {
                var data = _repository.GetAllTrips();

                return View(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to get trips in Index page: {ex.Message}");

                return Redirect("/error");
            }
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Contact(ContactViewModel model)
        {
            if (model.Email.Contains("aol.com"))
            {
                ModelState.AddModelError("Email", "We don't support AOL addresses");
            }

            if (ModelState.IsValid)
            {
                _mailService.SendMail(_config["MailSettings:ToAddress"], model.Email, "BigProject", model.Message);

                ModelState.Clear();
              
                ViewBag.UserMessage = "Message Sent";
            }

            return View();
        }
    }
}
