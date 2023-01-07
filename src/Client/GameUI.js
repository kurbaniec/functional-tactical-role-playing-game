// See: https://github.com/fable-compiler/fable3-samples/blob/main/interopFableFromJS/src/index.js
import {GameInfo, init, pollServer, updateServer} from "./output/Index.js"
import {DomainDto_CharacterDto, DomainDto_IMessage, DomainDto_IResult} from "./output/Shared/Shared";
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
    ArcRotateCamera,
    Color3,
    Engine,
    HemisphericLight,
    MeshBuilder,
    MultiMaterial,
    Scene,
    StandardMaterial,
    SubMesh,
    Vector3
} from "@babylonjs/core";
import {Input, InputManager} from "./InputManager"
import {AdvancedDynamicTexture, InputText, StackPanel, TextBlock} from "@babylonjs/gui";


class Cursor {


    constructor(start, scene) {
        const mesh = MeshBuilder.CreateBox("cursor", {depth: 1, width: 1, height: 0.2}, scene);
        mesh.position = start
        const cursorMaterial = new StandardMaterial("cursormat", scene);
        cursorMaterial.diffuseColor = Color3.Red();
        // cursorMaterial.emissiveColor = new Color3(0.1, 0.1, 0.1);
        cursorMaterial.alpha = 0.7;
        mesh.material = cursorMaterial;
        // cursorMaterial.needDepthPrePass = false
        // Render later than other transparent meshes
        // See: https://doc.babylonjs.com/features/featuresDeepDive/materials/advanced/transparent_rendering
        mesh.renderingGroupId = 1
        mesh.alphaIndex = 1
        this.mesh = mesh
    }

    /** @param {string} input **/
    moveCursor(input) {
        if (input === Input.Up)
            this.mesh.position.addInPlace(new Vector3(0, 0, 1))
        else if (input === Input.Down)
            this.mesh.position.addInPlace(new Vector3(0, 0, -1))
        else if (input === Input.Left)
            this.mesh.position.addInPlace(new Vector3(-1, 0, 0))
        else if (input === Input.Right)
            this.mesh.position.addInPlace(new Vector3(1, 0, 0))
    }

    get position() {
        return this.mesh.position
    }

    get positionDto() {
        return vec3ToPositionDto(this.mesh.position)
    }
}

class Character {

    /**
     * @param {DomainDto_CharacterDto} model
     * @param {Scene} scene
     */
    constructor(model, scene) {
        this.model = model
        const mesh = MeshBuilder.CreateBox(this.model.id, {depth: 0.5, width: 0.5, height: 0.8}, scene);
        mesh.position.y = 1.2 / 2
        mesh.position.addInPlace(positionDtoToVec3(this.model.position))
        const cursorMaterial = new StandardMaterial(`mat${this.model.id}`, scene);
        if (this.model.player === Player.Player1)
            cursorMaterial.diffuseColor = Color3.Blue();
        else
            cursorMaterial.diffuseColor = Color3.Green();
        cursorMaterial.emissiveColor = new Color3(0.1, 0.1, 0.1);
        cursorMaterial.alpha = 0.75;
        mesh.material = cursorMaterial;
        mesh.renderingGroupId = 0
        mesh.alphaIndex = 0
        this.mesh = mesh
    }

    /** @param {DomainDto_CharacterDto} model **/
    updateModel(model) {
        console.log("before update model", this.model)
        eachRecursive(this.model, model)
        setVec3FromPositionDto(this.mesh.position, this.model.position)
        console.log("after update model", this.model)
    }

    get id() {
        return this.model.id
    }

    get position() {
        return this.mesh.position
    }

    get positionDto() {
        return vec3ToPositionDto(this.mesh.position)
    }
}

class Selector {

    /**
     * @param {Array<string>} selections
     */
    constructor(selections) {
        this.mesh = AdvancedDynamicTexture.CreateFullscreenUI("UI")
        this.setSelections(selections)
    }

    /** @param {Array<string>} selections */
    setSelections(selections) {
        if (!selections || selections.length === 0) return
        const panel = new StackPanel()
        selections.forEach(s => {
            // TODO: better way?
            const ui = new InputText()
            ui.width = "150px";
            ui.maxWidth = 0.2;
            ui.height = "40px";
            ui.color = "white";
            ui.background = "black";
            ui.text = s
            panel.addControl(ui)
        })
        this.panel = panel
        this.mesh.addControl(this.panel)
        this.length = panel.children.length
        this.index = this.length-1
        this.moveCursor(Input.Down)
    }

    clear() {
        this.mesh.removeControl(this.panel)
    }

