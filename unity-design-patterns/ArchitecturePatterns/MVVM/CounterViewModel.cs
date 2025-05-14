using System;

public class CounterViewModel
{
    public int Count { get; private set; }

    public event Action<int> OnCountChanged;

    public void Add()
    {
        Count += 1;
        OnCountChanged?.Invoke(Count);
    }
}
