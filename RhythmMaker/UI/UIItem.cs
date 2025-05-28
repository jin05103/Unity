using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class UIItem : MonoBehaviour
{
    [SerializeField] TMP_Text titleText;
    [SerializeField] TMP_Text tagText;
    [SerializeField] TMP_Text maxComboText;
    [SerializeField] TMP_Text maxPercentageText;
    [SerializeField] Button playButton;

    public void SetData(ABC abc, LoadManager loadManager)
    {
        titleText.text = abc.songFileInfo.songInfo.title;
        tagText.text = abc.songFileInfo.songInfo.tag;
        maxComboText.text = "Max Combo: " + abc.songFileInfo.gameStats.maxCombo;
        maxPercentageText.text = "Max Rate: " + abc.songFileInfo.gameStats.maxPercentage.ToString("F2");


        playButton.onClick.AddListener(() =>
        {
            if (loadManager.canClick == false)
                return;
            // GameManager.songFileInfo = abc.songFileInfo;
            GameManager.abc = abc;

            SceneManager.LoadScene("Game");
        });
    }
}
