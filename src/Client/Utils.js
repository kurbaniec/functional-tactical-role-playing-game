import { Union, Record } from "./output/fable_modules/fable-library.3.7.5/Types.js";


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