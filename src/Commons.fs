module Commons
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

module Routes =
    [<Literal>]
    let EventsListPageRoute = "EventsListPageRoute"

    [<Literal>]
    let EventsAdderPageRoute = "EventsAdderPageRoute"
