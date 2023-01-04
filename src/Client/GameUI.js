// See: https://github.com/fable-compiler/fable3-samples/blob/main/interopFableFromJS/src/index.js
import {init, pollServer, GameInfo, updateServer} from "./output/Index.js"
import {DomainDto_CharacterDto, DomainDto_IMessage, DomainDto_IResult} from "./output/Shared/Shared";
import {boardPosToVec3, coerceIn, positionDtoToVec3, simpleRecordName, unwrap} from "./Utils";
import {
    ArcRotateCamera, Color3,
    Engine,
    HemisphericLight, KeyboardEventTypes,
    MeshBuilder, MultiMaterial,
    Scene,
    StandardMaterial, SubMesh,
    Vector3
} from "@babylonjs/core";
import {Input, InputManager} from "./InputManager"


class Cursor {


    constructor(start, scene) {
        const mesh = MeshBuilder.CreateBox("cursor", {depth: 1, width: 1, height: 0.2}, scene);
        mesh.position = start
        const cursorMaterial = new StandardMaterial("cursormat", scene);
        cursorMaterial.diffuseColor = Color3.Red();
        // cursorMaterial.emissiveColor = new Color3(0.1, 0.1, 0.1);
        cursorMaterial.alpha = 0.7;
        mesh.material = cursorMaterial;
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
}

class Character {

    /**
     * @param {DomainDto_CharacterDto} model
     * @param {Scene} scene
     */
    constructor(model, scene) {
        this.model = model
        const mesh = MeshBuilder.CreateBox(this.model.id, {depth: 0.5, width: 0.5, height: 0.8}, scene);
        mesh.position.y = 1.2/2
        mesh.position.addInPlace(positionDtoToVec3(this.model.position))
        const cursorMaterial = new StandardMaterial(`mat${this.model.id}`, scene);
        // if (this.model.)
        // TODO get player info
        cursorMaterial.diffuseColor = Color3.Blue();
        // cursorMaterial.emissiveColor = new Color3(0.1, 0.1, 0.1);
        cursorMaterial.alpha = 0.7;
        mesh.material = cursorMaterial;
        this.mesh = mesh
        console.log("char", this.mesh.position)
    }
}

class GameUI {

    /** @param {string} input **/
    onInput(input) {
        this.cursor.moveCursor(input)

        // Only for debugging
        const msg = new DomainDto_IMessage(
            0, [GameUI.cid]
        )
        console.log("msg", msg)
        updateServer(msg, this.gameInfo)
    }

    /** @param {Record} result **/
    onResult(result) {
        console.log("result", result)
        const name = simpleRecordName(result)
        if (name === "PlayerOverseeResult") {

        } else {
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


    constructor(gameInfo, engineInfo) {
        this.gameInfo = gameInfo
        this.engineInfo = engineInfo
        const inputManager = new InputManager()
        inputManager.register(this.onInput.bind(this), this.engineInfo.scene)
        this.cursor = new Cursor(
            boardPosToVec3(0, 0),
            this.engineInfo.scene
        )
        this.isPolling = true
        const _ = this.poll()
    }

    static cid = ""

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
        const camera = new ArcRotateCamera("Camera", -Math.PI / 2, Math.PI / 4, 15, Vector3.Zero());

        camera.attachControl(canvas, true);
        const light = new HemisphericLight("light", new Vector3(1, 1, 0), scene);

        console.log(startInfo.board)
        /** @type {{row: number, col: number}} **/
        const board = {
            row: startInfo.board.length,
            col: startInfo.board[0].length,
            tiles: startInfo.board
        }

        const tiledGround = MeshBuilder
            .CreateTiledGround("board", {
                xmin: -0.5, zmin: 0.5 - board.col,
                xmax: -0.5 + board.row, zmax: 0.5,
                subdivisions: {'h': board.col, 'w': board.row }
            })

        // Place camera at board center
        camera.target = new Vector3(-0.5 + board.row/2, 0, 0.5 -board.col/2)

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
        const tileIndicesLength = tiledGround.getIndices().length / (board.row * board.col);

        // Set subMeshes of the tiled ground
        tiledGround.subMeshes = [];
        let base = 0;

        // meshes are built from bottom to top, right to left
        for (let col = board.col-1; col >= 0; col--) {
            for (let row = board.row-1; row >= 0; row--) {
                tiledGround.subMeshes.push(new SubMesh(board.tiles[row][col], 0, verticesCount, base , tileIndicesLength, tiledGround));
                base += tileIndicesLength;
            }
        }

        engine.runRenderLoop(() => {
            scene.render()
        })

        window.addEventListener("resize", function () {
            engine.resize()
        })

        /** @type {{engine: Engine, scene: Scene}} **/
        const engineInfo = {
            engine: engine,
            scene: scene
        }

        for (const c of startInfo.characters) {
            console.log("character", c)
            this.cid = c.id
            new Character(c, scene)
        }

        return new GameUI(gameInfo, engineInfo)
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

export { GameUI };