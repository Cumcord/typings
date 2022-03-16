module defgen.Util

exception JSON_FormatException of string

type PropType =
    | Const | Function
    
    static member parse =
        function
        | "const" -> Ok Const
        | "function" -> Ok Function
        | _ -> Error "not const or function"
    
    member this.ToString = match this with | Const -> "const" | Function -> "function"

type Prop = {
    kind: (*string*) PropType
    typedef: string
}

type Namespace = {
    name: string
    children: NamespaceChild list
}
and NamespaceChild =
    | Nmspc of Namespace
    | Prp of Prop


/// lists all props on an object
let inline (!?) (object: obj) =
    object.GetType().GetProperties() |> Array.toList

/// dynamically gets the value of a property safely
let inline (?) (object: obj) prop =
    let prop = !? object |> List.tryFind (fun p -> p.Name = prop)
    match prop with
    | None -> None
    | Some p -> Some (p.GetValue(object))