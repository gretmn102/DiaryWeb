module EventView
open Elmish
open Feliz

open Components
open Commons

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
            prop.textf "%s" <| Utils.CustomDateTime.toString state.Event.DateTime
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
