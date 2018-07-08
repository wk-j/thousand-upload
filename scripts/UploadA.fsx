#r "System.Net.Http"

open System.Net.Http
open System.IO

let startupload (client: HttpClient) (url:string) bytes dup =
    let part = new MultipartFormDataContent()
    let byteContent = new ByteArrayContent(bytes)
    byteContent.Headers.ContentType <- Headers.MediaTypeHeaderValue.Parse "application/pdf"

    for _ in 0..dup do
        part.Add(byteContent, "file", "hello.pdf")

    async {
        let! rs = client.PostAsync(url, part ) |> Async.AwaitTask
        return rs.StatusCode
    }

let go url count dup =
    let bytes = File.ReadAllBytes("scripts/aspnetcoremvc.pdf");
    let client = new HttpClient()

    [1..count]
    |> Seq.map(fun _ ->
        async {
            let! rs = startupload client url bytes dup
            return rs
        }
        |> Async.RunSynchronously
    ) |> Seq.countBy (id)

#time
go "http://localhost:4000/api/uploadA/upload" 100 10
|> printfn "%A"
#time

#time
go "http://localhost:4000/api/uploadB/upload" 100 10
|> printfn "%A"
#time

#time
go "http://localhost:4000/api/uploadC/upload" 100 10
|> printfn "%A"
#time
