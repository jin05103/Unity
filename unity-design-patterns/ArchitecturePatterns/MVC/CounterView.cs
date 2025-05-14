using UnityEngine;
using UnityEngine.UI;

public class CounterView : MonoBehaviour
{
    public Text countText;
    public Button addButton;
    
    public void UpdateView(int count)
    {
        countText.text = $"Count: {count}";
    }
}