    /** @param {string} input **/
    moveCursor(input) {
        const lastIndex = this.index
        if (input === Input.Up) {
            this.index = modulo((lastIndex + 1), this.length)
        }
        else if (input === Input.Down) {
            this.index = modulo((lastIndex - 1), this.length)
        }
        console.log("index", lastIndex, this.index)
        this.removeHighlight(lastIndex)
        this.highlight(this.index)
    }

    currentSelection() {
        return this.panel.children[this.index].text
    }

    highlight(index) {
        const ui = this.panel.children[index]
        ui.background = "green";
    }

    removeHighlight(index) {
        console.log("pp", this.panel.children)
        const ui = this.panel.children[index]
        console.log("pp", ui)
        ui.background = "black";
    }



}

class GameUI {

    /** @param {DomainDto_PlayerOverseeResult} result **/
    onPlayerOverseeResult(result) {
        this.gameState.turnOf = result.player
        if (!this.myTurn()) return;
        this.removeHighlight();
        this.gameState.update = (input) => {
            if (input === Input.Enter) {
                const c = this.findCharacter(this.cursor.positionDto)
                console.log("oversee-c", c)
                if (!c) return;
                updateServer(new DomainDto_IMessage(
                0, [c.id]
                ), this.gameInfo)

            } else {
                this.cursor.moveCursor(input)
                this.showCharacterInfo(this.cursor.positionDto)
            }
        }
    }

    /** @param {DomainDto_PlayerMoveSelectionResult} result **/
    onPlayerMoveSelectionResult(result) {
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
                this.showCharacterInfo(this.cursor.positionDto)
            }
        }
    }

    /** @param {DomainDto_PlayerActionSelectionResult} result **/
    onPlayerActionSelectionResult(result) {
        this.removeHighlight()
        this.selection.setSelections(result.availableActions)
        this.gameState.update = (input) => {
            if (input === Input.Enter) {
                console.log("sel", this.selection.currentSelection())
                // const pos = this.cursor.positionDto
                // updateServer(new DomainDto_IMessage(
                //     2, pos
                // ), this.gameInfo)
            } else {
                this.selection.moveCursor(input)
                // this.showCharacterInfo(this.cursor.positionDto)
            }
        }
    }



    /** @param {DomainDto_CharacterUpdateResult} result */
    onCharacterUpdate(result) {
        console.log(result)
        const c = this.characters.get(result.character.id)
        console.log("c", c)
        if (c) c.updateModel(result.character)
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
        }
        else {
            console.error(`Unknown Result: ${name}`, result)
        }
    }

    async poll() {
        while (this.isPolling) {
            await new Promise(resolve => setTimeout(resolve, 100));
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
        highlightMaterial.diffuseColor = Color3.Teal();
        highlightMaterial.emissiveColor = new Color3(0.1, 0.1, 0.1);
        highlightMaterial.alpha = 0.3;
        this.highlightMeshes = []
        for (const [i, pos] of positions.entries()) {
            const mesh = MeshBuilder.CreateBox(`highlight${i}`, {depth: 1, width: 1, height: 0.1}, this.engineInfo.scene);
            mesh.material = highlightMaterial
            mesh.position = positionDtoToVec3(pos)
            this.highlightMeshes.push(mesh)
        }
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
        canvas.textContent = JSON.stringify(c.model, undefined, 2)
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
        const startInfoUnion = await pollServer(gameInfo);
        /** @type {DomainDto_StartResult} */
        const startInfo = unwrap(startInfoUnion)
        if (startInfo.constructor.name !== "DomainDto_StartResult")
            throw "Invalid Start Result"

        const canvas = document.getElementById("map-canvas")
        const engine = new Engine(canvas)
        const scene = new Scene(engine)
        // scene.useOrderIndependentTransparency = true;
        const camera = new ArcRotateCamera("Camera", -Math.PI / 2, Math.PI / 4, 15, Vector3.Zero());

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
        const whiteMaterial = new StandardMaterial("White");
        whiteMaterial.diffuseColor = new Color3(1, 1, 1);
        const blackMaterial = new StandardMaterial("Black");
        blackMaterial.diffuseColor = new Color3(0, 0, 0);

        // Create Multi Material
        const multimat = new MultiMaterial("multi", scene);
        multimat.subMaterials.push(whiteMaterial);
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
        for (let col = boardInfo.col - 1; col >= 0; col--) {
            for (let row = boardInfo.row - 1; row >= 0; row--) {
                tiledGround.subMeshes.push(new SubMesh(boardInfo.tiles[row][col], 0, verticesCount, base, tileIndicesLength, tiledGround));
                base += tileIndicesLength;
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