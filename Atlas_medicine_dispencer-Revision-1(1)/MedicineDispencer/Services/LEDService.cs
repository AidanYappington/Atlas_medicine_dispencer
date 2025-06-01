using System;
using System.Device.Gpio;
using System.Threading;

public static class LEDService
{
    public static bool simulate { get; private set; } = false;
    public static bool ledState { get; private set; } = false;

    private const int LEDPIN = 14;
    private static GpioController? gpioController;
    private static bool initialized = false;
    private static Timer? blinkTimer;
    private static bool blinkState = false;

    static LEDService()
    {
        try
        {
            Console.WriteLine("Initializing LED Service...");
            gpioController = new GpioController();
            gpioController.OpenPin(LEDPIN, PinMode.Output);
            gpioController.Write(LEDPIN, PinValue.Low);
            initialized = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"LEDService initialization failed: {ex.Message}");
            simulate = true;
            initialized = false;
        }
    }

    public static void TurnOn()
    {
        if (ledState) return;
        ledState = true;
        if (simulate)
        {
            Console.WriteLine("Simulated LED BLINK ON");
            StartBlink();
            return;
        }
        if (!initialized) return;
        StartBlink();
    }

    public static void TurnOff()
    {
        if (!ledState) return;
        ledState = false;
        if (simulate)
        {
            Console.WriteLine("Simulated LED BLINK OFF");
            StopBlink();
            return;
        }
        if (!initialized) return;
        StopBlink();
        gpioController!.Write(LEDPIN, PinValue.Low);
    }

    private static void StartBlink()
    {
        blinkTimer?.Dispose();
        blinkState = false;
        blinkTimer = new Timer(_ =>
        {
            blinkState = !blinkState;
            if (simulate)
            {
                Console.WriteLine($"Simulated LED {(blinkState ? "ON" : "OFF")}");
            }
            else if (initialized)
            {
                gpioController!.Write(LEDPIN, blinkState ? PinValue.High : PinValue.Low);
            }
        }, null, 0, 500); // Blink every 500ms
    }

    private static void StopBlink()
    {
        blinkTimer?.Dispose();
        blinkTimer = null;
        blinkState = false;
        if (!simulate && initialized)
        {
            gpioController!.Write(LEDPIN, PinValue.Low);
        }
    }

    public static void Dispose()
    {
        TurnOff();
        blinkTimer?.Dispose();
        blinkTimer = null;
    }
}