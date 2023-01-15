module Index

open Browser.Types
open Elmish
open Fable.Core
open Fable.Core.JS
open Fable.Remoting.Client
open Fulma
open Shared
open Shared.DomainDto

// type Model = { Todos: Todo list; Input: string }
//
// type Msg =
//     | GotTodos of Todo list
//     | SetInput of string
//     | AddTodo
//     | AddedTodo of Todo
//     | Start of StartResult
//
//
// let kek =
//     let a = 2
//     ()
//
// let timer onTick =
//     let start dispatch =
//         let intervalId =
//             JS.setInterval
//                 (fun _ -> dispatch (onTick))
//                 1000
//         { new IDisposable with
//             member _.Dispose() = JS.clearInterval intervalId }
//     start
//
// let subscribe model =
//     [ ["timer"], timer "Tick" ]

open Fable.Core.JsInterop

// type IScene =
//     abstract createScene: unit -> unit
//     abstract triggerAlert: message: string -> unit
//
// [<ImportAll("./Scene.js")>]
// let sceneJs: IScene = jsNative
//


// console.log ("here 3")

// let init2 () : Model * Cmd<Msg> =
//     let model = { Todos = []; Input = "" }
//
//     // console.log ("here")
//     // printfn "here"
//     // sceneJs.createScene ()
//     // printfn "here"
//     // console.log ("here")
//
//     // let cmd = Cmd.OfAsync.perform todosApi.getTodos () GotTodos
//     let cmd = Cmd.none
//     // let cmd = Cmd.OfAsync.perform todosApi.start () Start
//
//     model, cmd

// let update2 (msg: Msg) (model: Model) : Model * Cmd<Msg> =
//     match msg with
//     | Start startResult ->
//         printfn $"%A{startResult}"
//
//         match startResult :> obj with
//         | :? StartResult -> printfn ("| start result")
//         | _ -> printfn "| kek"
//
//         model, Cmd.none
//
//     | GotTodos todos -> { model with Todos = todos }, Cmd.none
//     | SetInput value -> { model with Input = value }, Cmd.none
//     | AddTodo ->
//         let todo = Todo.create model.Input
//
//         //let cmd = Cmd.OfAsync.perform todosApi.addTodo todo AddedTodo
//
//         // { model with Input = "" }, cmd
//         { model with Input = "" }, Cmd.none
//     | AddedTodo todo -> { model with Todos = model.Todos @ [ todo ] }, Cmd.none



// let navBrand =
//     Bulma.navbarBrand.div [
//         Bulma.navbarItem.a [
//             prop.href "https://safe-stack.github.io/"
//             navbarItem.isActive
//             prop.children [
//                 Html.img [
//                     prop.src "/favicon.png"
//                     prop.alt "Logo"
//                 ]
//             ]
//         ]
//     ]
//
// let containerBox (model: Model) (dispatch: Msg -> unit) =
//     Bulma.box [
//         Bulma.content [
//             Html.ol [
//                 for todo in model.Todos do
//                     Html.li [ prop.text todo.Description ]
//             ]
//         ]
//         Bulma.field.div [
//             field.isGrouped
//             prop.children [
//                 Bulma.control.p [
//                     control.isExpanded
//                     prop.children [
//                         Bulma.input.text [
//                             prop.value model.Input
//                             prop.placeholder "What needs to be done?"
//                             prop.onChange (fun x -> SetInput x |> dispatch)
//                         ]
//                     ]
//                 ]
//             ]
//         ]
//     ]

open Feliz
open Feliz.Bulma
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
// | Win

type Msg =
    | Started
    | Character of CharacterDto
    | Action of ActionPreview


let appInit () : UI * Cmd<Msg> =
    let model = Waiting
    let cmd = Cmd.none
    model, cmd

let appUpdate (msg: Msg) (model: UI) : UI * Cmd<Msg> =
    match msg with
    | Msg.Started -> UI.Started, Cmd.none
    | Msg.Character character -> UI.Character character, Cmd.none
    | Msg.Action action -> UI.Action action, Cmd.none

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

let boldStr text =
    Text.span [ Modifiers [ Modifier.TextWeight TextWeight.Bold ] ] [ str text ]

let propertiesToElement (properties: Dictionary<string, System.Object>) =
    let hp = p [] [ boldStr $"""HP:  {properties[ "hp" ] } / {properties[ "maxHp" ]}""" ]
    let atk = p [] [ boldStr $"""ATK: {properties[ "atk" ] } """ ]
    let def = p [] [ boldStr $"""DEF: {properties[ "def" ] } """ ]
    if properties.ContainsKey "heal" then
        let heal = p [] [ boldStr $"""HEAL: {properties[ "heal" ] } """ ]
        div [] [ hp; atk; heal; def ]
    else
        div [] [ hp; atk; def ]



