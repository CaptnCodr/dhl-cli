namespace Dhl

open Argu
open Resources

module Arguments =

    type TrackingNumbersArgs =
        | [<CliPrefix(CliPrefix.None);AltCommandLine("-a")>] Add of string
        | [<CliPrefix(CliPrefix.None);AltCommandLine("-r")>] Remove of string

        interface IArgParserTemplate with
            member this.Usage =
                match this with 
                | Add _ -> Arguments_AddNumber.ResourceString
                | Remove _ -> Arguments_RemoveNumber.ResourceString

    [<DisableHelpFlags>]
    type CliArguments = 
        | [<CliPrefix(CliPrefix.None);AltCommandLine("-n")>] Number of ParseResults<TrackingNumbersArgs>
        | [<CliPrefix(CliPrefix.None);AltCommandLine("-d")>] Detail of string
        | [<CliPrefix(CliPrefix.None);AltCommandLine("-u")>] Update
        
        | [<CliPrefix(CliPrefix.None);AltCommandLine("-s")>] SetKey of string
        | [<CliPrefix(CliPrefix.None);AltCommandLine("-k")>] GetKey
        | [<CliPrefix(CliPrefix.None);AltCommandLine("-v")>] Version
        | [<CliPrefix(CliPrefix.None);AltCommandLine("-h")>] Help
        
        interface IArgParserTemplate with
            member this.Usage =
                match this with 
                | Number _ -> Arguments_Number.ResourceString
                | Detail _ -> Arguments_Detail.ResourceString
                | Update -> Arguments_Update.ResourceString
                | SetKey _ -> Arguments_SetKey.ResourceString
                | GetKey -> Arguments_GetKey.ResourceString
                | Version -> Arguments_Version.ResourceString
                | Help -> ""
