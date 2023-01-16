import {
    Color3,
    MeshBuilder,
    StandardMaterial,
    Vector3,
} from "@babylonjs/core";
import { vec3ToPositionDto } from "../Utils";
import { Input } from "./InputManager";

class Cursor {
    constructor(start, scene) {
        const mesh = MeshBuilder.CreateBox(
            "cursor",
            { depth: 1, width: 1, height: 0.2 },
            scene
        );
        mesh.position = start;
        const cursorMaterial = new StandardMaterial("cursormat", scene);
        cursorMaterial.diffuseColor = Color3.Red();
        // cursorMaterial.emissiveColor = new Color3(0.1, 0.1, 0.1);
        cursorMaterial.alpha = 0.7;
        mesh.material = cursorMaterial;
        // cursorMaterial.needDepthPrePass = false
        // Render later than other transparent meshes
        // See: https://doc.babylonjs.com/features/featuresDeepDive/materials/advanced/transparent_rendering
        mesh.renderingGroupId = 3;
        mesh.alphaIndex = 3;
        this.mesh = mesh;
    }

    /** @param {string} input **/
    moveCursor(input) {
        if (input === Input.Up)
            this.mesh.position.addInPlace(new Vector3(0, 0, 1));
        else if (input === Input.Down)
            this.mesh.position.addInPlace(new Vector3(0, 0, -1));
        else if (input === Input.Left)
            this.mesh.position.addInPlace(new Vector3(-1, 0, 0));
        else if (input === Input.Right)
            this.mesh.position.addInPlace(new Vector3(1, 0, 0));
    }

    hide() {
        this.mesh.isVisible = false;
    }

    show() {
        this.mesh.isVisible = true;
    }

    get position() {
        return this.mesh.position;
    }

    get positionDto() {
        return vec3ToPositionDto(this.mesh.position);
    }

    dispose() {
        this.mesh.dispose();
    }
}

export { Cursor };
