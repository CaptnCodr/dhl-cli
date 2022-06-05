namespace Dhl

open System.Net.Http

type AuthHandler(messagehandler) =
    inherit DelegatingHandler(messagehandler)

    override __.SendAsync(request, cancellationToken) =
        request.Headers.TryAddWithoutValidation("DHL-API-Key", Settings.getSystemKey()) |> ignore
        base.SendAsync(request, cancellationToken)