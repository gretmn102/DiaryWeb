module Program
open Feliz
open Browser.Dom
open Fable.Builders.Fela
open Fable.Core
open Fable.Core

let view =
    Fela.RendererProvider {
        renderer (Fela.createRenderer ())

        Index.router ()
    }

module RegisterServiceWorker =
    type [<AllowNullLiteral>] IExports =
        abstract ``default``: unit -> unit

    let [<Import("*","./registerServiceWorker.js")>] registerServiceWorker: IExports = jsNative

RegisterServiceWorker.registerServiceWorker.``default`` ()

let root = ReactDOM.createRoot(document.getElementById "feliz-app")
root.render(view)
