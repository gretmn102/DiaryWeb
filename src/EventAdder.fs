module EventAdder
open Elmish
open Feliz

open Components
open Commons

type Msg =
    | DateTimeInputMsg of DateTimeInput.Msg
    | SetDescription of string
    | Submit
    | Cancel

type State =
    {
        DateTimeInputState: DateTimeInput.State
        Description: string
    }

let init () =
    let state =
        {
            DateTimeInputState = DateTimeInput.init System.DateTime.Now
            Description = ""
        }
    state, Cmd.none

type UpdateResult =
    | UpdateRes of State * Cmd<Msg>
    | SubmitRes of Event
    | CancelRes

let update (msg: Msg) (state: State) =
    match msg with
    | DateTimeInputMsg msg ->
        let state', msg = DateTimeInput.update msg state.DateTimeInputState
        let state =
            { state with
                DateTimeInputState = state'
            }
        (state, msg |> Cmd.map DateTimeInputMsg)
        |> UpdateRes
    | SetDescription description ->
        let state =
            { state with
                Description = description
            }
        (state, Cmd.none)
        |> UpdateRes
    | Submit ->
        match state.DateTimeInputState.ResultDateTime with
        | Ok dateTime ->
            Event.create dateTime state.Description
            |> SubmitRes
        | Error errMsg ->
            (state, Cmd.none)
            |> UpdateRes
    | Cancel ->
        CancelRes

let view (state: State) (dispatch: Msg -> unit) =
    Html.div [
        DateTimeInput.view state.DateTimeInputState (DateTimeInputMsg >> dispatch)

        Html.input [
            prop.value state.Description
            prop.onInput (fun e ->
                match e.target :?> Browser.Types.HTMLInputElement with
                | null ->
                    failwithf "e.target :?> Browser.Types.HTMLInputElement is null"
                | inputElement ->
                    (SetDescription inputElement.value) |> dispatch
            )
        ]

        Html.div [
            Html.button [
                prop.disabled (Result.isError state.DateTimeInputState.ResultDateTime)
                prop.onClick (fun _ -> dispatch Submit)
                prop.text "Submit"
            ]
            Html.button [
                prop.onClick (fun _ -> dispatch Cancel)
                prop.text "Cancel"
            ]
        ]
    ]
