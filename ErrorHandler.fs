namespace Dhl

open System.Net.Http
open System.Web

type ErrorHandler(inner) =
    inherit DelegatingHandler(inner)

    override __.SendAsync(request, cancellationToken) =
        let resp = base.SendAsync(request, cancellationToken)

        async {
            let! result = resp |> Async.AwaitTask

            if result.IsSuccessStatusCode then
                return result
            else
                let! content = result.Content.ReadAsStringAsync() |> Async.AwaitTask

                (HttpUtility.ParseQueryString(request.RequestUri.Query).["trackingNumber"], content)
                ||> fun q c -> failwith $"{q},{c}"

                return result
        }
        |> Async.StartAsTask
