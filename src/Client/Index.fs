module Index

open Fable.Core
open Fable.Core.JS
open Fable.Remoting.Client
open Shared
open Shared.DomainDto

// This module starts the non-functional `GameUI` class
// that renders the game via Babylon.js
// Also provides handler functions that can be used from JavaScript

type GameUI =
    abstract start: StartResult -> unit

type GameUIStatic =
    abstract create: unit -> Promise<GameUI>

[<Import("GameUI", "./GameUI.js")>]
let GameUI: GameUIStatic = jsNative

let gameApi =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.buildProxy<IGameApi>

// See: https://github.com/fable-compiler/Fable/issues/2115
let init () : Promise<GameInfo> =
    async { return! gameApi.start () } |> Async.StartAsPromise

let pollServer (gameInfo: GameInfo) : Promise<Option<IResult>> =
    async { return! gameApi.poll gameInfo.id gameInfo.player }
    |> Async.StartAsPromise

let updateServer (msg: IMessage) (gameInfo: GameInfo) : Promise<unit> =
    async { return! gameApi.update gameInfo.id gameInfo.player msg }
    |> Async.StartAsPromise
