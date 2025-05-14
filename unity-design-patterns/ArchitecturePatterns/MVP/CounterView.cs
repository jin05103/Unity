using UnityEngine;
using UnityEngine.UI;

public class CounterView : MonoBehaviour, ICounterView
{
    public Text countText;
    public Button addButton;

    private CounterPresenter presenter;

    void Start()
    {
        presenter = new CounterPresenter(this);
    }

    public void SetCountText(string text)
    {
        countText.text = text;
    }

    public void SetOnClick(System.Action callback)
    {
        addButton.onClick.AddListener(() => callback.Invoke());
    }
}
