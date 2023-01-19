module Commons
type 'a Deferred =
    | NotStartedYet
    | InProgress
    | Resolved of 'a

type Event =
    {
        DateTime: System.DateTime
        Description: string
    }

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
[<RequireQualifiedAccess>]
module Event =
    let create dateTime description =
        {
            DateTime = dateTime
            Description = description
        }

module Result =
    let defaultWith fn (result: Result<_, _>) =
        match result with
        | Ok x -> x
        | Error x -> fn x

    let isError (result: Result<_, _>) =
        match result with
        | Error _ -> true
        | Ok _ -> false

module Routes =
    [<Literal>]
    let CounterPageRoute = "CounterPageRoute"

    [<Literal>]
    let EventsListPageRoute = "EventsListPageRoute"

    [<Literal>]
    let EventsAdderPageRoute = "EventsAdderPageRoute"
