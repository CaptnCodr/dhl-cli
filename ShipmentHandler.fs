namespace Dhl

open SwaggerProvider
open System
open System.Net.Http

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

    let fetchTrackingNumber (record: TrackingNumberRecord) =
        task {
            let! x = client.GetShipments(record.TrackingNumber, language="de")
            return x.Shipments |> Seq.map printShipmentLine
        }

    let loadTrackingNumbers () =
        task {
            for number in Repository.loadTrackingNumbers() do
                let! shipments = fetchTrackingNumber number
                return shipments |> String.concat Environment.NewLine |> printfn "%s"
        } |> Async.AwaitTask |> Async.RunSynchronously