using System;
using System.Device.Gpio;
using System.Threading;

public static class ButtonService
{
    public static bool isPressed { get; private set; } = false;

    private const int BUTTONPIN = 18; // Change to your button's GPIO pin
    private static GpioController? gpioController;
    private static bool initialized = false;
    private static Timer? pollTimer;

    public static event Action? OnButtonPressed;

    static ButtonService()
    {
        try
        {
            Console.WriteLine("Initializing Button Service...");
            gpioController = new GpioController();
            gpioController.OpenPin(BUTTONPIN, PinMode.InputPullUp);
            initialized = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ButtonService initialization failed: {ex.Message}");
            initialized = false;
        }
    }

    public static void StartPolling()
    {
        pollTimer?.Dispose();
        pollTimer = new Timer(_ =>
        {
            bool pressed = false;
            if (initialized)
            {
                pressed = gpioController!.Read(BUTTONPIN) == PinValue.Low;
            }

            if (pressed && !isPressed)
            {
                isPressed = true;
                OnButtonPressed?.Invoke();
            }
            else if (!pressed && isPressed)
            {
                isPressed = false;
            }
        }, null, 0, 100); // Poll every 100ms
    }

    public static void StopPolling()
    {
        pollTimer?.Dispose();
        pollTimer = null;
    }

    public static void Dispose()
    {
        pollTimer?.Dispose();
        pollTimer = null;
        if (gpioController != null && gpioController.IsPinOpen(BUTTONPIN))
        {
            gpioController.ClosePin(BUTTONPIN);
            gpioController.Dispose();
            gpioController = null;
        }
    }
}