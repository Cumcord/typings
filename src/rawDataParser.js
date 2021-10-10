//@flow

const membersOf = function (obj: any): [string, any][] {
    return Object.keys(obj).map((key) => [key, obj[key]]);
};

const parseRawDef = function (obj: any): definitionElement[] {
    return membersOf(obj).map((m) => parseElement(m[0], m[1]));
};

const parseElement = function (key: string, value: any): definitionElement {
    let member = tryParseMember(value);
    if (member != null) return member;

    let namespace = tryParseNamespace(key, value);
    if (namespace != null) return namespace;

    // the only other possible situation is a module
    let module = tryParseModule(key, value);
    if (module != null) return module;

    throw Error("invalid input data!");
};

const tryParseMember = function (value: any): ?namespaceMember {
    // array of 2 strings, first value must be function or const
    if (
        Array.isArray(value) &&
        value.length === 2 &&
        (value[0] === "function" || value[0] === "const")
    ) {
        return [value[1], value[0]];
    }
};

const tryParseNamespace = function (name: string, value: any): ?namespace {
    // array of arrays of strings
    if (
        Array.isArray(value) &&
        Array.isArray(value[0]) &&
        typeof value[0][0] === "string"
    ) {
        let nsMembers = membersOf(value).map((v) => tryParseMember(v[1]));
        if (nsMembers.every((v) => v != null)) {
            // $FlowIssue[incompatible-return] we have already verified the whole array to be not null
            return [name, nsMembers];
        }
    }
};

const tryParseModule = function (name: string, obj: any): ?submodule {
    let objMembers = membersOf(obj);
    // modules must contain only namespaces
    let namespaces = objMembers.map((m) => tryParseNamespace(m[0], m[1]));
    if (namespaces.every((v) => v != null)) {
        // no invalid (null) members
        // $FlowIssue[incompatible-return] see above
        return [name, namespaces];
    }
};

module.exports = {
    parseRawDef,
};
