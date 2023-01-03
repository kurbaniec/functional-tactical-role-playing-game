import { Union } from "./output/fable_modules/fable-library.3.7.5/Types.js";


/** @param {Union} union **/
export function unwrap(union) {
    return union["fields"][0]
}