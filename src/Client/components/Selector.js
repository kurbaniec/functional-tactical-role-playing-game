import { AdvancedDynamicTexture, InputText, StackPanel } from "@babylonjs/gui";
import { modulo } from "../Utils";
import { Input } from "./InputManager";

class Selector {
    /**
     * @param {Array<string>} selections
     */
    constructor(selections) {
        this.mesh = AdvancedDynamicTexture.CreateFullscreenUI("UI");
        this.setSelections(selections);
    }

    /** @param {Array<string>} selections */
    setSelections(selections) {
        if (!selections || selections.length === 0) return;
        const panel = new StackPanel();
        selections.forEach((s) => {
            const ui = new InputText();
            ui.width = "150px";
            ui.maxWidth = 0.2;
            ui.height = "40px";
            ui.color = "white";
            ui.background = "black";
            ui.text = s;
            panel.addControl(ui);
        });
        this.panel = panel;
        this.mesh.addControl(this.panel);
        this.length = panel.children.length;
        this.index = this.length - 1;
        this.moveCursor(Input.Down);
    }

    clear() {
        if (this.panel) this.mesh.removeControl(this.panel);
    }

    /** @param {string} input **/
    moveCursor(input) {
        const lastIndex = this.index;
        if (input === Input.Up) {
            this.index = modulo(lastIndex - 1, this.length);
        } else if (input === Input.Down) {
            this.index = modulo(lastIndex + 1, this.length);
        }
        this.removeHighlight(lastIndex);
        this.highlight(this.index);
    }

    currentSelection() {
        return this.panel.children[this.index].text;
    }

    highlight(index) {
        const ui = this.panel.children[index];
        ui.background = "green";
    }

    removeHighlight(index) {
        const ui = this.panel.children[index];
        ui.background = "black";
    }
}

export { Selector };
