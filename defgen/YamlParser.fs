/// Responsible for parsing YAML into a nice data structure
module defgen.YamlParser

open System
open System.IO
open System.Text
open YamlDotNet.Serialization
open defgen.Util

/// :husk:
let private deserialize (x: string) =
    x |> Encoding.Default.GetBytes |> MemoryStream |> StreamReader |> Deserializer().Deserialize

let private (|PropertyType|_|) v =
    match PropType.parse v with
    | Ok(r) -> Some r
    | Error _ -> None

let private parseProp (prop: obj) =
    match prop with
    | :? array<string> as sa ->
        match sa with
        // active patterns :SanOhYes:
        | [| PropertyType t; def |] -> Ok { kind = t; typedef = def }
        | _ -> Error "array was not in the expected format"
        
    | _ -> Error "prop was not a string array"

let private parseNamespace (nmsp: obj) = raise (NotImplementedException())

let parse yml =
    let tree = deserialize yml
    
    raise (NotImplementedException())