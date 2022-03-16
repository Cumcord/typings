/// Expands the namespace tree into a list of every namespace that needs to be emitted
module defgen.Transform

open defgen.Util


/// Recursively flattens the namespace tree and replaces subnamespaces with references
let flatten rootNamespace =
    let rec flattenNamespaces (nsToFlat: FullNamespace) =
        nsToFlat.children
        |> List.choose (function
            | FNmspc subNs -> Some(nsToFlat :: (flattenNamespaces subNs))
            | FPrp _ -> None)
        |> List.concat

    let referencify (ns: FullNamespace) =
        let children =
            ns.children
            |> List.map (function
                | FPrp p -> CPrp p
                | FNmspc n -> CRef(ns.name + "/" + n.name))

        {name = ns.name; children = children}

    rootNamespace |> flattenNamespaces |> List.map referencify