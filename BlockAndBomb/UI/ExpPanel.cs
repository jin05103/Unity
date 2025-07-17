
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExpPanel : MonoBehaviour
{
    [SerializeField] Image expImage;
    [SerializeField] TMP_Text levelText;

    public Image GetExpImage()
    {
        return expImage;
    }

    public TMP_Text GetLevelText()
    {
        return levelText;
    }
}
