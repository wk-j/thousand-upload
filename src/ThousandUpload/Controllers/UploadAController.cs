using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Linq;
using System;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System.Text;

namespace ThousandUpload.Controllers {

    public static class TempFile {
        private static readonly string tempPath =
            Path.Combine("/tmp/1000-upload", DateTime.Now.ToString("yyyy/MM/dd"));

        static TempFile() {
            if (!Directory.Exists(tempPath)) {
                Directory.CreateDirectory(tempPath);
            }
        }
        public static string GetTempPath() => tempPath;
    }

    [Route("api/[controller]/[action]")]
    public class UploadAController : ControllerBase {

        private static readonly string tempPath = TempFile.GetTempPath();

        [HttpGet]
        public string Hi() => "Hello, world!";

        [HttpPost]
        public async Task<IActionResult> Upload(List<IFormFile> files) {
            var filePath = Path.Combine(tempPath, Guid.NewGuid().ToString("N")) + ".pdf";
            var size = files.Sum(x => x.Length);

            // Console.WriteLine($" {DateTime.Now} :: Size: {size}, Files: {files.Count}");
            foreach (var file in files) {
                if (file.Length > 0) {
                    using (var stream = new FileStream(filePath, FileMode.Create)) {
                        await file.CopyToAsync(stream);
                    }
                }
            }
            return Ok(new { files.Count, Size = size });
        }

        private static Encoding GetEncoding(MultipartSection section) {
            MediaTypeHeaderValue mediaType;
            var hasMediaTypeHeader = MediaTypeHeaderValue.TryParse(section.ContentType, out mediaType);
            // UTF-7 is insecure and should not be honored. UTF-8 will succeed in
            // most cases.
            if (!hasMediaTypeHeader || Encoding.UTF7.Equals(mediaType.Encoding)) {
                return Encoding.UTF8;
            }
            return mediaType.Encoding;
        }

        [HttpPost]
        public async Task<IActionResult> UploadB() {
            if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType)) {
                return BadRequest("WTF");
            }
            var formAccumulator = new KeyValueAccumulator();
            var boundary = MultipartRequestHelper.GetBoundary(MediaTypeHeaderValue.Parse(Request.ContentType), 10000);
            var reader = new MultipartReader(boundary, HttpContext.Request.Body);
            var section = await reader.ReadNextSectionAsync();

            while (section != null) {
                var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition);
                if (hasContentDispositionHeader) {
                    if (MultipartRequestHelper.HasFileContentDisposition(contentDisposition)) {
                        var targetFilePath = (Path.Combine(tempPath, Guid.NewGuid().ToString("N") + ".pdf"));
                        using (var targetStream = System.IO.File.Create(targetFilePath)) {
                            await section.Body.CopyToAsync(targetStream);
                        }
                    } else if (false && MultipartRequestHelper.HasFormDataContentDisposition(contentDisposition)) {
                        var key = HeaderUtilities.RemoveQuotes(contentDisposition.Name);
                        var encoding = GetEncoding(section);
                        using (var streamReader = new StreamReader(
                            section.Body,
                            encoding,
                            detectEncodingFromByteOrderMarks: true,
                            bufferSize: 1024,
                            leaveOpen: true
                        )) {
                            var value = await streamReader.ReadToEndAsync();
                            if (String.Equals(value, "undefined", StringComparison.OrdinalIgnoreCase)) {
                                value = string.Empty;
                            }
                            formAccumulator.Append(key.ToString(), value);

                            if (formAccumulator.ValueCount > 100) {
                                throw new InvalidDataException("Form WTF");
                            }
                        }
                    }
                }
                section = await reader.ReadNextSectionAsync();
            }
            return Ok(new { Success = true });
        }
    }
}