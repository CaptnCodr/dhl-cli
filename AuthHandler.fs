namespace Dhl

open System.Net.Http

type AuthHandler(messageHandler) =
    inherit DelegatingHandler(messageHandler)

    override _.SendAsync(request, cancellationToken) =
        request.Headers.TryAddWithoutValidation("DHL-API-Key", Settings.getSystemKey ())
        |> ignore

        base.SendAsync(request, cancellationToken)
