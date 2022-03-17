/// Expands the namespace tree into a list of every namespace that needs to be emitted
module defgen.Transform

open defgen.Util


let inline private joinPath prefix next =
    if prefix = "" then next else prefix + "/" + next

/// Recursively flattens the namespace tree and replaces subnamespaces with references
let flatten rootNamespace =
    let rec flattenNamespaces path (nsToFlat: FullNamespace) =
        let subs =
            nsToFlat.children
            |> List.choose (function
                | FNmspc subNs -> Some(flattenNamespaces (joinPath path subNs.name) subNs)
                | FPrp _ -> None)
            |> List.concat
            //|> List.map (fun ns -> { ns with name = prefix + "/" + ns.name }: FullNamespace)
        
        { nsToFlat with name = path } ::subs

    // replaces the tree with a single layer of references to other (now flattened) namespaces
    let referencify (ns: FullNamespace) =
        let children =
            ns.children
            |> List.map (function
                | FPrp p -> CPrp p
                | FNmspc n -> CRef(ns.name + "/" + n.name, n.name))

        { name = ns.name; children = children }

    rootNamespace
    // first, find all namespaces in the tree and bring them to top level
    |> flattenNamespaces rootNamespace.name
    // now replace the now redundant subnamespaces with references to the top level ones
    |> List.map referencify