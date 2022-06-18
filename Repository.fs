namespace Dhl

open FSharp.Data
open FSharp.Data.Runtime
open System
open System.IO

type TrackingNumbers = CsvProvider<"./Data/sample.csv", Separators=";", ResolutionFolder=__SOURCE_DIRECTORY__>

type TrackingNumber = TrackingNumber of string

module Repository =

    let private path = 
        (Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "dhl.csv")
        |> Path.Combine

    let loadFile () =
        path |> TrackingNumbers.Load

    let saveFile (file: CsvFile<TrackingNumbers.Row>) =
        path |> file.Save

    let loadTrackingNumbers () =
        loadFile().Rows
        |> Seq.map (fun r -> TrackingNumber(r))

    let add (TrackingNumber(number)) =
        loadFile()
        |> fun rows -> rows.Append [ new TrackingNumbers.Row (number) ]
        |> saveFile

        $"{number} added!"

    let remove (TrackingNumber(number)) = 
        loadFile()
        |> fun file -> file.Filter (fun item -> item.TrackingNumber <> number)
        |> saveFile

        $"{number} removed!"