let characterView (character: CharacterDto) =
    let heroCls =
        if character.player.Value = PlayerDto.Player1 then
            "is-success"
        else
            "is-danger"
    let title = $"{clsToEmoji character.classification} {character.name}"

    let properties = propertiesToElement character.properties

    section [ Id "map-info"; Class $"info-canvas hero {heroCls}" ] [
        div [ Class "hero-body" ] [
            p [ Class "title" ] [
                str title
            ]
            properties
        ]
    ]

let mock () =
    section [ Id "map-info"; Class "info-canvas hero is-info" ] [
        div [ Class "hero-body" ] [
            p [ Class "title" ] [
                str "Not implemented"
            ]
            p [ Class "subtitle" ] [ str "🕹️" ]
        ]
    ]

let view (model: UI) (dispatch: Msg -> unit) =
    div [] [
        Bulma.navbar [ prop.children [] ]
        div [ Class "row" ] [
            canvas [ Id "map-canvas"; Class "map-canvas" ] []

            match model with
            | UI.Waiting -> waitingView ()
            | UI.Started -> startedView ()
            | UI.Character character -> characterView character
            | UI.Action character -> mock ()
        ]

    ]

module Msg =
    let startedMsg () = Msg.Started
    let characterMsg (character: CharacterDto) = Msg.Character character
    let actionMsg (preview: ActionPreview) = Msg.Action preview

// ()
// Bulma.hero [
//     hero.isFullHeight
//     color.isPrimary
//     prop.style [
//         style.backgroundSize "cover"
//         style.backgroundImageUrl "https://unsplash.it/1200/900?random"
//         style.backgroundPosition "no-repeat center center fixed"
//     ]
//     prop.children [
//         Bulma.heroHead [
//             Bulma.navbar [
//                 Bulma.container [ navBrand ]
//             ]
//         ]
//         Bulma.heroBody [
//             Bulma.container [
//                 Bulma.column [
//                     column.is6
//                     column.isOffset3
//                     prop.children [
//                         Bulma.title [
//                             text.hasTextCentered
//                             prop.text "safe_test"
//                         ]
//                         containerBox model dispatch
//                     ]
//                 ]
//             ]
//         ]
//     ]
// ]

type GameUI =
    abstract start: StartResult -> unit

type GameUIStatic =
    // [<Emit("new $0()")>]
    abstract create: unit -> Promise<GameUI>

[<Import("GameUI", "./GameUI.js")>]
let GameUI: GameUIStatic = jsNative

// let gameUI = GameUI.Create()

// See: https://github.com/fable-compiler/Fable/issues/2115

// type GameInfo = { id: string; player: PlayerDto }


let gameApi =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.buildProxy<IGameApi>

let init () : Promise<GameInfo> =
    async {
        return! gameApi.start ()
    //return { id = id; player = player }
    }
    |> Async.StartAsPromise

let pollServer (gameInfo: GameInfo) : Promise<Option<IResult>> =
    async { return! gameApi.poll gameInfo.id gameInfo.player }
    |> Async.StartAsPromise

let updateServer (msg: IMessage) (gameInfo: GameInfo) : Promise<unit> =
    async { return! gameApi.update gameInfo.id gameInfo.player msg }
    |> Async.StartAsPromise

let game = GameUI.create ()

// async {
//
//     printf "Started"
// }
// |> Async.StartAsPromise

// async {
//     let! id, player = todosApi.start()
//     printf "Received reply"
//     do! Async.Sleep(3000)
//     printf "Waited a bit"
//     printf "hey"
//     let! res = todosApi.poll id player
//     printf "wow"
//     match res with
//     | Some r ->
//         JS.console.log(r)
//         printfn $"%A{r}"
// }
// |> Async.StartAsPromise


(*let words = System.Collections.Generic.Dictionary<string, System.Object>()
words.Add("1", "book")
words.Add("2", 2)

let c: CharacterDto =
    { id = ""
      name = ""
      classification = CharacterClassDto.Axe
      properties = words

    }

let char: PlaceCharacterDto =
    { player = PlayerDto.Player1
      character = c
      pos = { row = 1; col = 2 }

    }

let test: StartResult =
    { id = "test"
      board = System.Collections.Generic.List([ System.Collections.Generic.List([ TileDto.Land; TileDto.Water ]) ])
      characters = System.Collections.Generic.List([ char ]) }

gameUI.start test

// Check for concrete type
// See: https://stackoverflow.com/q/5368655
// See: https://stackoverflow.com/a/5369315
let test2 (kek: IResult) =
    match box kek with
    | :? StartResult -> printfn "start result"
    | _ -> printfn "kek"

let test3 (kek: IResult) =
    match kek :> obj with
    | :? StartResult -> printfn ("start result")
    | _ -> printfn "kek"

test2 test
test3 test*)
