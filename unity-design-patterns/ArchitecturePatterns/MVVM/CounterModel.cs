public class CounterModel
{
    public int Count { get; private set; }

    public void Add(int value)
    {
        Count += value;
    }
}
