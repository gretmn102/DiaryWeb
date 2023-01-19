module EventsList
open Elmish
open Feliz
open Feliz.Router

open Commons
open Api

module SaveAndLoadEvents =
    open Elmish
    open Feliz

    type Msg =
        | Import of string

    type State =
        {
            ImportResult: Result<Api.LocalEvents, string> Deferred
        }

    let init =
        let state =
            {
                ImportResult = NotStartedYet
            }
        state

    type UpdateResult =
        | UpdateRes of State
        | ImportResult of State * Api.LocalEvents

    let update (msg: Msg) (state: State) =
        match msg with
        | Import rawJson ->
            let res =
                Api.LocalEvents.import rawJson

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

    let view (state: State) (dispatch: Msg -> unit) =
        Html.div [
            Upload.upload {|
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

module EventView =
    open Elmish
    open Feliz

    module DescripitionEditor =
        type Msg =
            | SetDescription of string
            | Submit
            | Cancel

        type State =
            {
                Description: string
            }

        let init description =
            let state =
                {
                    Description = description
                }
            state, Cmd.none

        type UpdateResult =
            | UpdateRes of State * Cmd<Msg>
            | SubmitRes of string
            | CancelRes

        let update (msg: Msg) (state: State) =
            match msg with
            | SetDescription description ->
                let state =
                    { state with
                        Description = description
                    }
                (state, Cmd.none)
                |> UpdateRes
            | Submit ->
                SubmitRes state.Description
            | Cancel ->
                CancelRes

        let view isInputEnabled (state: State) (dispatch: Msg -> unit) =
            Html.div [
                if isInputEnabled then
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
                        prop.onClick (fun _ -> dispatch Submit)
                        prop.text "Submit"
                    ]
                    Html.button [
                        prop.onClick (fun _ -> dispatch Cancel)
                        prop.text "Cancel"
                    ]
                ]
            ]

    type Msg =
        | StartEdit
        | DescripitionEditorMsg of DescripitionEditor.Msg
        | StartRemove
        | SetRemove of DescripitionEditor.Msg

    type State =
        {
            Event: Event
            DescripitionEditorState: DescripitionEditor.State Option
            IsStartedRemove: DescripitionEditor.State Option
        }

    let init arg =
        let state =
            {
                Event = arg
                DescripitionEditorState = None
                IsStartedRemove = None
            }
        state

    type UpdateResult =
        | UpdateRes of State * Cmd<Msg>
        | UpdateEventRes of Event
        | RemoveRes

    let update (msg: Msg) (state: State) =
        match msg with
        | StartEdit ->
            let state', msg = DescripitionEditor.init state.Event.Description
            let state =
                { state with
                    DescripitionEditorState = Some state'
                }
            (state, msg |> Cmd.map DescripitionEditorMsg)
            |> UpdateRes
        | DescripitionEditorMsg msg ->
            match state.DescripitionEditorState with
            | Some descripitionEditorState ->
                match DescripitionEditor.update msg descripitionEditorState with
                | DescripitionEditor.UpdateRes(state', msg) ->
                    let state =
                        { state with
                            DescripitionEditorState = Some state'
                        }
                    (state, msg |> Cmd.map DescripitionEditorMsg)
                    |> UpdateRes
                | DescripitionEditor.SubmitRes description ->
                    { state.Event with
                        Description = description
                    }
                    |> UpdateEventRes
                | DescripitionEditor.CancelRes ->
                    let state =
                        { state with
                            DescripitionEditorState = None
                        }
                    (state, Cmd.none)
                    |> UpdateRes
            | None ->
                (state, Cmd.none)
                |> UpdateRes
        | StartRemove ->
            let state', msg = DescripitionEditor.init state.Event.Description
            let state =
                { state with
                    IsStartedRemove = Some state'
                }
            (state, msg |> Cmd.map SetRemove)
            |> UpdateRes
        | SetRemove msg ->
            match state.IsStartedRemove with
            | Some descripitionEditorState ->
                match DescripitionEditor.update msg descripitionEditorState with
                | DescripitionEditor.UpdateRes(state', msg) ->
                    let state =
                        { state with
                            IsStartedRemove = Some state'
                        }
                    (state, msg |> Cmd.map SetRemove)
                    |> UpdateRes
                | DescripitionEditor.SubmitRes _ ->
                    RemoveRes
                | DescripitionEditor.CancelRes ->
                    let state =
                        { state with
                            IsStartedRemove = None
                        }
                    (state, Cmd.none)
                    |> UpdateRes
            | None ->
                (state, Cmd.none)
                |> UpdateRes

    let view (state: State) (dispatch: Msg -> unit) =
        Html.div [
            Html.div [
                prop.textf "%s" <| CustomDateTime.toString state.Event.DateTime
            ]

            match state.DescripitionEditorState with
            | Some descriptionEditorState ->
                DescripitionEditor.view true descriptionEditorState (DescripitionEditorMsg >> dispatch)
            | None ->
                Html.div [
                    prop.text state.Event.Description
                ]

                match state.IsStartedRemove with
                | Some descriptionEditorState ->
                    DescripitionEditor.view false descriptionEditorState (SetRemove >> dispatch)
                | None ->
                    Html.button [
                        prop.text "Edit"
                        prop.onClick (fun _ ->
                            dispatch StartEdit
                        )
                    ]

                    Html.button [
                        prop.text "Remove"
                        prop.onClick (fun _ ->
                            dispatch StartRemove
                        )
                    ]
        ]

type Msg =
    | StartNewEvent
    | SetEvent of Event
    | RemoveEvent of Event
    | EventViewMsg of System.DateTime * EventView.Msg
    | SaveAndLoadEventsMsg of SaveAndLoadEvents.Msg
    | Export

type State =
    {
        LocalEvents: LocalEvents
        Events: Map<System.DateTime, EventView.State>
        SaveAndLoadEventsState: SaveAndLoadEvents.State
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
            SaveAndLoadEventsState =
                SaveAndLoadEvents.init
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
    | SaveAndLoadEventsMsg msg ->
        match SaveAndLoadEvents.update msg state.SaveAndLoadEventsState with
        | SaveAndLoadEvents.UpdateRes(state') ->
            let state =
                { state with
                    SaveAndLoadEventsState = state'
                }
            state, Cmd.none
        | SaveAndLoadEvents.ImportResult(state', localEvents) ->
            let state, cmd = init localEvents
            let state =
                { state with
                    SaveAndLoadEventsState = state'
                }
            state, cmd
    | Export ->
        LocalEvents.export state.LocalEvents
        |> saveToDisc "application/json" "events.json"

        state, Cmd.none

let view (state: State) (dispatch: Msg -> unit) =
    Html.div [
        Html.div [
            Html.button [
                prop.text "save"
                prop.onClick (fun _ ->
                    dispatch Export
                )
            ]

            SaveAndLoadEvents.view state.SaveAndLoadEventsState (SaveAndLoadEventsMsg >> dispatch)
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
