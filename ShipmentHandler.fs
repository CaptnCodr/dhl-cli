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

    type Dimension = JsonProvider<"""{ "value": 0.709, "unitText": "kg" }""">

    let client =
        (new AuthHandler(new ErrorHandler(new HttpClientHandler())))
        |> fun a -> new HttpClient(a, BaseAddress = System.Uri(ApiAddress))
        |> DhlSchema.Client

    let private checkShipmentDate (timestamp: string) : bool =
        (timestamp |> System.DateTime.Parse |> System.DateTime.Now.Subtract |> _.Days) > DaysAfterDelivery

    let private dateTimeStringToTimeOnly (dateTime: string) =
        dateTime |> System.DateTime.Parse |> System.TimeOnly.FromDateTime
    
    let printShipmentLine (idx: int) (number: string) (shipment: DhlSchema.supermodelIoLogisticsTrackingShipment) =
        if
            shipment.Status.Timestamp.ToString() |> checkShipmentDate
            && shipment.Status.StatusCode = "delivered"
        then
            ("removed", TrackingNumber(number) |> Repository.remove)
        else
            let shipmentLine =
                match shipment.Status.Description with
                | null
                | "" -> shipment.Status.Status
                | _ -> shipment.Status.Description
                
            let dateOfDelivery =
                match shipment.EstimatedTimeOfDelivery with
                | null -> ""
                | dod -> $"-> {System.DateOnly.Parse(dod.ToString())}"

            let timeFrame =
                match shipment.EstimatedDeliveryTimeFrame with
                | null -> ""
                | tf -> $" ({tf.EstimatedFrom.ToString() |> dateTimeStringToTimeOnly}-{tf.EstimatedThrough.ToString() |> dateTimeStringToTimeOnly})"

            let deliveryRemark =
                match shipment.EstimatedTimeOfDeliveryRemark with
                | null -> ""
                | remark -> $" ({remark.ToString()})"

            (shipment.Status.StatusCode,
             $"[{idx}] {shipment.Id} @ ({System.DateTime.Parse(shipment.Status.Timestamp.ToString())}): {shipmentLine} {dateOfDelivery}{timeFrame}{deliveryRemark}")

    let printShipmentProblem (exceptionMessage: string) =
        let number, json =
            exceptionMessage
            |> fun s -> let i = s.IndexOf(",") in (s.Substring(0, i), s.Substring(i + 1))

        let error = json |> ErrorResponse.Parse
        ("error", $"{number} -> {error.Status} - {error.Title}: {error.Detail}")

    let getShipments trackingNumber =
        task {
            let! x = client.GetShipments(trackingNumber, language = "de")
            return x.Shipments
        }

    let rec fetchTrackingNumber (retries: int) (idx: int) (TrackingNumber(number)) =
        task {
            try
                let! shipments = getShipments number
                return shipments |> Seq.map (printShipmentLine idx number)
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
        let eventLine =
            match event.Description with
            | null
            | "" -> event.Status
            | _ -> event.Description

        (event.StatusCode, $"{System.DateTime.Parse(event.Timestamp.ToString())}: {eventLine}")

    let loadTrackingNumberDetail (TrackingNumber(number)) =
        task {
            let! shipments = getShipments number
            return shipments |> Seq.head |> (fun s -> s.Events |> Seq.map printShipmentEvent)
        }
        |> Async.AwaitTask
        |> Async.RunSynchronously

    let getDimension dimension =
        match dimension with
        | null -> ""
        | _ ->
            let dim = Dimension.Parse(dimension.ToString())

            match dim.UnitText with
            | "m" -> $"{(dim.Value * 100.0m):N1} cm"
            | _ -> $"{dim.Value} {dim.UnitText}"

    let printPackageDetails (details: DhlSchema.supermodelIoLogisticsTrackingShipmentDetails) =
        let dimensions =
            match details.Dimensions with
            | null -> ""
            | d -> $"\nWidth: {getDimension d.Width}\nHeight: {getDimension d.Height}\nLength: {getDimension d.Length}"

        $"Weight: {getDimension details.Weight}{dimensions}"

    let loadTrackingNumberPackageDetails (TrackingNumber(number)) =
        task {
            let! shipments = getShipments number
            return shipments |> Seq.head |> (fun s -> s.Details |> printPackageDetails)
        }
        |> Async.AwaitTask
        |> Async.RunSynchronously

    let getWeblink (TrackingNumber(number)) =
        task {
            let! shipments = getShipments number
            return shipments |> Seq.head |> _.ServiceUrl |> string
        }
        |> Async.AwaitTask
        |> Async.RunSynchronously
