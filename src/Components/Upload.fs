module Components.Upload
open Feliz
open Fable.Builders.Fela
open Browser

open Components.Utils

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

[<RequireQualifiedAccess>]
module Elmish =
    type Msg =
        | Import of string

    type State<'T> =
        {
            ImportResult: Result<'T, string> Deferred
        }

    let init =
        let state =
            {
                ImportResult = NotStartedYet
            }
        state

    type UpdateResult<'T> =
        | UpdateRes of State<'T>
        | ImportResult of State<'T> * 'T

    let update tryDeserialize (msg: Msg) (state: State<'T>) =
        match msg with
        | Import rawJson ->
            let res =
                tryDeserialize rawJson

            let state =
                { state with
                    ImportResult =
                        Resolved res
                }
            match res with
            | Ok resultValue ->
                ImportResult (state, resultValue)
            | Error _ ->
                UpdateRes state

    let view (state: State<'T>) (dispatch: Msg -> unit) =
        Html.div [
            upload {|
                description = "Load"
                accept = "application/json"
                cb = Import >> dispatch
            |}

            match state.ImportResult with
            | NotStartedYet -> ()
            | InProgress ->
                Html.div [
                    prop.text "Loading"
                ]
            | Resolved r ->
                match r with
                | Error errMsg ->
                    Html.div [
                        prop.style [
                            style.color "red"
                        ]
                        prop.text errMsg
                    ]
                | _ ->
                    ()
        ]
