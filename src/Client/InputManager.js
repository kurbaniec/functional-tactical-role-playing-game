import {KeyboardEventTypes} from "@babylonjs/core";

export const Input = {
    Up: 'Up',
    Down: 'Down',
    Left: 'Left',
    Right: 'Right',
    Enter: 'Enter',
    Escape: 'Escape'
}

export class InputManager {
    /**
     * @callback onInputCallback
     * @param {string} input
     */

    /**
     * @param {onInputCallback} callback
     * @param {Scene} scene
     */
    register(callback, scene) {
        scene.onKeyboardObservable.add((kbInfo) => {
            if (kbInfo.type === KeyboardEventTypes.KEYDOWN) return;
            // console.log(kbInfo.event);
            switch (kbInfo.event.keyCode) {
                case 87: // W
                    callback(Input.Up)
                    break;
                case 83: // S
                    callback(Input.Down)
                    break;
                case 65: // A
                    callback(Input.Left)
                    break;
                case 68: // D
                    callback(Input.Right)
                    break;
                case 32: // Space
                    callback(Input.Enter)
                    break;
                case 27: // Escape
                    callback(Input.Escape)
                    break;
            }
        })
    }


}