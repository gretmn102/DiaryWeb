module Index
open Elmish
open Feliz
open Feliz.UseElmish
open Feliz.Router

open Commons

type Page =
    | EventsListPage
    | EventsAdderPage
    | NotFoundPage of string

type Msg =
    | ChangePage of Page
    | EventsListMsg of EventsList.Msg
    | EventsAdderMsg of EventAdder.Msg

type State =
    {
        EventsListState: EventsList.State
        CurrentPage: Page
        EventsAdderState: EventAdder.State
    }

let parseRoute currentUrl =
    match currentUrl with
    | [] -> EventsListPage
    | Routes.EventsListPageRoute::_ -> EventsListPage
    | Routes.EventsAdderPageRoute::_ -> EventsAdderPage
    | xs ->
        NotFoundPage (sprintf "%A" xs)

let init rawRoute =
    let eventsListState, eventsListMsg =
        Api.LocalEvents.get()
        |> Result.defaultWith (fun errMsg ->
            failwithf "%s" errMsg // TODO
        )
        |> EventsList.init
    let eventsAdderState, eventsAdderMsg =
        EventAdder.init ()
    let state =
        {
            CurrentPage = parseRoute rawRoute
            EventsListState = eventsListState
            EventsAdderState = eventsAdderState
        }
    let msg =
        Cmd.batch [
            eventsListMsg |> Cmd.map EventsListMsg
            eventsAdderMsg |> Cmd.map EventsAdderMsg
        ]
    state, msg

let update (msg: Msg) (state: State) =
    match msg with
    | EventsListMsg msg ->
        let state', msg = EventsList.update msg state.EventsListState
        let state =
            { state with
                EventsListState = state'
            }
        state, msg |> Cmd.map EventsListMsg
    | EventsAdderMsg msg ->
        match EventAdder.update msg state.EventsAdderState with
        | EventAdder.UpdateRes (state', msg) ->
            let state =
                { state with
                    EventsAdderState = state'
                }
            state, msg |> Cmd.map EventsAdderMsg
        | EventAdder.SubmitRes newEvent ->
            let cmd =
                Cmd.ofMsg (EventsList.SetEvent newEvent)
                |> Cmd.map EventsListMsg
            let cmd =
                Cmd.batch [
                    cmd
                    Cmd.navigate [| Routes.EventsListPageRoute |]
                ]
            state, cmd
        | EventAdder.CancelRes ->
            let cmd =
                Cmd.navigate [| Routes.EventsListPageRoute |]
            state, cmd
    | ChangePage page ->
        let state =
            { state with
                CurrentPage = page
            }
        state, Cmd.none

let router = React.functionComponent(fun () ->
    let state, dispatch = React.useElmish(init, update, Router.currentUrl())

    React.router [
        router.onUrlChanged (parseRoute >> ChangePage >> dispatch)
        router.children [
            match state.CurrentPage with
            | EventsListPage ->
                EventsList.view state.EventsListState (EventsListMsg >> dispatch)
            | EventsAdderPage ->
                EventAdder.view state.EventsAdderState (EventsAdderMsg >> dispatch)
            | NotFoundPage query ->
                Html.h1 (sprintf "Not found %A" query)
        ]
    ]
)
