module Api
// open Browser
open Thoth

open Commons

type Events = Map<System.DateTime, Event>
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
[<RequireQualifiedAccess>]
module Events =
    let empty = Map.empty

type LocalEvents =
    {
        Cache: Events
    }
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
[<RequireQualifiedAccess>]
module LocalEvents =
    let localKey = "DiaryWeb"

    let get () : Result<LocalEvents, string> =
        match Browser.WebStorage.localStorage.getItem localKey with
        | null ->
            // Error (sprintf "%s is empty" localKey)
            {
                Cache = Events.empty
            }
            |> Ok
        | res ->
            Json.Decode.Auto.fromString res
            |> Result.map (fun events ->
                {
                    Cache = events
                }
            )

    let set (event: Event) (localEventsApi: LocalEvents) =
        let events =
            Map.add event.DateTime event localEventsApi.Cache

        Browser.WebStorage.localStorage.setItem (localKey, Json.Encode.Auto.toString events)

        { localEventsApi with
            Cache = events
        }

    let insert dateTime description (localEventsApi: LocalEvents) =
        let newEvent = Event.create dateTime description
        set newEvent localEventsApi
