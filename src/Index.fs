module Index
open Elmish
open Feliz
open Feliz.UseElmish
open Feliz.Router

open Commons

module Counter =
    open Elmish

    type Msg =
        | Incr
        | Decr

    type State =
        {
            Counter: int
        }

    let init arg =
        let state =
            {
                Counter = 0
            }
        state, Cmd.none

    let update (msg: Msg) (state: State) =
        match msg with
        | Incr ->
            let state =
                { state with
                    Counter = state.Counter + 1
                }
            state, Cmd.none
        | Decr ->
            let state =
                { state with
                    Counter = state.Counter - 1
                }
            state, Cmd.none

    let view (state: State) (dispatch: Msg -> unit) =
        Html.div [
            Html.div [
                prop.text (sprintf "Counter is %d" state.Counter)
            ]
            Html.div [
                Html.div [
                    Html.button [
                        prop.onClick (fun _ -> dispatch Decr)
                        prop.children [
                            Html.i [
                                prop.text "-"
                            ]
                        ]
                    ]
                    Html.button [
                        prop.onClick (fun _ -> dispatch Incr)
                        prop.children [
                            Html.i [
                                prop.text "+"
                            ]
                        ]
                    ]
                ]
            ]
        ]

type Page =
    | CounterPage
    | EventsListPage
    | EventsAdderPage
    | NotFoundPage of string

type Msg =
    | ChangePage of Page
    | CounterMsg of Counter.Msg
    | EventsListMsg of EventsList.Msg
    | EventsAdderMsg of EventAdder.Msg

type State =
    {
        CounterState: Counter.State
        EventsListState: EventsList.State
        CurrentPage: Page
        EventsAdderState: EventAdder.State
    }

let parseRoute currentUrl =
    match currentUrl with
    | [] -> EventsListPage
    | Routes.CounterPageRoute::_ -> CounterPage
    | Routes.EventsListPageRoute::_ -> EventsListPage
    | Routes.EventsAdderPageRoute::_ -> EventsAdderPage
    | xs ->
        NotFoundPage (sprintf "%A" xs)

let init rawRoute =
    let counterState, counterMsg = Counter.init ()
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
            CounterState = counterState
            CurrentPage = parseRoute rawRoute
            EventsListState = eventsListState
            EventsAdderState = eventsAdderState
        }
    let msg =
        Cmd.batch [
            counterMsg |> Cmd.map CounterMsg
            eventsListMsg |> Cmd.map EventsListMsg
            eventsAdderMsg |> Cmd.map EventsAdderMsg
        ]
    state, msg

let update (msg: Msg) (state: State) =
    match msg with
    | CounterMsg msg ->
        let state', msg = Counter.update msg state.CounterState
        let state =
            { state with
                CounterState = state'
            }
        state, msg |> Cmd.map CounterMsg
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
            | CounterPage ->
                Counter.view state.CounterState (CounterMsg >> dispatch)
            | EventsListPage ->
                EventsList.view state.EventsListState (EventsListMsg >> dispatch)
            | EventsAdderPage ->
                EventAdder.view state.EventsAdderState (EventsAdderMsg >> dispatch)
            | NotFoundPage query ->
                Html.h1 (sprintf "Not found %A" query)
        ]
    ]
)
