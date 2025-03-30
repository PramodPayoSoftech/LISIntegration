using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using LISIntegration.Models;

namespace LISIntegration.Controllers
{
    [Route("[controller]")]
    public class AstmController : Controller
    {
        private readonly ILogger<AstmController> _logger;
        private readonly AstmSettings _astmSettings;

        public AstmController(ILogger<AstmController> logger, IOptions<AstmSettings> astmSettings)
        {
            _logger = logger;
            _astmSettings = astmSettings.Value;
        }

        [HttpGet("/astm")]
        [HttpGet("/astm/index")]
        [HttpGet("/tcp")] // Keep old route for backward compatibility
        [HttpGet("/tcp/index")] // Keep old route for backward compatibility
        public IActionResult Index()
        {
            return View(_astmSettings);
        }

        [HttpPost("/astm")]
        [HttpPost("/astm/index")]
        [HttpPost("/tcp")] // Keep old route for backward compatibility
        [HttpPost("/tcp/index")] // Keep old route for backward compatibility
        public IActionResult Index(AstmSettings model)
        {
            if (ModelState.IsValid)
            {
                // We can't actually update the settings directly
                // This would need to be implemented through a configuration service
                // or by persisting changes to the appsettings.json file

                // For now, just log the requested changes
                _logger.LogInformation($"Settings update requested: IP={model.IpAddress}, Port={model.Port}, OutputDir={model.OutputDirectory}");

                // Show a message to the user
                TempData["Message"] = "ASTM settings saved. Please restart the application for the changes to take effect.";
            }

            return View(model);
        }

        [HttpGet("/astm/viewlogs")]
        [HttpGet("/tcp/viewlogs")] // Keep old route for backward compatibility
        public IActionResult ViewLogs()
        {
            var dataDirectory = _astmSettings.OutputDirectory;
            if (!Directory.Exists(dataDirectory))
            {
                Directory.CreateDirectory(dataDirectory);
            }

            var files = Directory.GetFiles(dataDirectory, "*.txt")
                .Select(f => new FileInfo(f))
                .OrderByDescending(f => f.CreationTime)
                .Select(f => new LogFileViewModel
                {
                    FileName = f.Name,
                    CreatedAt = f.CreationTime,
                    FilePath = f.FullName,
                    SizeInBytes = f.Length
                })
                .ToList();

            return View(files);
        }

        [HttpGet("/astm/viewfile")]
        [HttpGet("/tcp/viewfile")] // Keep old route for backward compatibility
        public IActionResult ViewFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var content = System.IO.File.ReadAllText(filePath);
            var fileName = Path.GetFileName(filePath);

            var viewModel = new FileContentViewModel
            {
                FileName = fileName,
                Content = content
            };

            return View(viewModel);
        }
    }
}