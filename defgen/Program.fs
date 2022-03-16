open System
open defgen
open defgen.Util

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
        let parsed = YamlParser.parse raw
        0
        (*let parsedInput = JsonParser.parse raw
        match parsedInput with
        | Failure msg ->
            printfn $"%s{msg}"
            1
        | Success nspcChildren ->
            let defs = 
                {name = args[0]; children = nspcChildren}
                |> Transform.expand       // @cumcord/* import madness
                |> Emitter.emitAllModules // emit to .d.ts defs
                
            printfn $"%s{defs}" 
            0*)