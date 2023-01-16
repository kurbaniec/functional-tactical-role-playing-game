import {Color3, MeshBuilder, SceneLoader, StandardMaterial, TransformNode, Vector3} from "@babylonjs/core";
import {eachRecursive, Player, positionDtoToVec3, setVec3FromPositionDto, vec3ToPositionDto} from "../Utils";
import "@babylonjs/loaders/glTF"

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

        SceneLoader.ImportMesh(
            "", "./", "support.glb", scene, (newMeshes) =>  {
            const importRoot = new TransformNode("")
            // scene.stopAnimation(newMeshes[0]);
            newMeshes.forEach(m => {
                // scene.stopAnimation(m)
                m.setParent(importRoot)
            })

            importRoot.scaling = new Vector3(0.5, 0.5, 0.5)

            // Stop all mesh animations
            // See: https://github.com/BabylonJS/Babylon.js/issues/4514
            scene.animationGroups.forEach(group => {
                group.stop()
                group.reset()
            })
        })

    }

    /** @param {DomainDto_CharacterDto} model **/
    updateModel(model) {
        // console.log("model", JSON.stringify(model))
        // console.log("before update model", JSON.stringify(this.model))
        eachRecursive(this.model, model)
        // console.log("after update model", JSON.stringify(this.model))
        setVec3FromPositionDto(this.mesh.position, this.model.position)
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

    dispose() { this.mesh.dispose() }
}

export { Character }