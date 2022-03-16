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
        let parsedInput = YamlParser.parse raw
        match parsedInput with
        | None ->
            printfn "Parse failed"
            1
        | Some def ->
            let rootNamespace: FullNamespace = {name = args[0]; children = def.defs} 
            let defs = 
                rootNamespace
                |> Transform.flatten      // @cumcord/* import madness
                |> Emitter.emitAllModules // emit to .d.ts defs
                
            printfn $"%s{defs}" 
            0