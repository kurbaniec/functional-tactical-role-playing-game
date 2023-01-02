// See: https://github.com/fable-compiler/fable3-samples/blob/main/interopFableFromJS/src/index.js
import { init, update } from "./output/Index.js"

class GameUI {
    constructor(gameInfo) {
        this.gameInfo = gameInfo
    }

    static async create() {
        const gameInfo = await init();
        const startInfo = await update(gameInfo);


        console.log(gameInfo)
        console.log(gameInfo.id)
        console.log(startInfo)

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