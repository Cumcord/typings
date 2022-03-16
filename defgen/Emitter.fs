/// Emits .d.ts files from a data structure
module defgen.Emitter

open defgen.Util

let private indent (str: string) =
    str.Split "\n"
    |> Array.map (fun s -> "  " + s)
    |> String.concat "\n"

let propTemplate kind def = $"export %s{kind} %s{def};"

let namespaceTemplate name children =
    $"""namespace %s{name} {{
%s{indent children}
}}"""

let moduleTemplate name children =
    $"""declare module "%s{name}" {{
%s{indent children}
}}"""

let emitProp (prop: Prop) =
    propTemplate (string prop.kind) prop.typedef

let rec private emitNamespaceOrModule isMod (nmsp: ContractedNamespace) =
    let template = if isMod then moduleTemplate else namespaceTemplate
    
    template
        nmsp.name
        (nmsp.children
         |> List.map (function
             | FNmspc ns -> emitNamespaceOrModule false ns
             | FPrp prop -> emitProp prop)
         |> String.concat "\n")

let emitNamespace = emitNamespaceOrModule false
let emitModule = emitNamespaceOrModule true

let emitAllModules = List.map emitModule >> String.concat "\n\n"