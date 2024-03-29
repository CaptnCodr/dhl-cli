﻿namespace Dhl

open Argu
open System
open System.Reflection
open Resources
open Arguments
open ShipmentHandler

module Program =

    let statusCodeToColor (code: string) =
        match code with
        | "pre-transit" -> "[97;1m" // WHITE
        | "transit" -> "[93;1m" // YELLOW
        | "delivered" -> "[32;1m" // GREEN
        | "failure" -> "[31;1m" // RED
        | "unknown"
        | _ -> "[90;1m" // GRAY

    let buildColoredLine (element: string * string) =
        let esc = string (char 0x1B)
        $"{esc}{element |> fst |> statusCodeToColor}{element |> snd}{esc}[0m"

    let printTrackingNumberLines elements =
        elements |> Seq.map buildColoredLine |> String.concat Environment.NewLine

    let matchTrackingNumberOrIndex (value: string) (execFunc: TrackingNumber -> string) =
        match value with
        | l when l.Length < 7 ->

            match Int32.TryParse(l) with
            | true, i ->
                Repository.loadTrackingNumbers ()
                |> fun n -> n |> Seq.toArray |> Array.tryItem i
                |> function
                    | Some v -> v |> execFunc
                    | None -> NoTrackingNumber.ResourceString
            | _, _ -> IndexNotParsable.ResourceString

        | _ -> TrackingNumber(value) |> execFunc

    let runCommands (parser: ArgumentParser<CliArguments>) (args: string array) =
        match (parser.Parse args).GetAllResults() with
        | [ Number n ] ->

            match n.GetAllResults() with
            | [ Add a ] -> TrackingNumber(a) |> Repository.add

            | [ Remove r ] -> TrackingNumber(r) |> Repository.remove

            | _ -> parser.PrintUsage()

        | [ Detail d ] -> matchTrackingNumberOrIndex d (loadTrackingNumberDetail >> printTrackingNumberLines)

        | [ Package p ] -> matchTrackingNumberOrIndex p loadTrackingNumberPackageDetails

        | [ Weblink w ] -> matchTrackingNumberOrIndex w getWeblink

        | [ Update ] ->
            let countTransits elements : int =
                elements
                |> Seq.filter (fun (x, _) -> x = "pre-transit" || x = "transit")
                |> Seq.length

            let statuses = Repository.loadTrackingNumbers () |> loadTrackingNumbers

            Environment.SetEnvironmentVariable(
                "DhlCount",
                $"{statuses |> countTransits}",
                EnvironmentVariableTarget.User
            )

            statuses |> printTrackingNumberLines

        | [ SetKey k ] ->
            k |> Settings.setSystemKey
            Key_Set.ResourceString

        | [ GetKey ] -> Settings.getSystemKey ()

        | [ Version ] -> Assembly.GetExecutingAssembly().GetName().Version |> string

        | [ Help ] -> parser.PrintUsage()

        | _ -> parser.PrintUsage()

    [<EntryPoint>]
    let main ([<ParamArray>] args: string[]) : int =
        try
            (ArgumentParser.Create<CliArguments>(), args) ||> runCommands |> printfn "%s"
        with ex ->
            eprintfn $"{ex.Message}"

        0
