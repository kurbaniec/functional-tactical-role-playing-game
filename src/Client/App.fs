module App

open System
open Elmish
open Elmish.React

#if DEBUG
open Elmish.Debug
open Elmish.HMR
open Fable.Core
#endif



Program.mkProgram Index.appInit Index.appUpdate Index.view
#if DEBUG
// |> Program.withConsoleTrace
#endif
|> Program.withReactSynchronous "elmish-app"
|> Program.withSubscription Index.uiSub
// #if DEBUG
// |> Program.withDebugger
// #endif
|> Program.run