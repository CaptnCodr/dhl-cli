namespace Dhl

open Argu
open System
open System.Reflection
open Arguments
open ShipmentHandler

module Program =

    let statusCodeToColor (code: string) = 
        match code with 
        | "pre-transit" -> "[31;1m"
        | "transit" -> "[93;1m"
        | "delivered" -> "[32;1m"
        | "failure" -> "[94;1m"
        | "unknown" | _ -> "[90;1m"

    let buildColoredLine (element: string * string) =
        let esc = string (char 0x1B)
        $"{esc}{element |> fst |> statusCodeToColor}{element |> snd}{esc}[0m"

    let printTrackingNumberLines elements =
        elements
        |> Seq.map buildColoredLine
        |> String.concat Environment.NewLine

    let runCommands (parser: ArgumentParser<CliArguments>) (args: string array) =
        match (parser.Parse args).GetAllResults() with
        | [ Number n ] -> 

            match n.GetAllResults() with 
            | [ Add a ] -> 
                TrackingNumber(a) |> Repository.add

            | [ Remove r ] -> 
                TrackingNumber(r) |> Repository.remove

            | _ -> parser.PrintUsage()

        | [ Detail d ] -> 
            TrackingNumber(d)
            |> loadTrackingNumberDetail 
            |> printTrackingNumberLines

        | [ Update ] -> 
            Repository.loadTrackingNumbers() 
            |> loadTrackingNumbers 
            |> printTrackingNumberLines

        | [ SetKey k ] -> 
            k |> Settings.setSystemKey
            "Key set!"

        | [ GetKey ] -> 
            Settings.getSystemKey()

        | [ Version ] -> 
            Assembly.GetExecutingAssembly().GetName().Version |> string

        | _ -> parser.PrintUsage()

    [<EntryPoint>]
    let main ([<ParamArray>] args: string[]) : int =
    
        try 
            (ArgumentParser.Create<CliArguments>(), args)
            ||> runCommands |> printfn "%s"
        with 
        | ex -> eprintfn $"{ex.Message}"

        0