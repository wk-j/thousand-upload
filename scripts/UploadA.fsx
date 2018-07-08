#r "System.Net.Http"

open System.Net.Http
open System.IO

let startupload (client: HttpClient) (url:string) bytes dup =
    let part = new MultipartFormDataContent()
    let byteContent = new ByteArrayContent(bytes)
    byteContent.Headers.ContentType <- Headers.MediaTypeHeaderValue.Parse "application/pdf"

    for _ in 1..dup do
        part.Add(byteContent, "file", "hello.pdf")

    async {
        let! rs = client.PostAsync(url, part ) |> Async.AwaitTask
        return rs.StatusCode
    }

let sequence url count dup =
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

let par url count dup  =
    let bytes = File.ReadAllBytes("scripts/aspnetcoremvc.pdf");

    [1..count]
    |> Seq.map(fun _ ->
        async {
            let client = new HttpClient()
            let! rs = startupload client url bytes dup
            return rs
        }
    )
    |> Async.Parallel
    |> Async.RunSynchronously
    |> Seq.countBy (id)

#time
sequence "http://localhost:4000/api/uploadA/upload" 100 10
|> printfn "\nSA %A"
#time

#time
sequence "http://localhost:4000/api/uploadB/upload" 100 10
|> printfn "\nSB %A"
#time

#time
sequence "http://localhost:4000/api/uploadC/upload" 100 10
|> printfn "\nSC %A"
#time

#time
par "http://localhost:4000/api/uploadA/upload" 100 10
|> printfn "\nPA %A"
#time

#time
par "http://localhost:4000/api/uploadB/upload" 100 10
|> printfn "\nPB %A"
#time

#time
par "http://localhost:4000/api/uploadC/upload" 100 10
|> printfn "\nPC %A"
#time

(*
SA seq [(OK, 100)]
Real: 00:00:06.578, CPU: 00:00:00.942, GC gen0: 3, gen1: 0

SB seq [(OK, 100)]
Real: 00:00:06.791, CPU: 00:00:00.780, GC gen0: 3, gen1: 1

SC seq [(OK, 100)]
Real: 00:00:13.367, CPU: 00:00:00.789, GC gen0: 2, gen1: 0

PA seq [(OK, 100)]
Real: 00:01:49.687, CPU: 00:00:02.253, GC gen0: 3, gen1: 0

PB seq [(OK, 100)]
Real: 00:00:52.962, CPU: 00:00:02.289, GC gen0: 4, gen1: 0

PC seq [(OK, 100)]
Real: 00:01:13.734, CPU: 00:00:02.091, GC gen0: 4, gen1: 0
*)