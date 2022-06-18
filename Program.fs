namespace Dhl

open Argu
open System
open System.Reflection
open Arguments

module Program =

    let runCommands (parser: ArgumentParser<CliArguments>) (args: string array) : unit =
        match (parser.Parse args).GetAllResults() with
        | [ Number n ] -> 

            match n.GetAllResults() with 

            | [ Add a ] -> 
                a |> TrackingNumber |> Repository.add |> ignore
                printfn $"{a} added!"

            | [ Remove r ] -> 
                r |> TrackingNumber |> Repository.remove |> ignore
                printfn $"{r} removed!"

            | _ -> parser.PrintUsage() |> printfn "%s"

        | [ Detail d ] -> 
            let details = d |> TrackingNumber |> ShipmentHandler.loadTrackingNumberDetail
            details |> Seq.iter (printfn "%s")

        | [ Update ] -> 
            Repository.loadTrackingNumbers() |> ShipmentHandler.loadTrackingNumbers

        | [ SetKey k ] -> 
            k |> Settings.setSystemKey
            "Key set!" |> printfn "%s"

        | [ GetKey ] -> 
            Settings.getSystemKey() |> printfn "%s"

        | [ Version ] -> 
            Assembly.GetExecutingAssembly().GetName().Version |> string |> printfn "%s"

        | _ -> parser.PrintUsage() |> printfn "%s"

    [<EntryPoint>]
    let main ([<ParamArray>] args: string[]) : int =
    
        try 
            (ArgumentParser.Create<CliArguments>(), args)
            ||> runCommands 
        with 
        | ex -> eprintfn $"{ex.Message}"

        0