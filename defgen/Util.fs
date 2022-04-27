module defgen.Util

open FSharp.Data

type MemberType =
    | Const
    | Function
    | Export
    | Import

    static member parse =
        function
        | "const" -> Ok Const
        | "function" -> Ok Function
        | "export" -> Ok Export
        | "import" -> Ok Import
        | _ -> Error "not valid member type"

    override this.ToString() =
        match this with
        | Const -> "const"
        | Function -> "function"
        | Export -> "export"
        | Import -> "import"

let (|MemberType|_|) =
    MemberType.parse
    >> function
        | Ok r -> Some r
        | Error _ -> None

/// dynamically but safely gets a string prop on a yaml object
let (?) (ymlVal: YamlValue) prop =
    ymlVal.TryGetProperty(YamlValue.String prop)

type Member =
    {kind: MemberType
     typedef: string
     secondPart: string option}

type FullNamespace =
    {name: string
     children: FullNamespaceChild list}

and FullNamespaceChild =
    | FNmspc of FullNamespace
    | FMem of Member

type ParsedDefs =
    {decls: string option
     defs: FullNamespace}

// (full name, subname)
type NamespaceReference = string * string

type MemberOrReference =
    | CMem of Member
    | CRef of NamespaceReference

type ContractedNamespace =
    {name: string
     children: MemberOrReference list}