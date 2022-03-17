module defgen.Util

type PropType =
    | Const | Function
    
    static member parse =
        function
        | "const" -> Ok Const
        | "function" -> Ok Function
        | _ -> Error "not const or function"
    
    override this.ToString () = match this with | Const -> "const" | Function -> "function"

type Prop = {
    kind: PropType
    typedef: string
}

type FullNamespace = {
    name: string
    children: FullNamespaceChild list
}
and FullNamespaceChild =
    | FNmspc of FullNamespace
    | FPrp of Prop

type ParsedDefs = {
    imports: string list option
    defs: FullNamespace
}

// (full name, subname)
type NamespaceReference = string * string

type PropOrReference =
    | CPrp of Prop
    | CRef of NamespaceReference

type ContractedNamespace = {
    name: string
    children: PropOrReference list
}