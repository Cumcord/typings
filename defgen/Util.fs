module defgen.Util

type PropType =
    | Const | Function
    
    static member parse =
        function
        | "const" -> Ok Const
        | "function" -> Ok Function
        | _ -> Error "not const or function"
    
    member this.ToString = match this with | Const -> "const" | Function -> "function"

type Prop = {
    kind: PropType
    typedef: string
}

type Namespace = {
    name: string
    children: NamespaceChild list
}
and NamespaceChild =
    | Nmspc of Namespace
    | Prp of Prop

type Defs = {
    imports: string list option
    defs: NamespaceChild list
}


/// lists all props on an object
let inline (!?) (object: obj) =
    object.GetType().GetProperties()
    |> Array.toList
    |> List.map (fun p -> (p.Name, p.GetValue object))

/// dynamically gets the value of a property safely
let inline (?) (object: obj) prop =
    let prop = !? object |> List.tryFind (fst >> ((=) prop))
    match prop with
    | None -> None
    | Some p -> Some (snd p)