﻿namespace Dhl

open System
open Argu
open Arguments

module Program =     

    let runCommands (parser: ArgumentParser<CliArguments>) (args: string array) : unit =
        match (parser.Parse args).GetAllResults() with
        | [ Number n ] -> 
            match n.GetAllResults() with 

            | [ Add a ] -> 
                { TrackingNumber = a } |> Repository.add |> ignore
                printfn $"{a} added!"

            | [ Remove r ] -> 
                { TrackingNumber = r } |> Repository.remove |> ignore
                printfn $"{r} removed!"

            | _ -> parser.PrintUsage() |> printfn "%s"

        | [ Update ] -> 
            ShipmentHandler.loadTrackingNumbers ()

        | _ -> parser.PrintUsage() |> printfn "%s"

    [<EntryPoint>]
    let main ([<ParamArray>] args: string[]) : int =
    
        try 
            (ArgumentParser.Create<CliArguments>(), args)
            ||> runCommands 
        with 
        | ex -> eprintfn $"{ex.Message}"

        0