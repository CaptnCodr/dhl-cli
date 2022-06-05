namespace Dhl

open System

module Settings =
    
    [<Literal>]
    let DhlKey = "DHL_KEY"
    
    let getSystemKey () =
        (Environment.GetEnvironmentVariable(DhlKey, EnvironmentVariableTarget.User) : string)
        |> Option.ofObj 
        |> Option.defaultValue ""

    let setSystemKey key =
        Environment.SetEnvironmentVariable(DhlKey, key, EnvironmentVariableTarget.User)