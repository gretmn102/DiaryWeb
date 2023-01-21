module Components.DateTimeInput
open Elmish
open Feliz

open Components.Utils

type Msg =
    | Set of string

type State =
    {
        Text: string
        ResultDateTime: Result<System.DateTime, string>
    }

let init initDateTime =
    let state =
        {
            Text = CustomDateTime.toString initDateTime
            ResultDateTime = Ok initDateTime
        }
    state

let update (msg: Msg) (state: State) =
    match msg with
    | Set text ->
        let state =
            { state with
                Text = text
                ResultDateTime =
                    CustomDateTime.tryParse text
            }
        state, Cmd.none

let view (state: State) (dispatch: Msg -> unit) =
    Html.div [
        Html.input [
            prop.value state.Text
            prop.onInput (fun e ->
                match e.target :?> Browser.Types.HTMLInputElement with
                | null ->
                    failwithf "e.target :?> Browser.Types.HTMLInputElement is null"
                | inputElement ->
                    (Set inputElement.value) |> dispatch
            )
        ]
        match state.ResultDateTime with
        | Error errMsg ->
            Html.div [
                prop.style [
                    style.color "red"
                ]
                prop.text errMsg
            ]
        | _ -> ()
    ]
