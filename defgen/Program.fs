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
let main _ =
    let parsedInput = readStdIn() |> YamlParser.parse
    
    match parsedInput with
    | None ->
        printfn "Parse failed"
        1
    | Some def -> 
        let defs = 
            def.defs
            |> Transform.flatten // @cumcord/* import madness
            |> Emitter.emitFull def.imports def.decls // emit to .d.ts defs
            
        printfn $"%s{defs}" 
        0