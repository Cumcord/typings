//@flow

var { getStdin } = require("./stdin.js");

function main() {
    let defs = JSON.parse(getStdin(0).toString());

    // parse our raw data into some more easily processable data
    let parsed = parseRawDef(defs);

    let typeDef = createTypeDef(parsed);
    console.log(typeDef);
}

// easily get members of an object :)
function members(obj: any): [string, any][] {
    return Object.keys(obj).map((key) => [key, obj[key]]);
}

// define some types that describe the input data, so we can take full advantage of static typing
type namespaceMember = [string, "function" | "const"];
type namespace = [string, namespaceMember[]];
type module = [string, namespace[]];
type definitionElement = module | namespace | namespaceMember;

function parseRawDef(obj: any): definitionElement[] {
    return members(obj).map((m) => parseElement(m[0], m[1]));
}

function parseElement(key: string, value: any): definitionElement {
    let member = tryParseMember(value);
    if (member != null) return member;

    let namespace = tryParseNamespace(key, value);
    if (namespace != null) return namespace;

    // the only other possible situation is a module
    let module = tryParseModule(key, value);
    if (module != null) return module;

    throw Error("invalid input data!");
}

function tryParseMember(value: any): ?namespaceMember {
    // array of 2 strings, first value must be function or const
    if (
        Array.isArray(value) &&
        value.length === 2 &&
        (value[0] === "function" || value[0] === "const")
    ) {
        return [value[1], value[0]];
    }
}

function tryParseNamespace(name: string, value: any): ?namespace {
    // array of arrays of strings
    if (
        Array.isArray(value) &&
        Array.isArray(value[0]) &&
        typeof value[0][0] === "string"
    ) {
        let nsMembers = members(value).map((v) => tryParseMember(v[1]));
        if (nsMembers.every((v) => v != null)) {
            // $FlowIssue[incompatible-return] we have already verified the whole array to be not null
            return [name, nsMembers];
        }
    }
}

function tryParseModule(name: string, obj: any): ?module {
    let objMembers = members(obj);
    // modules must contain only namespaces
    let namespaces = objMembers.map((m) => tryParseNamespace(m[0], m[1]));
    if (namespaces.every((v) => v != null)) {
        // no invalid (null) members
        // $FlowIssue[incompatible-return] see above
        return [name, namespaces];
    }
}

function createTypeDef(elems: definitionElement[]): string {
    return `declare module "@cumcord" {
${elems.map((e) => elementToString(e, 1)).join("\n")}
}
${elems.map(getAllModules).flat().map(extraModuleToString).join("\n")}`;
}

function getAllModules(elem: definitionElement): module[] {
    // members have no children, so hitting a member is one recursion base case
    if (typeof elem[1] === "string") return [];

    // the other base case is that we have hit a module!
    // to differentiate against a namespace, make sure the child is not a member
    if (typeof elem[1][1] !== "string") {
        // $FlowIssue[incompatible-return] we already know this *must* be a module (see above comment)
        return [elem];
    }

    // otherwise recursively get modules of children and flatten
    return elem[1].map((e) => getAllModules(e)).flat();
}

function elementToString(elem: definitionElement, indent: number): string {
    // member signature or namespace/module name
    let signature = elem[0];
    // member type or namespace/module children
    let extra = elem[1];

    let i = makeIndent(indent);

    if (typeof extra === "string") {
        // we must be a member
        return `${i}export ${extra} ${signature};`;
    }

    // must now be either module or namespace - both of which are treated the same
    return `${i}namespace ${signature} {
${extra.map((e) => elementToString(e, indent + 1)).join("\n")}
${i}}`;
}

function extraModuleToString(elem: module, indent: number): string {
    let i = makeIndent(indent);
    return `
${i}declare module "@cumcord/${elem[0]}" {
${elem[1].map((e) => elementToString(e, indent + 1)).join("\n")}
${i}}`;
}

function makeIndent(amount: number): string {
    return Array(amount).fill("  ").join("");
}

main();
