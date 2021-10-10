//@flow

const { getStdin } = require("./stdin.js");
const { parseRawDef } = require("./rawDataParser.js");
const { createTypeDef } = require("./typeDefGenerator.js");


let defs = JSON.parse(getStdin(0).toString());

// parse our raw data into some more easily processable data
let parsed = parseRawDef(defs);

let typeDef = createTypeDef(parsed);
console.log(typeDef);
