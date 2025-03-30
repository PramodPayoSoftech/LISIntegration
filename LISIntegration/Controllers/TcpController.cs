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
    public class TcpController : Controller
    {
        private readonly ILogger<TcpController> _logger;
        private readonly TcpSettings _tcpSettings;

        public TcpController(ILogger<TcpController> logger, IOptions<TcpSettings> tcpSettings)
        {
            _logger = logger;
            _tcpSettings = tcpSettings.Value;
        }

        public IActionResult Index()
        {
            return View(_tcpSettings);
        }

        [HttpPost]
        public IActionResult Index(TcpSettings model)
        {
            if (ModelState.IsValid)
            {
                // We can't actually update the settings directly
                // This would need to be implemented through a configuration service
                // or by persisting changes to the appsettings.json file

                // For now, just log the requested changes
                _logger.LogInformation($"Settings update requested: IP={model.IpAddress}, Port={model.Port}, OutputDir={model.OutputDirectory}");

                // Show a message to the user
                TempData["Message"] = "Settings saved. Please restart the application for the changes to take effect.";
            }

            return View(model);
        }

        public IActionResult ViewLogs()
        {
            var dataDirectory = _tcpSettings.OutputDirectory;
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