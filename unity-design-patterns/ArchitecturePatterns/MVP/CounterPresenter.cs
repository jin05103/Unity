public class CounterPresenter
{
    private ICounterView view;
    private CounterModel model;

    public CounterPresenter(ICounterView view)
    {
        this.view = view;
        this.model = new CounterModel();

        view.SetOnClick(OnButtonClicked);
        UpdateView();
    }

    private void OnButtonClicked()
    {
        model.Add(1);
        UpdateView();
    }

    private void UpdateView()
    {
        view.SetCountText($"Count: {model.Count}");
    }
}
