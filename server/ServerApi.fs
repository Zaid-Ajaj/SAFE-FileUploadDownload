module Server

open Microsoft.Extensions.Logging
open Microsoft.Extensions.Configuration
open Shared
open SkiaSharp
open System.IO

/// An implementation of the Shared IServerApi protocol.
/// Can require ASP.NET injected dependencies in the constructor and uses the Build() function to return value of `IServerApi`.
type ServerApi(logger: ILogger<ServerApi>, config: IConfiguration) =
    member this.Counter() =
        async {
            logger.LogInformation("Executing {Function}", "counter")
            do! Async.Sleep 1000
            return { value = 10 }
        }

    member this.Grayscale(imageBytes: byte[]) : Async<byte[]> = 
        use imageStream = new MemoryStream(imageBytes)
        use bitmap = SKBitmap.Decode(imageStream)

        for x in 0 .. bitmap.Width - 1 do 
            for y in  0 .. bitmap.Height - 1 do
                let currentPixel = bitmap.GetPixel(x, y)
                let average = (currentPixel.Red + currentPixel.Green + currentPixel.Blue) / (byte 3)
                let grayscale = SKColor.Empty.WithRed(average).WithGreen(average).WithBlue(average)
                bitmap.SetPixel(x, y, grayscale.WithAlpha(currentPixel.Alpha))

        let outputImage = bitmap.Encode(SKEncodedImageFormat.Jpeg, 100).ToArray()
        
        async { return outputImage }

    member this.Build() : IServerApi =
        {
            Counter = this.Counter
            Grayscale = this.Grayscale
        }