#! "netcoreapp2.1"

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

var url = "http://localhost:4000/api/upload/uploadA";
var bytes = File.ReadAllBytes("scripts/aspnetcoremvc.pdf");

async Task<HttpStatusCode> StartUpload() {
    using (var client = new HttpClient()) {
        var requestContent = new MultipartFormDataContent("---XX---");

        var fileContent = new ByteArrayContent(bytes);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/pdf");

        requestContent.Add(fileContent, "files", "file.pdf");
        requestContent.Add(fileContent, "files", "file.pdf");

        var rs = await client.PostAsync(url, requestContent);
        return rs.StatusCode;
    }
}

var rs = await StartUpload();
Console.WriteLine(rs);