import {
    AbstractMesh,
    Color3,
    Color4,
    MeshBuilder,
    SceneLoader,
    StandardMaterial,
    Tools,
    Vector3,
} from "@babylonjs/core";
import {
    eachRecursive,
    Player,
    positionDtoToVec3,
    setVec3FromPositionDto,
    vec3ToPositionDto,
} from "../Utils";
import "@babylonjs/loaders/glTF";

class Character {
    /**
     * @param {DomainDto_CharacterDto} model
     * @param {Scene} scene
     */
    constructor(model, scene) {
        this.model = model;
        const mesh = MeshBuilder.CreateBox(
            this.model.id,
            { depth: 1, width: 1, height: 0.01 },
            scene
        );
        mesh.position.y = 0;
        mesh.position.addInPlace(positionDtoToVec3(this.model.position));
        const cursorMaterial = new StandardMaterial(
            `mat${this.model.id}`,
            scene
        );
        if (this.model.player === Player.Player1)
            cursorMaterial.diffuseColor = Color4.FromHexString("#48c774");
        else cursorMaterial.diffuseColor = Color4.FromHexString("#f14668");
        cursorMaterial.emissiveColor = new Color3(0.1, 0.1, 0.1);
        cursorMaterial.alpha = 0.75;
        mesh.material = cursorMaterial;
        mesh.renderingGroupId = 0;
        mesh.alphaIndex = 0;
        this.mesh = mesh;

        const modelFile = queryModelFile(this.model.classification);
        SceneLoader.ImportMesh(
            "",
            "./",
            modelFile.filename,
            scene,
            (newMeshes) => {
                const modelRootMesh = new AbstractMesh(`root-${model.id}`);
                // scene.stopAnimation(newMeshes[0]);
                newMeshes.forEach((m) => {
                    // scene.stopAnimation(m)
                    // m.position = this.mesh.position
                    m.setParent(modelRootMesh);
                });

                modelFile.transform(modelRootMesh);
                if (this.model.player === Player.Player1) {
                    modelRootMesh.rotation.y += Tools.ToRadians(180);
                }

                // modelRootMesh.scaling = new Vector3(0.5, 0.5, 0.5)

                // Stop all mesh animations
                // See: https://github.com/BabylonJS/Babylon.js/issues/4514
                scene.animationGroups.forEach((group) => {
                    group.stop();
                    group.reset();
                });

                const meshPosWithoutHeight = this.mesh.position.clone();
                meshPosWithoutHeight.y = modelRootMesh.position.y;
                modelRootMesh.position = meshPosWithoutHeight;
                this.mesh.addChild(modelRootMesh);
            }
        );
    }

    /** @param {DomainDto_CharacterDto} model **/
    updateModel(model) {
        // console.log("model", JSON.stringify(model))
        // console.log("before update model", JSON.stringify(this.model))
        eachRecursive(this.model, model);
        // console.log("after update model", JSON.stringify(this.model))
        setVec3FromPositionDto(this.mesh.position, this.model.position);
    }

    get id() {
        return this.model.id;
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

class ModelFile {
    constructor(filename, transform) {
        this.filename = filename;
        this.transform = transform;
    }
}

function queryModelFile(cls) {
    console.log("model to file name", cls);
    // ["Axe", 0], ["Sword", 1], ["Lance", 2], ["Bow", 3], ["Support", 4
    let fileName = new ModelFile(
        "support.glb",
        (m) => (m.scaling = new Vector3(0.4, 0.4, 0.4))
    );
    if (cls === 0)
        fileName = new ModelFile("axe.glb", (m) => {
            m.scaling = new Vector3(0.5, 0.5, 0.5);
            m.position.addInPlace(new Vector3(0, 0.75, 0));
            m.rotation.y = Tools.ToRadians(-90);
        });
    else if (cls === 1)
        fileName = new ModelFile("sword.glb", (m) => {
            m.scaling = new Vector3(0.38, 0.38, 0.38);
            m.position.addInPlace(new Vector3(0, 0.95, 0));
            m.rotation.y = Tools.ToRadians(-90);
        });
    else if (cls === 2)
        fileName = new ModelFile("lance.glb", (m) => {
            m.scaling = new Vector3(0.0012, 0.0012, 0.0012);
            // m.position.addInPlace(new Vector3(0, 0.95, 0))
            // m.rotation.y = Tools.ToRadians(-90)
        });
    return fileName;
}

export { Character };
