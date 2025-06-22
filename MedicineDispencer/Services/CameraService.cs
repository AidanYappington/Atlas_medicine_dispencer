using System;
using System.Collections.Generic;
using System.Text.Json;
using OpenCvSharp;
using ZXing;
using ZXing.Windows.Compatibility;
using System.Diagnostics;
using System.IO;
using System.Threading;

public class CameraService
{
    private Process? _backgroundProcess;
    private string _framePath = "/tmp/cam.jpg";
    private Timer? _monitorTimer;

    public CameraService() { }

    /// <summary>
    /// Starts the background camera process if not already running.
    /// </summary>
    public void StartCamera()
    {
        if (_backgroundProcess != null && !_backgroundProcess.HasExited)
            return;

        StopCamera();

        // Use a shell loop to call libcamera-jpeg every 0.2 seconds (5 fps)
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
}

public class CompartmentQrData
{
    public string MedicijnNaam { get; set; } = "";
    public string Dosis { get; set; } = "";
    public int Voorraad { get; set; }
    public List<string> DoseringstijdenPerDag { get; set; } = new();
}