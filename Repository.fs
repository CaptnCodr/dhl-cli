namespace Dhl

open FSharp.Data
open FSharp.Data.Runtime
open System
open System.IO
open Arguments

type TrackingNumbers = CsvProvider<"./Data/sample.csv", Separators=";", ResolutionFolder=__SOURCE_DIRECTORY__>

module Repository =

    let private Path () = 
        (Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "dhl.csv")
        |> Path.Combine

    let loadFile () =
        Path() |> TrackingNumbers.Load

    let saveFile (file: CsvFile<TrackingNumbers.Row>) =
        Path() |> file.Save

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