namespace Dhl

open System
open System.Reflection
open System.Resources

module Resources = 

    [<Literal>]
    let ResourceFile = "dhl-cli.Resource.Strings"

    type Resource =

        | Arguments_Number
        | Arguments_Detail
        | Arguments_Update
        | Arguments_SetKey
        | Arguments_GetKey
        | Arguments_Version
        | Arguments_AddNumber
        | Arguments_RemoveNumber
        
        | Number_Added
        | Number_Removed

        | Key_Set

        | NoTrackingNumber
        | IndexNotParsable

        member this.ResourceString =
            this.ToString() 
            |> ResourceManager(ResourceFile, Assembly.GetExecutingAssembly()).GetString

        member this.FormattedString ([<ParamArray>] args) =
            (this.ResourceString, args) |> String.Format