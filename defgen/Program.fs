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
let main args =
    if args.Length <> 1 || String.IsNullOrWhiteSpace args[0] then
        printfn "Please pass just a root name in argv"
        1
    else
        let raw = readStdIn ()
        let parsedInput = JsonParser.parse raw
        match parsedInput with
        | Failure msg ->
            printfn $"%s{msg}"
            1
        | Success nspcChildren ->
            let rootNamespace = {name = args[0]; children = nspcChildren}
            let expanded = Transform.expand rootNamespace
            printfn $"%s{rootNamespace.name}, %i{rootNamespace.children.Length}, %i{expanded.Length}"
            0