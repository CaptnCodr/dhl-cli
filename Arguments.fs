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
        | [<CliPrefix(CliPrefix.None);AltCommandLine("-d")>] Detail of string
        | [<CliPrefix(CliPrefix.None);AltCommandLine("-u")>] Update
        
        | [<CliPrefix(CliPrefix.None);AltCommandLine("-s")>] SetKey of string
        | [<CliPrefix(CliPrefix.None);AltCommandLine("-k")>] GetKey
        | [<CliPrefix(CliPrefix.None);AltCommandLine("-v")>] Version
        
        interface IArgParserTemplate with
            member this.Usage =
                match this with 
                | Number _ -> "The tracking number options."
                | Detail _ -> "Shows tracking details about the given number."
                | Update -> "Updates all tracking events."
                | SetKey _ -> "Sets the API key."
                | GetKey -> "Gets the API key."
                | Version -> "Gets the current version of this binary."
