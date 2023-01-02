module App

open Elmish
open Elmish.React

#if DEBUG
open Elmish.Debug
open Elmish.HMR
open Fable.Core
#endif



Program.mkProgram Index.init2 Index.update2 Index.view
#if DEBUG
|> Program.withConsoleTrace
#endif
|> Program.withReactSynchronous "elmish-app"
// #if DEBUG
// |> Program.withDebugger
// #endif
|> Program.run