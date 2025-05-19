public class CounterState
{
    public int CurrentCount { get; set; }

    public void Inc()
    {
        CurrentCount++;
    }
}