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
        let parsedInput = YamlParser.parse args[0] raw
        match parsedInput with
        | None ->
            printfn "Parse failed"
            1
        | Some def -> 
            let defs = 
                def.defs
                |> Transform.flatten      // @cumcord/* import madness
                |> Emitter.emitFull def.imports // emit to .d.ts defs
                
            printfn $"%s{defs}" 
            0