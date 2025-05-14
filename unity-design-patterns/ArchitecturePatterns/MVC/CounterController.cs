using UnityEngine;

public class CounterController : MonoBehaviour
{
    [SerializeField] private CounterView view;

    private CounterModel model;

    void Start()
    {
        model = new CounterModel();
        view.addButton.onClick.AddListener(OnAdd);
        view.UpdateView(model.Count);
    }

    void OnAdd()
    {
        model.Add(1);
        view.UpdateView(model.Count);
    }
}
