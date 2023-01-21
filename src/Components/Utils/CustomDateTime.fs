module Components.Utils.CustomDateTime
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
