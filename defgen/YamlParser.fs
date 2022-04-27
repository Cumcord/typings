/// Responsible for parsing YAML into a nice data structure
module defgen.YamlParser

open FSharp.Data
open defgen.Util

let private (|MemType|_|) =
    function
    | YamlValue.String (MemberType t) -> Some t
    | _ -> None

let private (|Member|_|) =
    function
    // active patterns and just pattern matching in general :SanOhYes:
    | YamlValue.Sequence [|MemType Export; YamlValue.String name; YamlValue.String src|] ->
        Some
            {kind = Export
             typedef = name
             secondPart = Some src}
    | YamlValue.Sequence [|MemType t; YamlValue.String def|] ->
        Some
            {kind = t
             typedef = def
             secondPart = None}
    | _ -> None

let rec private (|NamespaceChildren|_|) =
    function
    | YamlValue.Sequence rawChildren ->
        let children =
            rawChildren
            |> Seq.toList
            |> List.choose (function
                | Member p -> Some(FMem p)
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

let private (|TypeDef|_|) (ymlRoot: YamlValue) =
    let decls =
        match ymlRoot?decls with
        | Some (YamlValue.String d) -> Some d
        | _ -> None

    match ymlRoot?toplevel with
    | Some (YamlValue.String toplevel) ->
        match ymlRoot?defs with
        | Some (NamespaceChildren defs) ->

            Some
                {decls = decls
                 defs = {name = toplevel; children = defs}}

        | _ -> None
    | _ -> None

// turns out you can call active patterns as normal functions too!
let parse = YamlValue.Parse >> (|TypeDef|_|)