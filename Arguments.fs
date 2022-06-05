namespace Dhl

open Argu

module Arguments =

    type TrackingNumbersArgs =
        | [<CliPrefix(CliPrefix.None);AltCommandLine("-a")>] Add of string
        | [<CliPrefix(CliPrefix.None);AltCommandLine("-r")>] Remove of string

        interface IArgParserTemplate with
            member this.Usage =
                match this with 
                | Add _ -> "Adds a tracking number to the storage."
                | Remove _ -> "Removes the specified number from storage."

    type CliArguments = 
        | [<CliPrefix(CliPrefix.None);AltCommandLine("-n")>] Number of ParseResults<TrackingNumbersArgs>
        | [<CliPrefix(CliPrefix.None);AltCommandLine("-u")>] Update

        interface IArgParserTemplate with
            member this.Usage =
                match this with 
                | Number _ -> "The tracking number options."
                | Update -> "Updates all tracking events."


