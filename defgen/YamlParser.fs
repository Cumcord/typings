/// Responsible for parsing YAML into a nice data structure
module defgen.YamlParser

open System.Collections.Generic
open System.IO
open System.Text
open YamlDotNet.Serialization
open defgen.Util

/// :husk:
let private deserialize (x: string) =
    x |> Encoding.Default.GetBytes |> MemoryStream |> StreamReader |> Deserializer().Deserialize

let private (|PropertyType|_|) (v: obj) =
    match v with
    | :? string as str ->
        match PropType.parse str with
        | Ok(r) -> Some r
        | Error _ -> None
    | _ -> None

let private (|Property|_|) (prop: obj) =
    match prop with
    | :? IList<obj> as arr ->
        match arr |> Seq.toArray with
        // active patterns :SanOhYes:
        | [| PropertyType t; :? string as def |] -> Some { kind = t; typedef = def }
        | _ -> None
        
    | _ -> None

let rec private (|NamespaceChildren|_|) (content: obj) =
    match content with
    | :? IList<obj> as oArr ->
        let children =
            oArr
            |> Seq.toList
            |> List.map (function
                | Property p -> Some (FPrp p)
                | Namespace n -> Some (FNmspc n)
                | _ -> None)
            
        if children |> List.forall ((<>) None) then
            Some (children |> List.choose id)
        else
            None
    | _ -> None

and private (|Namespace|_|) (nmsp: obj) =
    match nmsp with
    | :? Dictionary<obj, obj> as dict ->
        match dict |> Seq.toList with
        | [pair] ->
            let rawValue = pair.Value
            match rawValue with
            | NamespaceChildren c ->
                match pair.Key with
                | :? string as name -> Some { name = name; children = c }
                | _ -> None
            | _ -> None
        | _ -> None
        
    | _ -> None

let private (|Imports|_|) (raw: obj) =
    match raw with
    | :? IList<obj> as arr ->
        let matchingLists =
            arr
            |> Seq.toList
            |> List.map (function
                | :? string as s -> Some s
                | _ -> None)
            
        if matchingLists |> List.forall ((<>) None) then
            Some (matchingLists |> List.choose id)
        else
            None
    | _ -> None

let private (|TypeDef|_|) (topLevel: obj) =
    match topLevel with
    | :? Dictionary<obj, obj> as dict ->
        if dict.ContainsKey "defs" then
            match dict["defs"] with
            | NamespaceChildren defs ->
                match dict.TryGetValue "imports" with
                | true, Imports i -> Some { imports = Some i; defs = defs }
                | false, _ -> Some { imports = None; defs = defs }
                | _ -> None
            | _ -> None
        else
            None
    | _ -> None

let parse yml =
    match deserialize yml with
    | TypeDef d -> Some d
    | _ -> None