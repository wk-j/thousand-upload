using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Linq;
using System;

namespace ThousandUpload.Controllers {

    [Route("api/[controller]/[action]")]
    public class UploadController : ControllerBase {

        private static readonly string tempPath =
            Path.Combine("/tmp/1000-upload", DateTime.Now.ToString("yyyy/MM/dd"));

        static UploadController() {
            if (!Directory.Exists(tempPath)) {
                Directory.CreateDirectory(tempPath);
            }
        }

        [HttpGet]
        public string Hi() => "Hello, world!";

        [HttpPost]
        public async Task<IActionResult> UploadA(List<IFormFile> files) {
            var filePath = Path.Combine(tempPath, Guid.NewGuid().ToString("N"));
            var size = files.Sum(x => x.Length);

            Console.WriteLine($" Size: {size}");

            foreach (var file in files) {
                if (file.Length > 0) {
                    Console.WriteLine($" Length: {file.Length} Name: {file.Name}");
                    using (var stream = new FileStream(filePath, FileMode.Create)) {
                        await file.CopyToAsync(stream);
                    }
                }
            }

            return Ok(new { files.Count, Size = size });
        }
    }
}