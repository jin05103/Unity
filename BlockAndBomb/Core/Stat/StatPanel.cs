using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

public class StatPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text statPointText;
    [SerializeField] private GameObject statButtonPrefab;
    [SerializeField] private Transform statButtonParent;

    private Action<StatType> statSelectCallback;
    private List<Button> activeButtons = new List<Button>();
    private bool isActive = false;

    private void Update()
    {
        if (!isActive) return;
        // Z: 0, X: 1, C: 2
        if (Input.GetKeyDown(KeyCode.Z) && activeButtons.Count > 0)
            activeButtons[0].onClick.Invoke();
        else if (Input.GetKeyDown(KeyCode.X) && activeButtons.Count > 1)
            activeButtons[1].onClick.Invoke();
        else if (Input.GetKeyDown(KeyCode.C) && activeButtons.Count > 2)
            activeButtons[2].onClick.Invoke();
    }

    public void ShowStats(List<StatData> stats, Action<StatType> onStatSelected)
    {
        foreach (Transform child in statButtonParent)
            Destroy(child.gameObject);

        statSelectCallback = onStatSelected;
        activeButtons.Clear();

        string[] keyLabels = { "Z", "X", "C" };

        for (int i = 0; i < stats.Count; i++)
        {
            var stat = stats[i];
            var btnObj = Instantiate(statButtonPrefab, statButtonParent);
            btnObj.transform.Find("Icon").GetComponent<Image>().sprite = stat.icon;
            btnObj.transform.Find("Name").GetComponent<TMP_Text>().text = stat.statName;
            btnObj.transform.Find("Desc").GetComponent<TMP_Text>().text = $"{stat.description}\n<size=80%>(+{stat.incrementValue})</size>";

            if (i < keyLabels.Length)
            {
                btnObj.transform.Find("Image").transform.GetChild(0).GetComponent<TMP_Text>().text = keyLabels[i];
            }
            else
            {
                btnObj.transform.Find("Image").transform.GetChild(0).GetComponent<TMP_Text>().text = "";
            }

            var button = btnObj.GetComponent<Button>();
            button.interactable = true;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnStatButtonClicked(stat.statType));
            button.onClick.AddListener(() => button.interactable = false);

            activeButtons.Add(button);
        }

        gameObject.SetActive(true);
        isActive = true;
    }

    private void OnStatButtonClicked(StatType statType)
    {
        statSelectCallback?.Invoke(statType);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        isActive = false;
    }

    public TMP_Text GetStatPointText()
    {
        return statPointText;
    }
}