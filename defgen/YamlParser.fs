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

let private (|YmlStrArr|_|) =
    function
    | YamlValue.Sequence elems ->
        let strs =
            elems
            |> Seq.toList
            |> List.choose (function
                | YamlValue.String s -> Some s
                | _ -> None)

        if strs.Length = elems.Length then
            Some strs
        else
            None
    | _ -> None

let private (|TypeDef|_|) (ymlRoot: YamlValue) =
    let imports =
        match ymlRoot?imports with
        | Some (YmlStrArr i) -> Some i
        | _ -> None

    let decls =
        match ymlRoot?decls with
        | Some (YmlStrArr d) -> Some d
        | _ -> None

    match ymlRoot?toplevel with
    | Some (YamlValue.String toplevel) ->
        match ymlRoot?defs with
        | Some (NamespaceChildren defs) ->

            Some
                {imports = imports
                 decls = decls
                 defs = {name = toplevel; children = defs}}

        | _ -> None
    | _ -> None

let parse =
    YamlValue.Parse
    >> function // active patterns moment
        | TypeDef d -> Some d
        | _ -> None