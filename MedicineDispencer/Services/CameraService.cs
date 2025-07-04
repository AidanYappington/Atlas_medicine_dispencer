using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using ZXing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Image = SixLabors.ImageSharp.Image;
using MedicineDispencer.Components.Pages;

public class CameraService : IDisposable
{
    private Process? _backgroundProcess;
    private string _framePath = "/tmp/cam.jpg";
    private Timer? _monitorTimer;
    private Timer? _qrTimer;
    private string? _lastQrResult;

    public CameraService() { }

    /// <summary>
    /// Starts the background camera process if not already running.
    /// </summary>
    public void StartCamera()
    {
        if (_backgroundProcess != null && !_backgroundProcess.HasExited)
            return;

        StopCamera();

        // Start camera process
        var psi = new ProcessStartInfo
        {
            FileName = "/bin/bash",
            Arguments = $"-c \"while true; do libcamera-jpeg -o {_framePath} --width 1920 --height 1080 --quality 95 --nopreview --timeout 100 > /dev/null 2>&1; sleep 0.2; done\"",
            RedirectStandardOutput = false,
            RedirectStandardError = false,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        _backgroundProcess = Process.Start(psi);

        // Optionally monitor the process and restart if it dies
        _monitorTimer = new Timer(_ =>
        {
            if (_backgroundProcess == null || _backgroundProcess.HasExited)
            {
                StartCamera();
            }
        }, null, 5000, 5000);

        // Start QR timer
        _qrTimer = new Timer(async _ =>
        {
            var qr = TryDecodeQrFromFrame();
            if (!string.IsNullOrEmpty(qr) && qr != _lastQrResult)
            {
                _lastQrResult = qr;
                Console.WriteLine($"[CameraService] QR found: {_lastQrResult}");

                try
                {
                    // Parse QR as CompartmentQrData (same as SimpleMedicijnCompartiment)
                    var qrData = System.Text.Json.JsonSerializer.Deserialize<CompartmentQrData>(_lastQrResult);
                    if (qrData != null)
                    {
                        var tijden = qrData.DoseringstijdenPerDag.Select(t => TimeSpan.Parse(t)).ToList();
                        var newCompartment = new MedicijnCompartiment(
                            qrData.MedicijnNaam,
                            qrData.Dosis,
                            qrData.Voorraad,
                            tijden
                        );

                        if (DataService.AddToFirstEmpty(newCompartment))
                        {
                            Console.WriteLine("[CameraService] Compartment added from QR.");
                        }
                        else
                        {
                            Console.WriteLine("[CameraService] No empty compartment slot available.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[CameraService] Failed to add compartment from QR: {ex.Message}");
                }
            }
        }, null, 0, 1000); // every 1 second
    }

    /// <summary>
    /// Stops the background camera process.
    /// </summary>
    public void StopCamera()
    {
        try
        {
            _backgroundProcess?.Kill(true);
            _backgroundProcess?.Dispose();
            _backgroundProcess = null;
        }
        catch { }
        _monitorTimer?.Dispose();
        _monitorTimer = null;
        _qrTimer?.Dispose();
        _qrTimer = null;
    }

    /// <summary>
    /// Gets the latest JPEG frame from the background process.
    /// </summary>
    public byte[]? GetJpegFrame()
    {
        try
        {
            if (File.Exists(_framePath))
            {
                return File.ReadAllBytes(_framePath);
            }
            else
            {
                Console.WriteLine("[CameraService] No frame file found.");
                return null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CameraService] Error reading frame: {ex.Message}");
            return null;
        }
    }

    public string? GetJpegFrameBase64()
    {
        var bytes = GetJpegFrame();
        return bytes != null ? Convert.ToBase64String(bytes) : null;
    }

    public string? TryDecodeQrFromFrame()
    {
        try
        {
            if (!File.Exists(_framePath))
                return null;

            using var image = Image.Load<Rgba32>(_framePath);
            var pixels = new byte[image.Width * image.Height * 4];
            image.CopyPixelDataTo(pixels);

            var luminanceSource = new ZXing.RGBLuminanceSource(
                pixels, image.Width, image.Height, ZXing.RGBLuminanceSource.BitmapFormat.RGB32);

            var barcodeReader = new ZXing.BarcodeReader
            {
                AutoRotate = true,
                Options = { TryHarder = true, PossibleFormats = new List<ZXing.BarcodeFormat> { ZXing.BarcodeFormat.QR_CODE } }
            };
            var result = barcodeReader.Decode(luminanceSource);
            return result?.Text;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CameraService] QR decode error: {ex.Message}");
            return null;
        }
    }

    // Optionally, add a getter for the last QR result:
    public string? GetLastQrResult() => _lastQrResult;

    public void Dispose()
    {
        StopCamera();
        _backgroundProcess?.Dispose();
        _monitorTimer?.Dispose();
        _qrTimer?.Dispose();
    }
}    

public class CompartmentQrData
{
    public string MedicijnNaam { get; set; } = "";
    public string Dosis { get; set; } = "";
    public int Voorraad { get; set; }
    public List<string> DoseringstijdenPerDag { get; set; } = new();
}