module Components.Utils.ResultExt.Result
let defaultWith fn (result: Result<_, _>) =
    match result with
    | Ok x -> x
    | Error x -> fn x

let isError (result: Result<_, _>) =
    match result with
    | Error _ -> true
    | Ok _ -> false
