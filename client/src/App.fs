module App

open Feliz
open Elmish
open Shared
open Browser.Types
open Fable.Remoting.Client

type ProcessedImage = {
    OriginalImageFileName: string
    OriginalImage : byte[]
    ProcessedImage : byte[]
}

type State = {
    ProcessedImages : Deferred<ProcessedImage>
}

type Msg =
    | ProcessImage of File
    | ImageProcessedSuccessfully of ProcessedImage
    | Download
    | Reset
    | DoNothing

let init() = { ProcessedImages = HasNotStartedYet } , Cmd.none

let update (msg: Msg) (state: State) =
    match msg with
    | ProcessImage (imageFile: File) ->
        let processImage = async {
            try
                let! imageBytes = imageFile.ReadAsByteArray()
                let! grayscaleImage = Server.api.Grayscale imageBytes
                return ImageProcessedSuccessfully {
                    OriginalImageFileName = imageFile.name
                    OriginalImage = imageBytes
                    ProcessedImage = grayscaleImage
                }

            with error ->
                Log.developmentError error
                return DoNothing
        }

        { state with ProcessedImages = InProgress }, Cmd.fromAsync processImage

    | ImageProcessedSuccessfully images ->
        { state with ProcessedImages = Resolved images }, Cmd.none

    | Reset ->
        { state with ProcessedImages = HasNotStartedYet }, Cmd.none

    | Download ->
        match state.ProcessedImages with
        | Resolved images ->
            let fileName = images.OriginalImageFileName
            images.ProcessedImage.SaveFileAs(fileName)
            state, Cmd.none

        | _ ->
            state, Cmd.none

    | DoNothing ->
        state, Cmd.none

let fableLogo() = StaticFile.import "./imgs/fable_logo.png"

let render (state: State) (dispatch: Msg -> unit) =

    Html.div [
        prop.style [
            style.textAlign.center
            style.padding 40
        ]

        prop.children [

            match state.ProcessedImages with
            | HasNotStartedYet ->
                Html.input [
                    prop.type'.file
                    prop.onChange (ProcessImage >> dispatch)
                ]

            | InProgress ->
                Html.h1 "Loading"

            | Resolved images ->

                Html.div [
                    Html.button [
                        prop.text "Reset"
                        prop.style [ style.padding 20; style.margin 10 ]
                        prop.onClick (fun _ -> dispatch Reset)
                    ]

                    Html.button [
                        prop.text "Download"
                        prop.style [ style.padding 20; style.margin 10 ]
                        prop.onClick (fun _ -> dispatch Download)
                    ]

                    Html.br [ ]
                    Html.img [
                        prop.style [ style.margin 10 ]
                        prop.src (images.OriginalImage.AsDataUrl())
                    ]

                    Html.img [
                        prop.style [ style.margin 10 ]
                        prop.src (images.ProcessedImage.AsDataUrl())
                    ]
                ]
        ]
    ]