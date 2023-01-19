module Upload
open Feliz
open Fable.Builders.Fela
open Browser

let upload = React.functionComponent(fun (props: {| accept: string; description: string; cb: string -> unit |}) ->
    let css = Fela.useFela ()
    let inputFileStyle = css [
        style.display.none
    ]

    let uploadId = "uploadId"

    Html.div [
        prop.children [
            Html.button [
                prop.htmlFor uploadId
                prop.text props.description
                prop.onClick (fun e ->
                    match document.getElementById uploadId with
                    | null ->
                        failwithf "Not found %s" uploadId
                    | x ->
                        x.click()
                )
            ]

            Html.input [
                prop.className inputFileStyle
                prop.id uploadId
                prop.type' "file"
                prop.accept props.accept
                prop.onChange (fun (files: Types.File list) ->
                    let file = files[0]
                    let reader = FileReader.Create()
                    reader.onload <- (fun e ->
                        match reader.result with
                        | null -> ()
                        | content ->
                            props.cb (content :?> string)
                    )
                    reader.readAsText file
                )
            ]
        ]
    ]
)

