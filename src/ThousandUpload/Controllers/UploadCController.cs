using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ThousandUpload.Controllers {

    [Route("api/[controller]/[action]")]
    public class UploadCController : ControllerBase {
        private static readonly string tempPath = TempFile.GetTempPath();

        [HttpPost]
        public async Task<dynamic> Upload() {
            var files = this.Request.Form.Files;
            foreach (var file in files) {
                var dest = Path.Combine(tempPath, Guid.NewGuid().ToString("N") + "-class.pdf");
                using (var stream = new FileStream(dest, FileMode.Create, FileAccess.Write)) {
                    await file.CopyToAsync(stream);
                }
            }
            return new { };
        }
    }
}