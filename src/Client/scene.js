import {Engine, Scene} from "@babylonjs/core";


function createScene() {
    console.log("creating scene...")
    const canvas = document.createElement("canvas")
    canvas.id = "gameCanvas"
    document.body.appendChild(canvas)

    const engine = new Engine(canvas)
    const scene = new Scene(engine)

    engine.runRenderLoop(() => {
        scene.render()
    })
}

function triggerAlert(message) { alert(message)}

export { createScene, triggerAlert }