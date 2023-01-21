module Utils.RegisterServiceWorker
open Fable.Core

type [<AllowNullLiteral>] IExports =
    abstract ``default``: unit -> unit

let [<Import("*","./registerServiceWorker.js")>] registerServiceWorker: IExports = jsNative
