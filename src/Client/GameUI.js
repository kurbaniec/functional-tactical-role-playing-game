// See: https://github.com/fable-compiler/fable3-samples/blob/main/interopFableFromJS/src/index.js
import {init, MsgModule_startedMsg, pollServer, updateServer} from "./output/Index"
import {GameInfo, DomainDto_CharacterDto, DomainDto_IMessage, DomainDto_IResult} from "./output/Shared/Shared";
import {
    boardPosToVec3,
    eachRecursive, modulo,
    Player,
    positionDtoToVec3,
    setVec3FromPositionDto,
    simpleRecordName,
    unwrap,
    vec3ToPositionDto
} from "./Utils";
import {
    AbstractMesh,
    ArcRotateCamera,
    Color3, Color4,
    Engine,
    HemisphericLight,
    MeshBuilder,
    MultiMaterial,
    Scene, SceneLoader,
    StandardMaterial,
    SubMesh, Tools,
    Vector3
} from "@babylonjs/core";
import {Input, InputManager} from "./components/InputManager"
import {Cursor} from "./components/Cursor"
import {Selector} from "./components/Selector"
import {Character} from "./components/Character"
import {sendActionMsg, sendCharacterMsg, sendStartedMsg, sendWinMsg} from "./components/Subscription";

class GameUI {

    /** @param {DomainDto_PlayerOverseeResult} result **/
    onPlayerOverseeResult(result) {
        this.gameState.turnOf = result.player
        this.removeHighlight();
        this.cursor.hide()
        this.selection.clear();

        if (!this.myTurn()) return;
        this.highlightCharacters(result.selectableCharacters)
        this.cursor.show()
        this.showCharacterInfo(this.cursor.positionDto)

        this.gameState.update = (input) => {
            if (input === Input.Enter) {
                const character = this.findCharacter(this.cursor.positionDto)
                if (!character) return;
                updateServer(new DomainDto_IMessage(
                0, [character.id]
                ), this.gameInfo)
            } else {
                this.cursor.moveCursor(input)
            }
            this.showCharacterInfo(this.cursor.positionDto)
        }
    }

    /** @param {DomainDto_PlayerMoveSelectionResult} result **/
    onPlayerMoveSelectionResult(result) {
        this.removeHighlight();
        this.highlightPositions(result.availableMoves)

        this.gameState.update = (input) => {
            if (input === Input.Enter) {
                const pos = this.cursor.positionDto
                updateServer(new DomainDto_IMessage(
                    2, pos
                ), this.gameInfo)
            } if (input === Input.Escape) {
                updateServer(new DomainDto_IMessage(1), this.gameInfo)
            } else {
                this.cursor.moveCursor(input)
            }
            this.showCharacterInfo(this.cursor.positionDto)
        }
    }

    /** @param {DomainDto_PlayerActionSelectionResult} result **/
    onPlayerActionSelectionResult(result) {
        this.removeHighlight()
        this.selection.setSelections(result.availableActions)

        this.gameState.update = (input) => {
            if (input === Input.Enter) {
                const selection = this.selection.currentSelection()
                updateServer(new DomainDto_IMessage(
                    3, selection
                ), this.gameInfo)
            } else {
                this.selection.moveCursor(input)
            }
        }
    }

    /** @param {DomainDto_PlayerActionResult} result **/
    onPlayerActionResult(result) {
        this.selection.clear()
        this.highlightCharacters(result.selectableCharacters)
        const preview = result.preview
        this.showActionOrCharacterInfo(this.cursor.positionDto, preview)

        this.gameState.update = (input) => {
            if (input === Input.Enter) {
                const character = this.findCharacter(this.cursor.positionDto)
                if (!character) return;
                updateServer(new DomainDto_IMessage(
                    5, [character.id]
                ), this.gameInfo)
            } if (input === Input.Escape) {
                updateServer(new DomainDto_IMessage(4), this.gameInfo)
            } else {
                this.cursor.moveCursor(input)
            }
            this.showActionOrCharacterInfo(this.cursor.positionDto, preview)
        }
    }

    /** @param {DomainDto_PlayerWinResult} result **/
    onPlayerWinResult(result) {
        this.removeHighlight()
        this.cursor.hide()
        this.isPolling = false
        sendWinMsg(this.gameInfo.player, result.player)
        this.gameState.update = (input) => {
            console.log("Game already ended!")
        }
    }


    /** @param {DomainDto_CharacterUpdateResult} result */
    onCharacterUpdate(result) {
        console.log(result)
        const character = this.characters.get(result.character.id)
        if (character) character.updateModel(result.character)
    }

    /** @param {DomainDto_CharacterDefeatResult} result */
    onCharacterDefeat(result) {
        console.log("defeat", JSON.stringify(result))
        const character = this.characters.get(result.character)
        if (!character) return
        character.dispose()
        this.characters.delete(result.character)
    }

    /** @param {string} input **/
    onInput(input) {
        if (!this.myTurn()) return
        this.gameState.update(input)
    }

