namespace Dhl

open FSharp.Data
open SwaggerProvider
open System.Net.Http

module ShipmentHandler =

    [<Literal>]
    let DaysAfterDelivery = 7

    [<Literal>]
    let BaseAdress = "https://api-eu.dhl.com/track/shipments"

    type DhlSchema = OpenApiClientProvider<"./Data/dpdhl_tracking-unified_1.3.2.yaml">

    type ErrorResponse = JsonProvider<"Data/Error.json", ResolutionFolder=__SOURCE_DIRECTORY__>

    let client =
        (new AuthHandler(new ErrorHandler(new HttpClientHandler())))
        |> fun a -> new HttpClient(a, BaseAddress = System.Uri(BaseAdress))
        |> DhlSchema.Client

    let private checkShipmentDate (timstamp: string) : bool =
        (timstamp
         |> System.DateTime.Parse
         |> System.DateTime.Now.Subtract
         |> fun x -> x.Days) > DaysAfterDelivery

    let printShipmentLine (idx: int) (number: string) (shipment: DhlSchema.supermodelIoLogisticsTrackingShipment) =
        if
            shipment.Status.Timestamp.ToString() |> checkShipmentDate
            && shipment.Status.StatusCode = "delivered"
        then
            ("removed", TrackingNumber(number) |> Repository.remove)
        else
            (shipment.Status.StatusCode,
             $"[{idx}] {shipment.Id} @ ({System.DateTime.Parse(shipment.Status.Timestamp.ToString())}): {shipment.Status.Status}")

    let printShipmentProblem (exceptionMessage: string) =
        let (number, json) =
            exceptionMessage
            |> fun s -> let i = s.IndexOf(",") in (s.Substring(0, i), s.Substring(i + 1))

        let error = json |> ErrorResponse.Parse
        ("error", $"{number} -> {error.Status} - {error.Title}: {error.Detail}")

    let fetchTrackingNumber (idx: int) (TrackingNumber (number)) =
        task {
            try
                let! x = client.GetShipments(number, language = "de")
                do! Async.Sleep 500
                return x.Shipments |> Seq.map (printShipmentLine idx number)
            with ex ->
                return seq { ex.GetBaseException().Message |> printShipmentProblem }
        }
        |> Async.AwaitTask
        |> Async.RunSynchronously

    let loadTrackingNumbers numbers =
        numbers |> Seq.mapi fetchTrackingNumber |> Seq.collect id

    let printShipmentEvent (event: DhlSchema.supermodelIoLogisticsTrackingShipmentEvent) =
        (event.StatusCode, $"{System.DateTime.Parse(event.Timestamp.ToString())}: {event.Status}")

    let loadTrackingNumberDetail (TrackingNumber (number)) =
        task {
            let! shipment = client.GetShipments(number, language = "de")

            return
                shipment.Shipments
                |> Seq.head
                |> fun s -> s.Events |> Seq.map printShipmentEvent
        }
        |> Async.AwaitTask
        |> Async.RunSynchronously
