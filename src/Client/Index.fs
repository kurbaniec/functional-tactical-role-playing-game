module Index

open Elmish
open Fable.Core
open Fable.Remoting.Client
open Shared
open Shared.DomainDto

type Model = { Todos: Todo list; Input: string }

type Msg =
    | GotTodos of Todo list
    | SetInput of string
    | AddTodo
    | AddedTodo of Todo


let kek =
    let a = 2
    ()

open Fable.Core.JsInterop

type IScene =
    abstract createScene: unit -> unit
    abstract triggerAlert: message: string -> unit

[<ImportAll("./Scene.js")>]
let sceneJs: IScene = jsNative

let todosApi =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.buildProxy<ITodosApi>

// console.log ("here 3")

let init () : Model * Cmd<Msg> =
    let model = { Todos = []; Input = "" }

    // console.log ("here")
    printfn "here"
    sceneJs.createScene ()
    printfn "here"
    // console.log ("here")

    let cmd = Cmd.OfAsync.perform todosApi.getTodos () GotTodos



    model, cmd

let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
    match msg with
    | GotTodos todos -> { model with Todos = todos }, Cmd.none
    | SetInput value -> { model with Input = value }, Cmd.none
    | AddTodo ->
        let todo = Todo.create model.Input

        let cmd = Cmd.OfAsync.perform todosApi.addTodo todo AddedTodo

        { model with Input = "" }, cmd
    | AddedTodo todo -> { model with Todos = model.Todos @ [ todo ] }, Cmd.none

open Feliz
open Feliz.Bulma

let navBrand =
    Bulma.navbarBrand.div [
        Bulma.navbarItem.a [
            prop.href "https://safe-stack.github.io/"
            navbarItem.isActive
            prop.children [
                Html.img [
                    prop.src "/favicon.png"
                    prop.alt "Logo"
                ]
            ]
        ]
    ]

let containerBox (model: Model) (dispatch: Msg -> unit) =
    Bulma.box [
        Bulma.content [
            Html.ol [
                for todo in model.Todos do
                    Html.li [ prop.text todo.Description ]
            ]
        ]
        Bulma.field.div [
            field.isGrouped
            prop.children [
                Bulma.control.p [
                    control.isExpanded
                    prop.children [
                        Bulma.input.text [
                            prop.value model.Input
                            prop.placeholder "What needs to be done?"
                            prop.onChange (fun x -> SetInput x |> dispatch)
                        ]
                    ]
                ]
            ]
        ]
    ]

let view (model: Model) (dispatch: Msg -> unit) =
    Bulma.navbar [
        prop.children [

        ]
    ]
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
    [<Emit("new $0()")>]
    abstract Create: unit -> GameUI

[<Import("GameUI", "./GameUI.js")>]
let GameUI: GameUIStatic = jsNative

let gameUI = GameUI.Create()


let words = System.Collections.Generic.Dictionary<string, System.Object>()
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
      board = [ [ TileDto.Land; TileDto.Water ] ]
      characters = [ char ]

    }

gameUI.start test
