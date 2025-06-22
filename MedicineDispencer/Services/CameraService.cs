using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Drawing; // For Bitmap
using ZXing;         // For BarcodeReader and BarcodeFormat
using ZXing.CoreCompat.System.Drawing;

public class CameraService
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
            Arguments = $"-c \"while true; do libcamera-jpeg -o {_framePath} --width 640 --height 480 --nopreview --timeout 50 > /dev/null 2>&1; sleep 0.2; done\"",
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
        _qrTimer = new Timer(_ =>
        {
            if (_backgroundProcess != null && !_backgroundProcess.HasExited)
            {
                _lastQrResult = TryDecodeQrFromFrame();
                if (!string.IsNullOrEmpty(_lastQrResult))
                    Console.WriteLine($"[CameraService] QR found: {_lastQrResult}");
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

    /// <summary>
    /// Tries to decode QR code from the latest frame.
    /// </summary>
    public string? TryDecodeQrFromFrame()
    {
        try
        {
            if (!File.Exists(_framePath))
                return null;

            using var bitmap = new System.Drawing.Bitmap(_framePath);
            var reader = new ZXing.BarcodeReader<System.Drawing.Bitmap>(bmp => new ZXing.CoreCompat.System.Drawing.BitmapLuminanceSource(bmp));
            reader.Options.TryHarder = true;
            reader.Options.PossibleFormats = new List<ZXing.BarcodeFormat> { ZXing.BarcodeFormat.QR_CODE };
            reader.AutoRotate = true;

            var result = reader.Decode(bitmap);
            Console.WriteLine($"[CameraService] QR decode result: {result?.Text}");
            return result?.Text;
        }
        catch (DllNotFoundException ex)
        {
            Console.WriteLine($"[CameraService] System.Drawing dependency missing: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CameraService] QR decode error: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Gets the last detected QR code (if any).
    /// </summary>
    public string? GetLastQrResult() => _lastQrResult;
}

public class CompartmentQrData
{
    public string MedicijnNaam { get; set; } = "";
    public string Dosis { get; set; } = "";
    public int Voorraad { get; set; }
    public List<string> DoseringstijdenPerDag { get; set; } = new();
}