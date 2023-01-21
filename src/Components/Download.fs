module Components.Download
open Browser
open Fable.Core.JsInterop
open Fable.Core.JS
open Feliz

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

let download =
    React.functionComponent
        (fun (props:
            {| description: string
               accept: string
               filename: string
               getData: unit -> string
               onDone: unit -> unit |}) ->

            Html.button [
                prop.text props.description
                prop.onClick (fun _ ->
                    saveToDisc props.accept props.filename (props.getData ())
                    props.onDone ()
                )
            ]
        )
