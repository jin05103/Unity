using UnityEngine;
using UnityEngine.UI;

public class CounterView : MonoBehaviour
{
    public Text countText;
    public Button addButton;

    private CounterViewModel viewModel;

    void Start()
    {
        viewModel = new CounterViewModel();
        viewModel.OnCountChanged += UpdateText;

        addButton.onClick.AddListener(() => viewModel.Add());

        UpdateText(viewModel.Count);
    }

    void UpdateText(int value)
    {
        countText.text = $"Count: {value}";
    }
}
