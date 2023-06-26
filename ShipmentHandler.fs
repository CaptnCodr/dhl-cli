namespace Dhl

open FSharp.Data
open SwaggerProvider
open System.Net.Http

module ShipmentHandler =

    [<Literal>]
    let DaysAfterDelivery = 7

    [<Literal>]
    let MaxRetries = 3

    [<Literal>]
    let ApiAddress = "https://api-eu.dhl.com/track/shipments"

    type DhlSchema = OpenApiClientProvider<"./Data/utapi-traking-api-1.4.1.yaml">

    type ErrorResponse = JsonProvider<"Data/Error.json", ResolutionFolder=__SOURCE_DIRECTORY__>

    let client =
        (new AuthHandler(new ErrorHandler(new HttpClientHandler())))
        |> fun a -> new HttpClient(a, BaseAddress = System.Uri(ApiAddress))
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
             $"[{idx}] {shipment.Id} @ ({System.DateTime.Parse(shipment.Status.Timestamp.ToString())}): {shipment.Status.Description}")

    let printShipmentProblem (exceptionMessage: string) =
        let (number, json) =
            exceptionMessage
            |> fun s -> let i = s.IndexOf(",") in (s.Substring(0, i), s.Substring(i + 1))

        let error = json |> ErrorResponse.Parse
        ("error", $"{number} -> {error.Status} - {error.Title}: {error.Detail}")

    let rec fetchTrackingNumber (retries: int) (idx: int) (TrackingNumber(number)) =
        task {
            try
                let! x = client.GetShipments(number, language = "de")
                return x.Shipments |> Seq.map (printShipmentLine idx number)
            with ex ->
                if retries = 0 then
                    return seq { ex.GetBaseException().Message |> printShipmentProblem }
                else
                    return (fetchTrackingNumber (retries - 1) idx (TrackingNumber(number)))
        }
        |> Async.AwaitTask
        |> Async.RunSynchronously

    let loadTrackingNumbers numbers =
        numbers |> Seq.mapi (fetchTrackingNumber MaxRetries) |> Seq.collect id

    let printShipmentEvent (event: DhlSchema.supermodelIoLogisticsTrackingShipmentEvent) =
        (event.StatusCode, $"{System.DateTime.Parse(event.Timestamp.ToString())}: {event.Description}")

    let loadTrackingNumberDetail (TrackingNumber(number)) =
        task {
            let! shipment = client.GetShipments(number, language = "de")

            return
                shipment.Shipments
                |> Seq.head
                |> fun s -> s.Events |> Seq.map printShipmentEvent
        }
        |> Async.AwaitTask
        |> Async.RunSynchronously
