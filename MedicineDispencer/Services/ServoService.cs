using System;
using System.Device.Gpio;
using System.Device.Pwm;
using System.Threading.Tasks;

public static class ServoService
{
    private const int SERVOPIN = 15;
    private static PwmChannel? pwmChannel;
    private static bool initialized = false;
    public static bool simulate { get; private set; } = false;

    static ServoService()
    {
        try
        {
            Console.WriteLine("Initializing Servo Service...");
            pwmChannel = PwmChannel.Create(0, 0, frequency: 50, dutyCyclePercentage: 0.05); // 50Hz for servo
            pwmChannel.Start();
            SetAngle(0);
            initialized = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ServoService initialization failed: {ex.Message}");
            simulate = true;
            initialized = false;
        }
    }

    public static void Open() => SetAngle(0);

    public static void Close() => SetAngle(190);

    public static async Task Dispense()
    {
        Open();
        await Task.Delay(1000);
        Close();
    }

    private static void SetAngle(double angle)
    {
        if (simulate)
        {
            Console.WriteLine($"Simulated servo angle: {angle}°");
            return;
        }
        if (!initialized || pwmChannel == null) return;

        double dutyCycle = 0.05 + (angle / 190.0) * 0.0528;
        pwmChannel.DutyCycle = dutyCycle;
        Console.WriteLine($"Servo angle set to {angle}° (duty: {dutyCycle:P})");
    }

    public static void Stop()
    {
        if (simulate) return;
        pwmChannel?.Stop();
    }

    public static void Dispose()
    {
        SetAngle(0);
        Stop();
        pwmChannel?.Dispose();
    }
}