    /** @param {Record} result **/
    onResult(result) {
        console.log("result", result)
        const name = simpleRecordName(result)
        if (name === "PlayerOverseeResult") {
            this.onPlayerOverseeResult(result)
        } else if (name === "PlayerMoveSelectionResult") {
            this.onPlayerMoveSelectionResult(result)
        } else if (name === "CharacterUpdateResult") {
            this.onCharacterUpdate(result)
        } else if (name === "PlayerActionSelectionResult") {
            this.onPlayerActionSelectionResult(result)
        } else if (name === "PlayerActionResult") {
            this.onPlayerActionResult(result)
        } else if (name === "CharacterDefeatResult") {
            this.onCharacterDefeat(result)
        } else if (name === "PlayerWinResult") {
            this.onPlayerWinResult(result)
        } else {
            console.error(`Unknown Result: ${name}`, result)
        }
    }

    async poll() {
        while (this.isPolling) {
            await new Promise(resolve => setTimeout(resolve, 175));
            /** @type {DomainDto_IResult} */
            const result = await pollServer(this.gameInfo)
            if (result) this.onResult(unwrap(result))
        }
    }

    myTurn() { return this.gameInfo.player === this.gameState.turnOf }

    /**
     * @param {DomainDto_PositionDto} pos
     * @return {Character}
     */
    findCharacter(pos) {
        return [...this.characters.values()].find(
            c => c.positionDto.Equals(pos)
        )
    }

    /**
     * @param {Array<DomainDto_PositionDto>} positions
     */
    highlightPositions(positions) {
        const highlightMaterial = new StandardMaterial("highlightmat", this.engineInfo.scene);
        highlightMaterial.diffuseColor = Color3.Purple();
        highlightMaterial.emissiveColor = new Color3(0.1, 0.1, 0.1);
        highlightMaterial.alpha = 0.7;
        this.highlightMeshes = []
        for (const [i, pos] of positions.entries()) {
            const mesh = MeshBuilder.CreateBox(`highlight${i}`, {depth: 1, width: 1, height: 0.1}, this.engineInfo.scene);
            mesh.renderingGroupId = 1
            mesh.alphaIndex = 1
            mesh.material = highlightMaterial
            mesh.position = positionDtoToVec3(pos)
            this.highlightMeshes.push(mesh)
        }
    }

    /**
     * @param {Array<string>} cids
     */
    highlightCharacters(cids) {
        const positions = []
        for (const cid of cids) {
            const character = this.characters.get(cid)
            if (!character) return
            positions.push(character.positionDto)
        }
        this.highlightPositions(positions)
    }


    removeHighlight() {
        if (!this.highlightMeshes) return
        for (const mesh of this.highlightMeshes) {
            mesh.dispose()
        }
    }

    /** @param {DomainDto_PositionDto} pos */
    showCharacterInfo(pos) {
        const c = this.findCharacter(pos)
        if (!c) return;
        // See: https://stackoverflow.com/a/16862775
        const canvas = document.getElementById("info-canvas")
        sendCharacterMsg(c.model)
        // canvas.textContent = JSON.stringify(c.model, undefined, 2)
    }

    /**
     * @param {DomainDto_PositionDto} pos
     * @param {DomainDto_ActionPreviewDto} preview
     */
    showActionOrCharacterInfo(pos, preview) {
        const characterBeforeAction = this.findCharacter(pos)
        if (!characterBeforeAction) return;
        if (preview && preview.results.has(characterBeforeAction.id)) {
            const actionName = preview.name
            const characterAfterAction = preview.results.get(characterBeforeAction.id)
            sendActionMsg(actionName, characterBeforeAction.model, characterAfterAction)
        } else {
            this.showCharacterInfo(pos)
        }
    }

    /**
     * @param {GameInfo} gameInfo
     * @param gameState
     * @param {Map<string, Character>}characters
     * @param {Array<Array<null|Character>>} board
     * @param {{engine: Engine, scene: Scene}} engineInfo
     */
    constructor(gameInfo, gameState, characters, board, engineInfo) {
        this.gameInfo = gameInfo
        this.gameState = gameState
        this.characters = characters
        this.board = board
        this.engineInfo = engineInfo
        const inputManager = new InputManager()
        inputManager.register(this.onInput.bind(this), this.engineInfo.scene)
        this.cursor = new Cursor(
            boardPosToVec3(0, 0),
            this.engineInfo.scene
        )
        this.selection = new Selector([])
        this.isPolling = true
        const _ = this.poll()
    }

