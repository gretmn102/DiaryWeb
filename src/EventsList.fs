module EventsList
open Elmish
open Feliz
open Feliz.Router

open Commons
open Api

type Msg =
    | StartNewEvent
    | SetEvent of Event

type State =
    {
        Events: LocalEvents
    }

let init arg =
    let state =
        {
            Events = arg
        }
    state, Cmd.none

let update (msg: Msg) (state: State) =
    match msg with
    | SetEvent e ->
        let state =
            { state with
                Events =
                    state.Events
                    |> LocalEvents.set e
            }
        state, Cmd.none
    | StartNewEvent ->
        state, Cmd.navigate [| Routes.EventsAdderPageRoute |]

let eventView (event: Event) dispatch =
    Html.div [
        Html.div [
            prop.textf "%A" event.DateTime
        ]
        Html.div [
            prop.text event.Description
        ]
    ]

let view (state: State) (dispatch: Msg -> unit) =
    Html.div [
        Html.div [
            Html.button [
                prop.text "add"
                prop.onClick (fun e ->
                    dispatch StartNewEvent
                )
            ]
        ]
        Html.div [
            yield!
                state.Events.Cache
                |> Seq.sortByDescending (fun (KeyValue(dateTime, _)) -> dateTime)
                |> Seq.map (fun (KeyValue(_, e)) -> eventView e dispatch)
        ]
    ]
