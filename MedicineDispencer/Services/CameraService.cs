using System;
using System.Collections.Generic;
using System.Text.Json;
using OpenCvSharp;
using ZXing;
using ZXing.Windows.Compatibility;

public class CameraService
{
    public CameraService() { }

    public void StartScanner()
    {
        using var capture = new VideoCapture(0); // 0 = default camera
        using var window = new Window("QR Scanner");

        Console.WriteLine("Scanner gestart. Druk op 'q' om te stoppen.");

        using var frame = new Mat();
        var reader = new ZXing.QrCode.QRCodeReader();

        while (true)
        {
            capture.Read(frame);
            if (frame.Empty()) continue;

            using var bitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(frame);

            var luminanceSource = new BitmapLuminanceSource(bitmap);
            var binarizer = new ZXing.Common.HybridBinarizer(luminanceSource);
            var binaryBitmap = new BinaryBitmap(binarizer);

            Result? result = null;
            try
            {
                result = reader.decode(binaryBitmap);
            }
            catch { /* ignore decode errors */ }

            if (result != null)
            {
                var code = result.Text;
                try
                {
                    var qrData = JsonSerializer.Deserialize<CompartmentQrData>(code);
                    if (qrData != null)
                    {
                        Console.WriteLine($"QR Medicijn: {qrData.MedicijnNaam}, Dosis: {qrData.Dosis}, Voorraad: {qrData.Voorraad}, Tijden: {string.Join(", ", qrData.DoseringstijdenPerDag)}");
                        // You can now use qrData to fill a MedicijnCompartiment later
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("QR code kon niet worden gelezen als JSON: " + ex.Message);
                }

                // Draw rectangle if possible
                if (result.ResultPoints.Length >= 2)
                {
                    var p1 = result.ResultPoints[0];
                    var p2 = result.ResultPoints[1];
                    Cv2.Rectangle(frame, new Point((int)p1.X, (int)p1.Y), new Point((int)p2.X, (int)p2.Y), Scalar.Green, 2);
                }

                // Draw text
                Cv2.PutText(frame, $"QR: {code}", new Point(10, 30),
                    HersheyFonts.HersheySimplex, 0.6, Scalar.Blue, 2);
            }

            window.ShowImage(frame);
            var key = Cv2.WaitKey(1);
            if (key == 'q') break;
        }
        Cv2.DestroyAllWindows();
    }

    public byte[]? GetJpegFrame()
    {
        const int cameraIndex = 0; // Change if you want to try a different camera
        Console.WriteLine($"[CameraService] Trying to open camera with index: {cameraIndex}");
        using var capture = new VideoCapture(cameraIndex);
        using var frame = new Mat();
        capture.Read(frame);
        if (frame.Empty())
        {
            Console.WriteLine("[CameraService] Failed to capture frame from camera.");
            return null;
        }
        else
        {
            Console.WriteLine(frame);
        }
        return frame.ToBytes(".jpg");
    }

    public string? GetJpegFrameBase64()
    {
        var bytes = GetJpegFrame();
        return bytes != null ? Convert.ToBase64String(bytes) : null;
    }
}

public class CompartmentQrData
{
    public string MedicijnNaam { get; set; } = "";
    public string Dosis { get; set; } = "";
    public int Voorraad { get; set; }
    public List<string> DoseringstijdenPerDag { get; set; } = new();
}