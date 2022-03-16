/// Expands the namespace tree into a list of every namespace that needs to be emitted
module defgen.Transform

open defgen.Util

/// Does the madness required for cumcord's funky import syntax
let expand (nmsp: Namespace) =
    let rec expandRec nmsp name =
        let subNamespaces =
            nmsp.children
            |> List.choose (function
                | Nmspc ns -> Some(expandRec ns (name + "/" + ns.name))
                | Prp _ -> None)
            |> List.concat
        
        let renamedNmsp = { nmsp with name = name }
        
        if subNamespaces.Length = 0 then
            [renamedNmsp]
        else
            renamedNmsp :: subNamespaces
    
    expandRec nmsp nmsp.name