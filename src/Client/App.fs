module App

open Elmish
open Elmish.React

#if DEBUG
open Elmish.Debug
open Elmish.HMR
#endif

open Index


Program.mkProgram Interface.appInit Interface.appUpdate Interface.view
// #if DEBUG
// |> Program.withConsoleTrace
// #endif
|> Program.withReactSynchronous "elmish-app"
|> Program.withSubscription Interface.uiSub
// #if DEBUG
// |> Program.withDebugger
// #endif
|> Program.run

let game = GameUI.create ()