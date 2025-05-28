using UnityEngine;

public class SelectUIManager : MonoBehaviour
{
    [SerializeField] GameObject escPanel;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            escPanel.SetActive(!escPanel.activeSelf);
        }
    }

    public void ClosePanel()
    {
        escPanel.SetActive(false);
    }
}
