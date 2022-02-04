module defgen.Types

type Result<'a> =
    | Success of 'a
    | Failure of string

exception FormatException of string

type PropKind = Const | Function 

let parsePropKind =
    function
    | "const" -> Const
    | "function" -> Function
    | s -> raise (FormatException($"Cannot parse `%s{s}` as prop kind"))

type Prop = {
    kind: PropKind
    typedef: string
}

type Namespace = {
    name: string
    children: NamespaceChild list
}
and NamespaceChild =
    | Nmspc of Namespace
    | Prp of (*string * *)Prop