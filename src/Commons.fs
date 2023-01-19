module Commons
type 'a Deferred =
    | NotStartedYet
    | InProgress
    | Resolved of 'a

type Event =
    {
        DateTime: System.DateTime
        Description: string
    }

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
[<RequireQualifiedAccess>]
module Event =
    let create dateTime description =
        {
            DateTime = dateTime
            Description = description
        }

module Result =
    let defaultWith fn (result: Result<_, _>) =
        match result with
        | Ok x -> x
        | Error x -> fn x

    let isError (result: Result<_, _>) =
        match result with
        | Error _ -> true
        | Ok _ -> false

module Routes =
    [<Literal>]
    let EventsListPageRoute = "EventsListPageRoute"

    [<Literal>]
    let EventsAdderPageRoute = "EventsAdderPageRoute"

open Browser
open Browser.Types
open Fable.Core.JsInterop
open Fable.Core.JS

/// `type = "application/json"`
let saveToDisc type' (filename: string) (data: 'Data) =
    let file = Blob.Create(
        [|data|],
        jsOptions<Types.BlobPropertyBag>(fun x ->
            x.``type`` <- type'
        )
    )

    // todo:
    //   if ((window.navigator as any).msSaveOrOpenBlob) // IE10+
    //     (window.navigator as any).msSaveOrOpenBlob(file, filename)
    //   else { // Others
    let a = document.createElement "a" :?> Types.HTMLAnchorElement
    let url = URL.createObjectURL(file)
    a.href <- url
    a?download <- filename
    document.body.appendChild(a) |> ignore
    a.click()
    setTimeout
        (fun () ->
            document.body.removeChild a |> ignore
            URL.revokeObjectURL(url)
        )
        0
    |> ignore
