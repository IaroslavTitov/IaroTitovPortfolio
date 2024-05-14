﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Portfolio.Models;

namespace Portfolio.Controllers
{
    public class HomeController : Controller
    {
        private readonly StorageConnector storageConnector;
        private const string PORTFOLIO_CONNECTION_STRING = "PortfolioStorage";

        public HomeController(IConfiguration config)
        {
            string connectionString = config[PORTFOLIO_CONNECTION_STRING] ?? Environment.GetEnvironmentVariable(PORTFOLIO_CONNECTION_STRING);
            storageConnector = new StorageConnector(connectionString);
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewBag.LeftLink = new KeyValuePair<string, string>("My Story", "/MyStory");
            ViewBag.RightLink = new KeyValuePair<string, string>("Projects", "/Projects");

            IEnumerable<SkillGroupEntity> skills = await storageConnector.LoadSkillGroups();
            string skillsJson = JsonConvert.SerializeObject(skills, new JsonSerializerSettings
            {
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
            });

            return View(model: skillsJson);
        }

        [HttpGet]
        [Route("MyStory")]
        public async Task<IActionResult> PersonalStoryAsync()
        {
            ViewBag.LeftLink = new KeyValuePair<string, string>("Home Page", "/");
            ViewBag.RightLink = new KeyValuePair<string, string>("Projects", "/Projects");

            List<EventEntity> events = (await storageConnector.LoadEvents()).OrderByDescending(x => x.Date).ToList();
            int i = 0;
            events.ForEach(x => x.Right = i++ % 2 == 0);
            return View(events);
        }

        [HttpGet]
        [Route("Projects")]
        public async Task<IActionResult> ProjectsAsync(string skills)
        {
            ViewBag.LeftLink = new KeyValuePair<string, string>("My Story", "/MyStory");
            ViewBag.RightLink = new KeyValuePair<string, string>("Home Page", "/");

            IEnumerable<ProjectEntity> projects = await storageConnector.LoadProjects();
            ViewBag.skills = (await storageConnector.LoadSkills()).Where(skill => projects.Any(project => project.skills.Any(x => x.Id == skill.Id)));

            ViewBag.filterSkills = new List<string>();

            if (!string.IsNullOrEmpty(skills))
            {
                List<string> filterSkills = skills.Split(',').ToList();
                ViewBag.filterSkills = filterSkills;
                projects = projects.Where(x => filterSkills.All(x.skills.Select(skill => skill.SkillName).Contains));
            }


            return View(projects);
        }
    }
}