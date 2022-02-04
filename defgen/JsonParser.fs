module defgen.JsonParser

open FSharp.Data
open defgen.Types

let listToNspc name list = Nmspc({name = name; children = list})

/// Parses an array of [string, string] to a `Prop`
let private parseProp (elems: JsonValue[]) =
    if elems.Length <> 2 then
        raise (FormatException("Props should only have two args in the array"))
    else
        match elems[0] with
        | JsonValue.String s ->
            match elems[1] with
            | JsonValue.String s2 ->
                { kind = parsePropKind s; typedef = s2 }
                
            | _ -> raise (FormatException("Prop arrays must contain strings"))
            
        | _ -> raise (FormatException("Prop arrays must contain strings"))

/// Parses a list of props to either a NamespaceChild or nothing - use inside List.choose!
let private parseListToProps nspName (elems: JsonValue[]) =
    if elems.Length = 0 then
        (*
            remove things like the following (call this func inside List.choose):
            "emptyNamespace": []
        *)
        None
    else
        match elems[0] with
        | JsonValue.Array _ ->
            elems
            |> Array.toList
            |> List.map(function
                    | JsonValue.Array es -> Prp(parseProp es)
                    | _ -> raise (FormatException("Prop list type was incorrect")))
            |> listToNspc nspName
            |> Some
        | _ -> Some(Prp(parseProp elems))

/// Recursively parses into a list of NamespaceChild (wraps recursed down in a Namespace)
let rec private recursiveParse jsonVal =
    match jsonVal with
    | JsonValue.Record props ->
        props
        |> Array.toList
        |> List.choose (fun v ->
            
            match snd v with
            | JsonValue.Record _ -> snd v |> recursiveParse |> listToNspc (fst v) |> Some
            | JsonValue.Array elems -> parseListToProps (fst v) elems
            | _ -> raise (FormatException("Record value was not a valid type"))
            )
        
    | _ -> raise (FormatException("JSON was not a usable type, should be a namespace"))

/// Parses a JSON string into a nice usable F# data structure
let parse raw =
    let rawMaybeParsed = JsonValue.TryParse raw
    
    match rawMaybeParsed with
    | None -> Failure("Parse failed")
    | Some(rawParsed) ->
        // ew exceptions, not very functional
        // they are a clean ish way to break out of a function early tho
        // so ill use that ig
        try
            Success(recursiveParse rawParsed)
        with
            | FormatException(e) -> Failure(e)