import { Union } from "./output/fable_modules/fable-library.3.7.5/Types.js";


/** @param {Union} union **/
export function unwrap(union) {
    return union["fields"][0]
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