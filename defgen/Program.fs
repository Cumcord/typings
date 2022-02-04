open System
open defgen
open defgen.Types

let readStdIn () =
    let rec readRec working =
        match Console.Read() with
        | -1 -> working
        | c -> readRec (working + string (char c))
    
    readRec ""

[<EntryPoint>]
let main _ =
    let raw = readStdIn ()
    let parsedInput = JsonParser.parse raw
    match parsedInput with
    | Failure msg ->
        printfn $"%s{msg}"
        1
    | Success rootNamespace ->
        0