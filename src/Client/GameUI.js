// See: https://github.com/fable-compiler/fable3-samples/blob/main/interopFableFromJS/src/index.js
import { init, update, GameInfo } from "./output/Index.js"
import { DomainDto_IResult } from "./output/Shared/Shared";
import {unwrap} from "./Utils";
import {
    ArcRotateCamera, Color3,
    Engine,
    HemisphericLight, KeyboardEventTypes,
    MeshBuilder, MultiMaterial,
    Scene,
    StandardMaterial, SubMesh,
    Vector3
} from "@babylonjs/core";

class GameUI {
    constructor(gameInfo) {
        this.gameInfo = gameInfo
    }

    static async create() {
        /** @type {GameInfo} */
        const gameInfo = await init();
        /** @type {DomainDto_IResult} */
        const startInfoUnion = await update(gameInfo);
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
                xmin: -board.row/2, zmin: -board.col/2,
                xmax: board.row/2, zmax: board.col/2,
                subdivisions: {'h': board.col, 'w': board.row }
            })

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

        console.log(tiledGround.position)
        const cursor = MeshBuilder.CreateBox("cursor", { depth: 1, width: 1, height: 0.2 }, scene);
        cursor.position = new Vector3(0.5, 0.1, 0.5);
        const cursorMaterial = new StandardMaterial("cursormat", scene);
        cursorMaterial.diffuseColor = Color3.Red();
        // cursorMaterial.emissiveColor = new Color3(0.1, 0.1, 0.1);
        cursorMaterial.alpha = 0.7;
        cursor.material = cursorMaterial;

        function moveCursor(x, z) {
            cursor.position.addInPlace(new Vector3(x, 0, z));
        }

        scene.onKeyboardObservable.add((kbInfo) => {
            if (kbInfo.type === KeyboardEventTypes.KEYDOWN) return;
            // console.log(kbInfo.event);
            switch (kbInfo.event.keyCode) {
                case 87: // W
                    console.log("Pressed W");
                    moveCursor(0, 1);
                    break;
                case 83: // S
                    console.log("Pressed S");
                    moveCursor(0, -1);
                    break;
                case 65: // A
                    console.log("Pressed A");
                    moveCursor(-1, 0);
                    break;
                case 68: // D
                    console.log("Pressed D");
                    moveCursor(1, 0);
                    break;
                case 32: // Space
                    console.log("Pressed Space");
                    break;
            }
        });

        engine.runRenderLoop(() => {
            scene.render()
        })

        window.addEventListener("resize", function () {
            engine.resize()
        })

        const polling = async () => {
            let condition = false;
            while (!condition) {
                await new Promise(resolve => setTimeout(resolve, 100));
                // console.log("polling");
            }
        }
        const _ = polling();

        return new GameUI(gameInfo)
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