import { Union, Record } from "./output/fable_modules/fable-library.3.7.5/Types.js";
import {Vector3} from "@babylonjs/core";
import {DomainDto_PositionDto} from "./output/Shared/Shared";


/** @param {Union} union **/
export function unwrap(union) {
    return union["fields"][0]
}

/** @param {Record} record **/
export function simpleRecordName(record) {
    const fullName = record.constructor.name
    const moduleIndex = fullName.lastIndexOf("_")
    if (moduleIndex === -1) return fullName
    return fullName.substring(moduleIndex+1)
}

/**
 * @param {number} val
 * @param {number} minVal
 * @param {number} maxVal
 */
export function coerceIn(val, minVal, maxVal) {
    if (val < minVal) return minVal
    if (val > maxVal) return maxVal
    return val
}

export const Player = {
    Player1: 0,
    Player2: 1
}

export function boardPosToVec3(row, col) {
    return new Vector3(row, 0, -col)
}

/** @param {DomainDto_PositionDto} pos **/
export function positionDtoToVec3(pos) {
    return boardPosToVec3(pos.row, pos.col)
}

/** @param {Vector3} vec3 **/
export function vec3ToPositionDto(vec3) {
    return new DomainDto_PositionDto(vec3.x, -vec3.z)
}

// See: https://stackoverflow.com/a/2549333
export function eachRecursive(thisModel, model) {
    console.log(thisModel)
    console.log(model)
    for (let key in model) {
        if (typeof model[key] == "object" && model[key] !== null) {
            if (thisModel[key] === null)
                thisModel[key] = {}
            eachRecursive(thisModel[key], model[key])
        }
        else if (model[key]) {
            thisModel[key] = model[key]
        }
    }
}