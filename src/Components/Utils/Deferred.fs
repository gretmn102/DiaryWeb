namespace Components.Utils
type 'a Deferred =
    | NotStartedYet
    | InProgress
    | Resolved of 'a
