/// Emits .d.ts files from a data structure
module defgen.Emitter

open defgen.Util

let private indent (str: string) =
    str.Split "\n"
    |> Array.map (fun s -> "  " + s)
    |> String.concat "\n"

let private steriliseImport (import: string) =
    import
        .Replace('@', '_')
        .Replace('-', '_')
        .Replace('.', '_')
        .Replace('/', '_')

let propTemplate kind def = $"export %s{kind} %s{def};"

let refTemplate ref =
    $"""export * as %s{snd ref} from "%s{fst ref}";"""

let moduleTemplate name children =
    $"""declare module "%s{name}" {{
%s{indent children}
}}"""

let importTemplate import =
    $"""import * as %s{steriliseImport import} from "%s{import}";"""

let emitProp (prop: Prop) =
    propTemplate (string prop.kind) prop.typedef

let emitModule (nmsp: ContractedNamespace) =
    moduleTemplate
        nmsp.name
        (nmsp.children
         |> List.map (function
             | CRef ref -> refTemplate ref
             | CPrp prop -> emitProp prop)
         |> String.concat "\n")

let emitAllModules =
    List.map emitModule >> String.concat "\n\n"

let emitImports =
    List.map importTemplate >> String.concat "\n"

let emitFull imports modules =
    let emittedImports =
        match imports with
        | Some i -> emitImports i
        | None -> ""

    (emittedImports + "\n\n" + emitAllModules modules)
        .Trim()