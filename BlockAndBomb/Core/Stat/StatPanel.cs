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

    public void ShowStats(List<StatData> stats, Action<StatType> onStatSelected)
    {
        foreach (Transform child in statButtonParent)
            Destroy(child.gameObject);

        statSelectCallback = onStatSelected;

        foreach (var stat in stats)
        {
            var btnObj = Instantiate(statButtonPrefab, statButtonParent);
            btnObj.transform.Find("Icon").GetComponent<Image>().sprite = stat.icon;
            btnObj.transform.Find("Name").GetComponent<TMP_Text>().text = stat.statName;
            btnObj.transform.Find("Desc").GetComponent<TMP_Text>().text = $"{stat.description}\n<size=80%>(+{stat.incrementValue})</size>";
            btnObj.GetComponent<Button>().onClick.AddListener(() => OnStatButtonClicked(stat.statType));
        }
        gameObject.SetActive(true);
    }

    private void OnStatButtonClicked(StatType statType)
    {
        statSelectCallback?.Invoke(statType);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public TMP_Text GetStatPointText()
    {
        return statPointText;
    }
}