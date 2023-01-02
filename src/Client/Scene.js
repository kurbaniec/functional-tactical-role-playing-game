import {
    ArcRotateCamera, Color3,
    Engine,
    HemisphericLight, KeyboardEventTypes,
    MeshBuilder, MultiMaterial,
    Scene,
    StandardMaterial, SubMesh,
    Vector3
} from "@babylonjs/core";


function createScene() {
    console.log("creating scene...")
    const canvas = document.getElementById("map-canvas")
    // canvas.id = "gameCanvas"
    // document.body.appendChild(canvas)

    const engine = new Engine(canvas)
    const scene = new Scene(engine)

    const camera = new ArcRotateCamera("Camera", -Math.PI / 2, Math.PI / 4, 15, Vector3.Zero());

    camera.attachControl(canvas, true);
    const light = new HemisphericLight("light", new Vector3(1, 1, 0), scene);

    const grid = {
        'h': 8,
        'w': 8
    };

    const tiledGround = new MeshBuilder
        .CreateTiledGround("Tiled Ground", {
            xmin: -4, zmin: -4, xmax: 4, zmax: 4, subdivisions: grid
        });

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
    const tileIndicesLength = tiledGround.getIndices().length / (grid.w * grid.h);

    // Set subMeshes of the tiled ground
    tiledGround.subMeshes = [];
    let base = 0;
    for (let row = 0; row < grid.h; row++) {
        for (let col = 0; col < grid.w; col++) {
            tiledGround.subMeshes.push(new SubMesh(row%2 ^ col%2, 0, verticesCount, base , tileIndicesLength, tiledGround));
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

    // polling loop
    const polling = async () => {
        let condition = false;
        while (!condition) {
            await new Promise(resolve => setTimeout(resolve, 100));
            console.log("polling");
        }
    }
    const _ = polling();
}

function triggerAlert(message) { alert(message)}

export { createScene, triggerAlert }