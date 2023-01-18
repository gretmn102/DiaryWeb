module Program
open Feliz
open Browser.Dom

let root = ReactDOM.createRoot(document.getElementById "feliz-app")
root.render(Index.router())
