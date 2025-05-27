using System;
using System.Device.Gpio;

public static class LEDService
{
    private static bool simulate = false;
    private static bool ledState = false;

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
    }

    public static void TurnOn()
    {
        if (simulate)
        {
            if (ledState) return;
            ledState = true;
            Console.WriteLine("Simulated LED ON");
            return;
        }
        if (!initialized) return;
        gpioController!.Write(LEDPIN, PinValue.High);
    }

    public static void TurnOff()
    {
        if (simulate)
        {
            if (!ledState) return;
            ledState = false;
            Console.WriteLine("Simulated LED OFF");
            return;
        }
        if (!initialized) return;
        gpioController!.Write(LEDPIN, PinValue.Low);
    }
}