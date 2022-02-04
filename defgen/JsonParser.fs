module defgen.JsonParser

open FSharp.Data
open defgen.Types

let parseProp (elems: JsonValue[]) =
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

let rec private recursiveParseNamespace jsonVal name =
    let children =
        match jsonVal with
        | JsonValue.Record props ->
            props
            |> Array.toList
            |> List.map (fun v ->
                let props = 
                    match snd v with
                    | JsonValue.Record _ -> [Nmspc(recursiveParseNamespace (snd v) (fst v))]
                    | JsonValue.Array elems ->
                        if elems.Length = 0 then
                            []
                        else
                            match elems[0] with
                            | JsonValue.Array _ ->
                                elems
                                |> Array.toList
                                |> List.map(function
                                        | JsonValue.Array es -> Prp(parseProp es)
                                        | _ -> raise (FormatException("Prop list type was incorrect")))
                            | _ -> [Prp(parseProp elems)]
                    
                    | _ -> raise (FormatException("Record value was not a valid type"))
                
                Nmspc({name = fst v; children = props})
                )
            
        | _ -> raise (FormatException("JSON was not a usable type, should be a namespace"))
    
    {name = name; children = children}

let parse raw =
    let rawMaybeParsed = JsonValue.TryParse raw
    
    match rawMaybeParsed with
    | None -> Failure("Parse failed")
    | Some(rawParsed) ->
        // ew exceptions, not very functional
        // they are a clean ish way to break out of a function early tho
        // so ill use that ig
        try
            Success(recursiveParseNamespace rawParsed "")
        with
            | FormatException(e) -> Failure(e)