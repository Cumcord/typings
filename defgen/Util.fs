module defgen.Util

open FSharp.Data

type PropType =
    | Const
    | Function

    static member parse =
        function
        | "const" -> Ok Const
        | "function" -> Ok Function
        | _ -> Error "not const or function"

    override this.ToString() =
        match this with
        | Const -> "const"
        | Function -> "function"

let (|PropertyType|_|) =
    PropType.parse
    >> function
        | Ok r -> Some r
        | Error _ -> None

/// dynamically but safely gets a string prop on a yaml object
let (?) (ymlVal: YamlValue) prop =
    ymlVal.TryGetProperty(YamlValue.String prop)

type Prop = {kind: PropType; typedef: string}

type FullNamespace =
    {name: string
     children: FullNamespaceChild list}

and FullNamespaceChild =
    | FNmspc of FullNamespace
    | FPrp of Prop

type ParsedDefs =
    {imports: string list option
     defs: FullNamespace}

// (full name, subname)
type NamespaceReference = string * string

type PropOrReference =
    | CPrp of Prop
    | CRef of NamespaceReference

type ContractedNamespace =
    {name: string
     children: PropOrReference list}