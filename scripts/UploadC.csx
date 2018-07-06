#! "netcoreapp2.1"

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

var url = "http://localhost:4000/api/uploadC/upload";
var bytes = File.ReadAllBytes("scripts/aspnetcoremvc.pdf");

async Task<HttpStatusCode> StartUpload(int size) {
    using (var client = new HttpClient()) {
        var requestContent = new MultipartFormDataContent("---XX---");

        var fileContent = new ByteArrayContent(bytes);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/pdf");

        foreach (var item in Enumerable.Range(0, size)) {
            requestContent.Add(fileContent, "files", "file.pdf");
        }

        var rs = await client.PostAsync(url, requestContent);
        return rs.StatusCode;
    }
}

async Task<HttpStatusCode[]> Upload(int count, int size) {
    var r = Enumerable.Range(0, count);
    var query = r.Select(async (i) => {
        var ok = await StartUpload(size);
        return ok;
    });
    return await Task.WhenAll(query);
}

var results = await Upload(200, 5);

var group = results.GroupBy(x => x);
foreach (var item in group) {
    Console.WriteLine(" > {0} {1}", item.Key, item.Count());
}
