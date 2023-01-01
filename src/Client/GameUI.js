
class GameUI {

    start(startResult) {
        console.log("Baum");
        console.log(startResult);

        const char = startResult.characters.head.character
        console.log(char);
        const properties = char.properties
        console.log(properties)
        console.log(properties.get("1"))
    }
}

export { GameUI };