    static async create() {
        /** @type {GameInfo} */
        const gameInfo = await init();
        /** @type {DomainDto_IResult} */
        let startInfoUnion = null;
        while (true) {
            startInfoUnion = await pollServer(gameInfo);
            if (startInfoUnion) break
            await new Promise(resolve => setTimeout(resolve, 1000));
            console.log("Waiting for other players")
        }
        /** @type {DomainDto_StartResult} */
        const startInfo = unwrap(startInfoUnion)
        if (startInfo.constructor.name !== "DomainDto_StartResult")
            throw "Invalid Start Result"

        const canvas = document.getElementById("map-canvas")
        const engine = new Engine(canvas)
        const scene = new Scene(engine)
        scene.clearColor = Color4.FromHexString("#3dc1d9")
        // scene.useOrderIndependentTransparency = true;
        const camera = new ArcRotateCamera("Camera", -Math.PI / 2, Math.PI / 4, 15, Vector3.Zero());
        // https://forum.babylonjs.com/t/how-can-i-increase-the-speed-at-which-the-camera-zooms-when-using-the-scroll-wheel/12281/5
        camera.inputs.attached.mousewheel.wheelPrecision = 10

        camera.attachControl(canvas, true);
        const light = new HemisphericLight("light", new Vector3(1, 1, 0), scene);

        console.log(startInfo.board)
        /** @type {{row: number, col: number}} **/
        const boardInfo = {
            row: startInfo.board.length,
            col: startInfo.board[0].length,
            tiles: startInfo.board
        }

        const tiledGround = MeshBuilder
            .CreateTiledGround("board", {
                xmin: -0.5, zmin: 0.5 - boardInfo.col,
                xmax: -0.5 + boardInfo.row, zmax: 0.5,
                subdivisions: {'h': boardInfo.col, 'w': boardInfo.row}
            })

        // Place camera at board center
        camera.target = new Vector3(-0.5 + boardInfo.row / 2, 0, 0.5 - boardInfo.col / 2)

        //Create the multi material
        //Create differents materials
        const landMaterial = new StandardMaterial("White");
        landMaterial.diffuseColor = Color4.FromHexString("#aac928");
        const waterMaterial = new StandardMaterial("Blue");
        waterMaterial.diffuseColor = Color4.FromHexString("#3dc1d9")
        const blackMaterial = new StandardMaterial("Black");
        blackMaterial.diffuseColor = Color4.FromHexString("#5c696b")

        // Create Multi Material
        const multimat = new MultiMaterial("multi", scene);
        multimat.subMaterials.push(landMaterial);
        multimat.subMaterials.push(waterMaterial);
        multimat.subMaterials.push(blackMaterial);


        // Apply the multi material
        // Define multimat as material of the tiled ground
        tiledGround.material = multimat;

        // Needed variables to set subMeshes
        const verticesCount = tiledGround.getTotalVertices();
        const tileIndicesLength = tiledGround.getIndices().length / (boardInfo.row * boardInfo.col);

        // Set subMeshes of the tiled ground
        tiledGround.subMeshes = [];
        let base = 0;
        // meshes are built from bottom to top, right to left
        // let tiles = boardInfo.tiles.map(cols => cols.reverse())
        const tiles = [...boardInfo.tiles].reverse()
        // mountain meshes
        const mountainMeshes = ["mountain1.glb", "mountain2.glb"]
        for (let col = boardInfo.col - 1; col >= 0; col--) {
            for (let row = boardInfo.row - 1; row >= 0; row--) {
                tiledGround.subMeshes.push(new SubMesh(tiles[row][col], 0, verticesCount, base, tileIndicesLength, tiledGround));
                base += tileIndicesLength;
                // Load "mountain" mesh
                if (boardInfo.tiles[row][col] !== 2) continue
                const mountainMesh = mountainMeshes[Math.floor(Math.random()*mountainMeshes.length)]
                SceneLoader.ImportMesh(
                    "", "./", mountainMesh, scene, (newMeshes) =>  {
                        const modelRootMesh = new AbstractMesh(`root-mountain-${row}-${col}`)
                        newMeshes.forEach(m => {
                            m.setParent(modelRootMesh)
                        })
                        modelRootMesh.scaling = new Vector3(1.35, 1.35, 1.35)
                        modelRootMesh.rotation.y = Tools.ToRadians(Math.random()*360)
                        modelRootMesh.position = boardPosToVec3(row, col)
                    })
            }
        }

        engine.runRenderLoop(() => {
            scene.render()
        })

        window.addEventListener("resize", function () {
            engine.resize()
        })

        let differentPlayer;
        if (gameInfo.player === Player.Player1) differentPlayer = Player.Player2
        else differentPlayer = Player.Player1



        const gameState = {
            turnOf: differentPlayer,
            /** @param {string} input */
            update: (input) => {}
        }

        // TODO: remove board
        const board =
            Array.from({length: boardInfo.row}, () => Array.from(
                {length: boardInfo.col}, () => null
            ))

        const characters = new Map()
        for (const c of startInfo.characters) {
            const newCharacter = new Character(c, scene)
            characters.set(c.id, newCharacter)
            let pos = newCharacter.positionDto
            board[pos.row][pos.col] = newCharacter
        }

        /** @type {{engine: Engine, scene: Scene}} **/
        const engineInfo = {
            engine: engine,
            scene: scene
        }

        sendStartedMsg()

        return new GameUI(gameInfo, gameState, characters, board, engineInfo)
    }

    start(startResult) {

        console.log("Baum");
        console.log(startResult);

        const char = startResult.characters[0].character
        console.log(char);
        const properties = char.properties
        console.log(properties)
        console.log(properties.get("1"))

        console.log(startResult.constructor.name);
    }
}

export {GameUI};