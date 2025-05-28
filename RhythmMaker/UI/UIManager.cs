using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] GameObject escPanel;

    [SerializeField] GameObject comboPanel;
    [SerializeField] GameObject ratePanel;
    [SerializeField] GameObject verdictPanel;

    [SerializeField] GameObject endText;

    [SerializeField] TMP_Text comboText;
    [SerializeField] TMP_Text rateText;
    [SerializeField] TMP_Text verdictText;
    IEnumerator hideComboPanelCoroutine;
    IEnumerator hideRatePanelCoroutine;
    IEnumerator hideVerdictPanelCoroutine;

    [SerializeField] float hideTime = 0.12f;
    [SerializeField] float hideRate = 0.8f;

    public void Init()
    {
        endText.SetActive(false);
        escPanel.SetActive(false);
        comboPanel.GetComponent<CanvasGroup>().alpha = 0;
        ratePanel.GetComponent<CanvasGroup>().alpha = 0;
        verdictPanel.GetComponent<CanvasGroup>().alpha = 0;
        comboText.text = "";
        rateText.text = "";
        verdictText.text = "";
    }

    public void ShowEscPanel()
    {
        escPanel.SetActive(true);
    }

    public void HideEscPanel()
    {
        escPanel.SetActive(false);
    }

    public void ShowComboPanel(int combo)
    {
        comboText.text = combo.ToString();
        comboPanel.GetComponent<CanvasGroup>().alpha = 1;

        if (hideComboPanelCoroutine != null)
        {
            StopCoroutine(hideComboPanelCoroutine);
        }

        hideComboPanelCoroutine = HideComboPanel();
        StartCoroutine(hideComboPanelCoroutine);
    }

    IEnumerator HideComboPanel()
    {
        yield return new WaitForSeconds(hideTime);

        while (comboPanel.GetComponent<CanvasGroup>().alpha > 0)
        {
            comboPanel.GetComponent<CanvasGroup>().alpha -= Time.deltaTime * hideRate;
            yield return null;
        }
    }

    public void ShowRatePanel(float rate)
    {
        rateText.text = rate.ToString("F2") + "%";
        ratePanel.GetComponent<CanvasGroup>().alpha = 1;

        if (hideRatePanelCoroutine != null)
        {
            StopCoroutine(hideRatePanelCoroutine);
        }

        hideRatePanelCoroutine = HideRatePanel();
        StartCoroutine(hideRatePanelCoroutine);
    }

    IEnumerator HideRatePanel()
    {
        yield return new WaitForSeconds(hideTime);

        while (ratePanel.GetComponent<CanvasGroup>().alpha > 0)
        {
            ratePanel.GetComponent<CanvasGroup>().alpha -= Time.deltaTime * hideRate;
            yield return null;
        }
    }

    public void ShowVerdictPanel(string verdict)
    {
        verdictText.text = verdict;
        verdictPanel.GetComponent<CanvasGroup>().alpha = 1;

        if (hideVerdictPanelCoroutine != null)
        {
            StopCoroutine(hideVerdictPanelCoroutine);
        }

        hideVerdictPanelCoroutine = HideVerdictPanel();
        StartCoroutine(hideVerdictPanelCoroutine);
    }

    IEnumerator HideVerdictPanel()
    {
        yield return new WaitForSeconds(hideTime);

        while (verdictPanel.GetComponent<CanvasGroup>().alpha > 0)
        {
            verdictPanel.GetComponent<CanvasGroup>().alpha -= Time.deltaTime * hideRate;
            yield return null;
        }
    }

    public void ShowEndPanel()
    {
        endText.SetActive(true);
        comboPanel.GetComponent<CanvasGroup>().alpha = 1;
        ratePanel.GetComponent<CanvasGroup>().alpha = 1;
    }
}
