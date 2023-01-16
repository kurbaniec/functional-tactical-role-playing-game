module Index

open Browser.Types
open Elmish
open Fable.Core
open Fable.Core.JS
open Fable.Remoting.Client
open Fulma
open Shared
open Shared.DomainDto
open Fable.React
open Fable.React.Props

type ActionPreview =
    { name: string
      before: CharacterDto
      after: CharacterDto }

type UI =
    | Waiting
    | Started
    | Character of CharacterDto
    | Action of ActionPreview
    | Win of (PlayerDto*PlayerDto)

type Msg =
    | Started
    | Character of CharacterDto
    | Action of ActionPreview
    | Win of PlayerDto*PlayerDto


let appInit () : UI * Cmd<Msg> =
    let model = Waiting
    let cmd = Cmd.none
    model, cmd

let appUpdate (msg: Msg) (model: UI) : UI * Cmd<Msg> =
    match msg with
    | Msg.Started -> UI.Started, Cmd.none
    | Msg.Character character -> UI.Character character, Cmd.none
    | Msg.Action action -> UI.Action action, Cmd.none
    | Msg.Win (player, winner) -> UI.Win (player, winner), Cmd.none

open Browser // See: https://github.com/fable-compiler/fable-browser

type CustomEvent =
    inherit Event
    abstract member detail: Msg

// See: https://stackoverflow.com/a/61808412/12347616
let uiSub initial =
    let sub dispatch =
        window.addEventListener (
            "uiSub",
            fun (event: Event) -> event :?> CustomEvent |> fun e -> e.detail |> dispatch
        )

    Cmd.ofSub sub

let waitingView () =
    section [ Id "map-info"; Class "info-canvas hero is-info" ] [
        div [ Class "hero-body" ] [
            p [ Class "title" ] [
                str "Waiting for other player"
            ]
            p [ Class "subtitle" ] [
                str "This can take a while..."
            ]
        ]
    ]

let startedView () =
    section [ Id "map-info"; Class "info-canvas hero is-info" ] [
        div [ Class "hero-body" ] [
            p [ Class "title" ] [
                str "Joined Game"
            ]
            p [ Class "subtitle" ] [ str "🕹️" ]
        ]
    ]

let clsToEmoji (cls: CharacterClassDto) =
    match cls with
    | CharacterClassDto.Axe -> "🪓"
    | CharacterClassDto.Sword -> "⚔️"
    | CharacterClassDto.Lance -> "🔱"
    | CharacterClassDto.Bow -> "🏹"
    | CharacterClassDto.Support -> "🪄"
    | _ -> "⁉️"

let mvToEmoji (mv: MoveTypeDto) =
    match mv with
    | MoveTypeDto.Foot -> "🥾"
    | MoveTypeDto.Mount -> "🏇"
    | MoveTypeDto.Fly -> "🪶"
    | _ -> "⁉️"

let boldStr text =
    Text.span [ Modifiers [
                    Modifier.TextWeight TextWeight.Bold
                ] ] [
        str text
    ]

let propertiesToElement (properties: Dictionary<string, System.Object>) =
    let hp =
        p [] [
            boldStr $"""HP:  {properties["hp"]} / {properties["maxHp"]}"""
        ]

    let atk =
        p [] [
            boldStr $"""ATK: {properties["atk"]} """
        ]

    let def =
        p [] [
            boldStr $"""DEF: {properties["def"]} """
        ]

    let mv =
        p [] [
            boldStr $"""MV: {mvToEmoji (properties["mvType"] :?> MoveTypeDto)} ➡️ {properties["mv"]}"""
        ]

    if properties.ContainsKey "heal" then
        let heal =
            p [] [
                boldStr $"""HEAL: {properties["heal"]} """
            ]

        div [] [ hp; atk; heal; def; mv ]
    else
        div [] [ hp; atk; def; mv ]

let heroCls (character: CharacterDto) =
    if character.player.Value = PlayerDto.Player1 then
        "is-success"
    else
        "is-danger"

let characterView (character: CharacterDto) =
    let heroCls = heroCls character

    let title = $"{clsToEmoji character.classification} {character.name}"

    let properties = propertiesToElement character.properties

    section [ Id "map-info"; Class $"info-canvas hero {heroCls}" ] [
        div [ Class "hero-body" ] [
            p [ Class "title" ] [ str title ]
            properties
        ]
    ]

let actionView (preview: ActionPreview) =
    let heroCls = heroCls preview.before

    let title = p [ Class "title" ] [ str preview.name ]

    let character =
        p [ Class "subtitle" ] [
            boldStr $"{clsToEmoji preview.before.classification} {preview.before.name}"
        ]

    let hpBefore = preview.before.properties["hp"] :?> int
    let hpAfter = preview.after.properties["hp"] :?> int
    let maxHp = preview.after.properties["maxHp"] :?> int

    let health =
        if hpAfter <= 0 then
            p [] [
                boldStr $"""HP: {hpBefore} / {maxHp} ➡️ {hpAfter} / {maxHp} 💀"""
            ]
        else
            p [] [
                boldStr $"""HP: {hpBefore} / {maxHp} ➡️ {hpAfter} / {maxHp}"""
            ]

    section [ Id "map-info"; Class $"info-canvas hero {heroCls}" ] [
        div [ Class "hero-body" ] [
            title
            character
            health
        ]
    ]

let winView (player: PlayerDto) (winner: PlayerDto) =
    let isWin = player = winner
    let title = if isWin then "🏆 WIN" else "😞 LOSE"

    section [ Id "map-info"; Class "info-canvas hero is-warning" ] [
        div [ Class "hero-body" ] [
            p [ Class "title" ] [
                str title
            ]
            p [ Class "subtitle" ] [ str "🕹️ Refresh page to play again!" ]
        ]
    ]

let view (model: UI) (dispatch: Msg -> unit) =
    div [] [
        Navbar.navbar [] [
            Navbar.Item.a [] [
                boldStr "functional-tactical-role-playing-game"
            ]
        ]
        div [ Class "row" ] [
            canvas [ Id "map-canvas"; Class "map-canvas" ] []

            match model with
            | UI.Waiting -> waitingView ()
            | UI.Started -> startedView ()
            | UI.Character character -> characterView character
            | UI.Action preview -> actionView preview
            | UI.Win (player, winner) -> winView player winner
        ]

    ]

module Msg =
    let startedMsg () = Msg.Started
    let characterMsg (character: CharacterDto) = Msg.Character character
    let actionMsg (preview: ActionPreview) = Msg.Action preview
    let winMsg (player: PlayerDto) (winner: PlayerDto) = Msg.Win (player, winner)

type GameUI =
    abstract start: StartResult -> unit

type GameUIStatic =
    // [<Emit("new $0()")>]
    abstract create: unit -> Promise<GameUI>

[<Import("GameUI", "./GameUI.js")>]
let GameUI: GameUIStatic = jsNative

let gameApi =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.buildProxy<IGameApi>

// See: https://github.com/fable-compiler/Fable/issues/2115
let init () : Promise<GameInfo> =
    async {
        return! gameApi.start ()
    }
    |> Async.StartAsPromise

let pollServer (gameInfo: GameInfo) : Promise<Option<IResult>> =
    async { return! gameApi.poll gameInfo.id gameInfo.player }
    |> Async.StartAsPromise

let updateServer (msg: IMessage) (gameInfo: GameInfo) : Promise<unit> =
    async { return! gameApi.update gameInfo.id gameInfo.player msg }
    |> Async.StartAsPromise

let game = GameUI.create ()
