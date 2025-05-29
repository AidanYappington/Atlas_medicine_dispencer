using System;
using System.Device.Gpio;

public static class LEDService
{
    public static bool simulate { get; private set; } = false;
    public static bool ledState { get; private set; } = false;

    private const int LEDPIN = 14;
    private static GpioController? gpioController;
    private static bool initialized = false;

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
        // Console.WriteLine("Forcing LEDService to simulate mode.");
        // simulate = true;
        // initialized = false;
    }

    public static void TurnOn()
    {
        ledState = true;
        if (simulate)
        {
            if (ledState) return;
            Console.WriteLine("Simulated LED ON");
            return;
        }
        if (!initialized || ledState) return;
        gpioController!.Write(LEDPIN, PinValue.High);
    }

    public static void TurnOff()
    {
        ledState = false;
        if (simulate)
        {
            if (!ledState) return;
            Console.WriteLine("Simulated LED OFF");
            return;
        }
        if (!initialized || !ledState) return;
        
        gpioController!.Write(LEDPIN, PinValue.Low);
    }

    public static void Dispose()
    {
        // Zet alle pins op LOW
        TurnOff();
    }
}