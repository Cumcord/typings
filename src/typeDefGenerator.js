//@flow

const createTypeDef = function (elems: definitionElement[]): string {
    return `declare module "@cumcord" {
${elems.map((e) => elementToString(e, 1)).join("\n")}
}
${elems.map(getAllModules).flat().map(extraModuleToString).join("\n")}`;
};

const getAllModules = function (elem: definitionElement): submodule[] {
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
};

const elementToString = function (
    elem: definitionElement,
    indent: number
): string {
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
};

const extraModuleToString = function (elem: submodule, indent: number): string {
    let i = makeIndent(indent);
    return `
${i}declare module "@cumcord/${elem[0]}" {
${elem[1].map((e) => elementToString(e, indent + 1)).join("\n")}
${i}}`;
};

const makeIndent = function (amount: number): string {
    return Array(amount).fill("  ").join("");
};

module.exports = {
    createTypeDef,
};
