module Index
open Elmish
open Feliz
open Feliz.UseElmish
open Feliz.Router

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
    | NotFoundPage of string

[<Literal>]
let CounterPageRoute = "CounterPage"

type Msg =
    | CounterMsg of Counter.Msg
    | ChangePage of Page

type State =
    {
        CounterState: Counter.State
        CurrentPage: Page
    }

let parseRoute currentUrl =
    match currentUrl with
    | [] -> CounterPage
    | CounterPageRoute::_ -> CounterPage
    | xs ->
        NotFoundPage (sprintf "%A" xs)

let init arg =
    let state, msg = Counter.init ()

    let state =
        {
            CounterState = state
            CurrentPage = CounterPage
        }

    state, msg |> Cmd.map CounterMsg

let update (msg: Msg) (state: State) =
    match msg with
    | CounterMsg msg ->
        let state', msg = Counter.update msg state.CounterState

        let state =
            { state with
                CounterState = state'
            }
        state, msg |> Cmd.map CounterMsg
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
            | NotFoundPage query ->
                Html.h1 (sprintf "Not found %A" query)
        ]
    ]
)
