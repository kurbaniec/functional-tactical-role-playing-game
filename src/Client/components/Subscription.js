import {ActionPreview, MsgModule_actionMsg, MsgModule_characterMsg, MsgModule_startedMsg, MsgModule_winMsg}
    from "../output/Index";

export function sendStartedMsg() {
    const e = new CustomEvent("uiSub", { detail: MsgModule_startedMsg() })
    window.dispatchEvent(e)
}

/**
 * @param {DomainDto_CharacterDto} character
 */
export function sendCharacterMsg(character) {
    const e = new CustomEvent("uiSub", { detail: MsgModule_characterMsg(character) })
    window.dispatchEvent(e)
}

/**
 * @param {string} actionName
 * @param {DomainDto_CharacterDto} before
 * @param {DomainDto_CharacterDto} after
 */
export function sendActionMsg(actionName, before, after) {
    const actionPreview = new ActionPreview(actionName, before, after)
    const e = new CustomEvent("uiSub", { detail: MsgModule_actionMsg(actionPreview) })
    window.dispatchEvent(e)
}

/**
 * @param {number} player
 * @param {number} winner
 */
export function sendWinMsg(player, winner) {
    const e = new CustomEvent("uiSub", { detail: MsgModule_winMsg(player, winner) })
    window.dispatchEvent(e)
}





