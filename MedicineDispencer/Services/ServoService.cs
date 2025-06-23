using System;
using System.Device.Gpio;
using System.Device.Pwm;
using System.Device.Pwm.Drivers;
using System.Threading.Tasks;

public static class ServoService
{
    private const int SERVOPIN = 15; // GPIO15 (BCM)
    private static SoftwarePwmChannel? pwmChannel;
    private static bool initialized = false;
    public static bool simulate { get; private set; } = false;

    static ServoService()
    {
        try
        {
            Console.WriteLine("Initializing Servo Service with Software PWM...");
            pwmChannel = new SoftwarePwmChannel(SERVOPIN, frequency: 50, dutyCycle: 0.0, usePrecisionTimer: true);
            pwmChannel.Start();
            SetAngle(0); // Beginpositie
            initialized = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ServoService initialization failed: {ex.Message}");
            simulate = true;
            initialized = false;
        }
    }

    public static void Open() => SetAngle(0);     // Open positie (pas aan indien nodig)
    public static void Close() => SetAngle(90);   // Sluitpositie (pas aan indien nodig)

    public static async Task Dispense()
    {
        Open();
        await Task.Delay(1000); // 1 seconde open
        Close();
    }

    private static void SetAngle(double angle)
    {
        if (simulate)
        {
            Console.WriteLine($"[SIMULATIE] Servo naar {angle}°");
            return;
        }

        if (!initialized || pwmChannel == null)
        {
            Console.WriteLine("PWM niet geïnitialiseerd");
            return;
        }

        // Typische servo: 2% (0°) tot 12% (180°) duty cycle
        double dutyCycle = 2.0 + (angle / 18.0); // ≈ 2% - 12% duty
        pwmChannel.DutyCycle = dutyCycle / 100.0;

        Console.WriteLine($"Servo naar {angle}°");

        // Kleine vertraging voor beweging
        Task.Delay(500).Wait();
    }

    public static void Stop()
    {
        if (simulate) return;
        pwmChannel?.Stop();
    }

    public static void Dispose()
    {
        if (simulate) return;
        Close();
        Stop();
        pwmChannel?.Dispose();
    }
}
