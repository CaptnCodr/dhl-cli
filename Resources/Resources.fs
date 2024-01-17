namespace Dhl

open Extensions.ResourceExt

module Resources =

    [<Literal>]
    let private ResourceFile = "dhl-cli.Resources.Strings"

    type Resource =

        | Arguments_Number
        | Arguments_Detail
        | Arguments_Package
        | Arguments_Weblink
        | Arguments_Update
        | Arguments_SetKey
        | Arguments_GetKey
        | Arguments_Version
        | Arguments_AddNumber
        | Arguments_RemoveNumber
        | Arguments_Help

        | Number_Added
        | Number_Removed

        | Key_Set

        | NoTrackingNumber
        | IndexNotParsable

        member this.ResourceString = ResourceFile |> resourceManager |> getResourceString this

        member this.FormattedString([<System.ParamArray>] args) =
            ResourceFile |> resourceManager |> getFormattedString this args
