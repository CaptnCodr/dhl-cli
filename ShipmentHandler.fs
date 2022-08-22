namespace Dhl

open SwaggerProvider
open System.Net.Http

module ShipmentHandler =

    [<Literal>]
    let baseAdress = "https://api-eu.dhl.com/track/shipments"

    type DhlSchema = OpenApiClientProvider<"./Data/dpdhl_tracking-unified_1.3.1.yaml">

    let client = 
        (new AuthHandler(new HttpClientHandler()))
        |> fun a -> new HttpClient(a, BaseAddress=System.Uri(baseAdress))
        |> DhlSchema.Client

    let printShipmentLine (idx: int) (number: string) (shipment: DhlSchema.supermodelIoLogisticsTrackingShipment) =
        if (shipment.Status.Timestamp.ToString() |> System.DateTime.Parse |> System.DateTime.Now.Subtract |> fun x -> x.Days) > 14 && shipment.Status.StatusCode = "delivered" then
            ("removed", TrackingNumber(number) |> Repository.remove)
        else
            (shipment.Status.StatusCode, $"[{idx}] {shipment.Id} @ ({System.DateTime.Parse(shipment.Status.Timestamp.ToString())}): {shipment.Status.Status}")

    let printShipmentProblem (title: string) (status: int) (detail: string) =
        ("bla", $"{title} ({status}): {detail}")

    let fetchTrackingNumber (idx: int) (TrackingNumber(number)) =
        task {
            let! x = client.GetShipments(number, language="de")
            return x.Shipments |> Seq.map (printShipmentLine idx number)
        } |> Async.AwaitTask |> Async.RunSynchronously

    let loadTrackingNumbers numbers =
        numbers 
        |> Seq.mapi fetchTrackingNumber
        |> Seq.collect id

    let printShipmentEvent (event: DhlSchema.supermodelIoLogisticsTrackingShipmentEvent) =
        (event.StatusCode, $"{System.DateTime.Parse(event.Timestamp.ToString())}: {event.Status}")

    let loadTrackingNumberDetail (TrackingNumber(number)) = 
        task {
            let! shipment = client.GetShipments(number, language="de")
            return shipment.Shipments |> Seq.head |> fun s -> s.Events |> Seq.map printShipmentEvent
        } |> Async.AwaitTask |> Async.RunSynchronously