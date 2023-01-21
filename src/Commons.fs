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
    let EventsListPageRoute = "EventsListPageRoute"

    [<Literal>]
    let EventsAdderPageRoute = "EventsAdderPageRoute"

module CustomDateTime =
    let tryParse =
        let r = System.Text.RegularExpressions.Regex(@"(\d{1,2})\.(\d{1,2})\.(\d{2,4}) (\d{1,2}):(\d{1,2})")
        fun  text ->
            let res = r.Match(text)
            if res.Success then
                let groups = res.Groups
                let get (i: int) =
                    int groups.[i].Value

                System.DateTime(get 3, get 2, get 1, get 4, get 5, 0, System.DateTimeKind.Local)
                |> Ok
            else
                Error "Something wrong when parse datetime"

    let toString (dateTime: System.DateTime) =
        dateTime.ToLocalTime().ToString("dd.MM.yyyy HH:mm")
