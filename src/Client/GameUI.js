
class GameUI {

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