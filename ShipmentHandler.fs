namespace Dhl

open SwaggerProvider
open System
open System.Net.Http
open Arguments

module ShipmentHandler =

    [<Literal>]
    let baseAdress = "https://api-eu.dhl.com/track/shipments"

    type DhlSchema = OpenApiClientProvider<"./Data/dpdhl_tracking-unified_1.3.1.yaml">

    let client = 
        (new AuthHandler(new HttpClientHandler()))
        |> fun a -> new HttpClient(a, BaseAddress=Uri(baseAdress))
        |> DhlSchema.Client

    let printShipmentLine (shipment: DhlSchema.supermodelIoLogisticsTrackingShipment) =
        $"{shipment.Id} @ ({DateTime.Parse(shipment.Status.Timestamp.ToString())}): {shipment.Status.Status}"

    let fetchTrackingNumber (TrackingNumber(number)) =
        task {
            let! x = client.GetShipments(number, language="de")
            return x.Shipments |> Seq.map printShipmentLine
        }

    let loadTrackingNumbers numbers =
        task {
            for number in numbers do
                let! shipments = fetchTrackingNumber number
                return shipments |> String.concat Environment.NewLine |> printfn "%s"
        } |> Async.AwaitTask |> Async.RunSynchronously

    let printShipmentEvent (event: DhlSchema.supermodelIoLogisticsTrackingShipmentEvent) =
        $"{DateTime.Parse(event.Timestamp.ToString())}: {event.Status}"

    let loadTrackingNumberDetail (TrackingNumber(number)) = 
        task {
            let! shipment = client.GetShipments(number, language="de")
            return shipment.Shipments |> Seq.head |> fun s -> s.Events |> Seq.map printShipmentEvent
        } |> Async.AwaitTask |> Async.RunSynchronously