namespace Dhl

open FSharp.Data
open FSharp.Data.Runtime
open System
open System.IO

type TrackingNumbers = CsvProvider<"./Data/sample.csv", Separators=";", ResolutionFolder=__SOURCE_DIRECTORY__>

type TrackingNumberRecord = { TrackingNumber: string }

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
        |> Seq.map (fun r -> { TrackingNumber = r.TrackingNumber })

    let add (record: TrackingNumberRecord) =
        loadFile()
        |> fun rows -> rows.Append [ new TrackingNumbers.Row (record.TrackingNumber) ]
        |> saveFile

    let remove (record: TrackingNumberRecord) = 
        loadFile()
        |> fun file -> file.Filter (fun item -> item.TrackingNumber <> record.TrackingNumber)
        |> saveFile
        