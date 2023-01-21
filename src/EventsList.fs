module EventsList
open Elmish
open Feliz
open Feliz.Router

open Commons
open Components
open Api

type Msg =
    | StartNewEvent
    | SetEvent of Event
    | RemoveEvent of Event
    | EventViewMsg of System.DateTime * EventView.Msg
    | UploadElmishMsg of Upload.Elmish.Msg

type State =
    {
        LocalEvents: LocalEvents
        Events: Map<System.DateTime, EventView.State>
        UploadElmishState: Upload.Elmish.State<LocalEvents>
    }

let init localEvents =
    let state =
        {
            LocalEvents = localEvents
            Events =
                localEvents.Cache
                |> Map.map (fun _ e ->
                    EventView.init e
                )
            UploadElmishState =
                Upload.Elmish.init
        }
    state, Cmd.none

let update (msg: Msg) (state: State) =
    match msg with
    | SetEvent e ->
        let state =
            { state with
                LocalEvents =
                    state.LocalEvents
                    |> LocalEvents.set e
                Events =
                    Map.add e.DateTime (EventView.init e) state.Events
            }
        state, Cmd.none
    | RemoveEvent e ->
        let state =
            { state with
                LocalEvents =
                    state.LocalEvents
                    |> LocalEvents.remove e.DateTime
                Events =
                    Map.remove e.DateTime state.Events
            }
        state, Cmd.none
    | StartNewEvent ->
        state, Cmd.navigate [| Routes.EventsAdderPageRoute |]
    | EventViewMsg (dateTime, msg) ->
        match Map.tryFind dateTime state.Events with
        | Some eventEventState ->
            match EventView.update msg eventEventState with
            | EventView.UpdateRes(state', msg) ->
                let state =
                    { state with
                        Events =
                            Map.add dateTime state' state.Events
                    }
                state, msg |> Cmd.map (fun cmd -> EventViewMsg(dateTime, cmd))
            | EventView.UpdateEventRes e ->
                state, Cmd.ofMsg (SetEvent e)
            | EventView.RemoveRes ->
                state, Cmd.ofMsg (RemoveEvent eventEventState.Event)
        | None ->
            state, Cmd.none
    | UploadElmishMsg msg ->
        match Upload.Elmish.update LocalEvents.import msg state.UploadElmishState with
        | Upload.Elmish.UpdateRes(state') ->
            let state =
                { state with
                    UploadElmishState = state'
                }
            state, Cmd.none
        | Upload.Elmish.ImportResult(state', localEvents) ->
            let state, cmd = init localEvents
            let state =
                { state with
                    UploadElmishState = state'
                }
            state, cmd

let view (state: State) (dispatch: Msg -> unit) =
    Html.div [
        Html.div [
            Download.download {|
                description = "save"
                accept = "application/json"
                filename = "events.json"
                getData = fun () -> LocalEvents.export state.LocalEvents
                onDone = id
            |}

            Upload.Elmish.view state.UploadElmishState (UploadElmishMsg >> dispatch)
        ]

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
                state.Events
                |> Seq.sortByDescending (fun (KeyValue(dateTime, _)) -> dateTime)
                |> Seq.map (fun (KeyValue(k, e)) ->
                    EventView.view e (fun msg -> EventViewMsg(k, msg) |> dispatch)
                )
        ]
    ]
