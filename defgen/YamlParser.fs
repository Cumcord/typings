/// Responsible for parsing YAML into a nice data structure
module defgen.YamlParser

open FSharp.Data
open defgen.Util

let private (|PropType|_|) =
    function
    | YamlValue.String (PropertyType t) -> Some t
    | _ -> None

let private (|Property|_|) =
    function
    // active patterns and just pattern matching in general :SanOhYes:
    | YamlValue.Sequence [|PropType t; YamlValue.String def|] -> Some {kind = t; typedef = def}
    | _ -> None

let rec private (|NamespaceChildren|_|) =
    function
    | YamlValue.Sequence rawChildren ->
        let children =
            rawChildren
            |> Seq.toList
            |> List.choose (function
                | Property p -> Some(FPrp p)
                | Namespace n -> Some(FNmspc n)
                | _ -> None)

        if children.Length = rawChildren.Length then
            Some children
        else
            None
    | _ -> None

and private (|Namespace|_|) =
    function
    | YamlValue.StringMapping [|(name, NamespaceChildren c)|] -> Some {name = name; children = c}
    | _ -> None

let private (|Imports|_|) =
    function
    | YamlValue.Sequence elems ->
        let imps =
            elems
            |> Seq.toList
            |> List.choose (function
                | YamlValue.String s -> Some s
                | _ -> None)

        if imps.Length = elems.Length then
            Some imps
        else
            None
    | _ -> None

let private (|TypeDef|_|) name (topLevel: YamlValue) =
    match topLevel?defs with
    | Some (NamespaceChildren defs) ->
        let makeNs imports =
            Some
                {imports = imports
                 defs = {name = name; children = defs}}

        match topLevel?imports with
        | Some (Imports i) -> makeNs (Some i)
        | None -> makeNs None
        | _ -> None
    | _ -> None

let parse name =
    YamlValue.Parse
    >> function
        | TypeDef name d -> Some d
        | _ -> None