module defgen.JsonParser

open FSharp.Data
open defgen.Types

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

let rec private recursiveParse jsonVal =
    match jsonVal with
    | JsonValue.Record props ->
        props
        |> Array.toList
        |> List.choose (fun v ->
            let listToNspc list = Nmspc({name = fst v; children = list})
            
            match snd v with
            | JsonValue.Record _ -> (snd v) |> recursiveParse |> listToNspc |> Some
            | JsonValue.Array elems ->
                if elems.Length = 0 then
                    None // List.choose BRRRRR
                else
                    match elems[0] with
                    | JsonValue.Array _ ->
                        elems
                        |> Array.toList
                        |> List.map(function
                                | JsonValue.Array es -> Prp(parseProp es)
                                | _ -> raise (FormatException("Prop list type was incorrect")))
                        |> listToNspc
                        |> Some
                    | _ -> Some(Prp(parseProp elems))
                    
            | _ -> raise (FormatException("Record value was not a valid type"))
            )
        
    | _ -> raise (FormatException("JSON was not a usable type, should be a namespace"))

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