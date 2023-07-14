namespace Dhl

open Argu
open Resources

module Arguments =

    type TrackingNumbersArgs =
        | [<CliPrefix(CliPrefix.None); AltCommandLine("-a")>] Add of number: string
        | [<CliPrefix(CliPrefix.None); AltCommandLine("-r")>] Remove of number: string

        interface IArgParserTemplate with
            member this.Usage =
                match this with
                | Add _ -> Arguments_AddNumber.ResourceString
                | Remove _ -> Arguments_RemoveNumber.ResourceString

    [<DisableHelpFlags>]
    type CliArguments =
        | [<CliPrefix(CliPrefix.None); AltCommandLine("-n")>] Number of ParseResults<TrackingNumbersArgs>
        | [<CliPrefix(CliPrefix.None); AltCommandLine("-d")>] Detail of number: string
        | [<CliPrefix(CliPrefix.None); AltCommandLine("-p")>] Package of number: string
        | [<CliPrefix(CliPrefix.None); AltCommandLine("-u")>] Update

        | [<CliPrefix(CliPrefix.None); AltCommandLine("-s")>] SetKey of key: string
        | [<CliPrefix(CliPrefix.None); AltCommandLine("-k")>] GetKey
        | [<CliPrefix(CliPrefix.None); AltCommandLine("-v")>] Version
        | [<CliPrefix(CliPrefix.None); AltCommandLine("-h")>] Help

        interface IArgParserTemplate with
            member this.Usage =
                match this with
                | Number _ -> Arguments_Number.ResourceString
                | Detail _ -> Arguments_Detail.ResourceString
                | Package _ -> Arguments_Package.ResourceString
                | Update -> Arguments_Update.ResourceString
                | SetKey _ -> Arguments_SetKey.ResourceString
                | GetKey -> Arguments_GetKey.ResourceString
                | Version -> Arguments_Version.ResourceString
                | Help -> Arguments_Help.ResourceString
