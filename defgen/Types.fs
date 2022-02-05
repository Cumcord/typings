module defgen.Types

type Result<'a> =
    | Success of 'a
    | Failure of string

exception FormatException of string

type Prop = {
    kind: string
    typedef: string
}

type Namespace = {
    name: string
    children: NamespaceChild list
}
and NamespaceChild =
    | Nmspc of Namespace
    | Prp of (*string * *)Prop