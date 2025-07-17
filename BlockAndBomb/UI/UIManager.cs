using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] Image Bomb;
    [SerializeField] Image Dirt;
    [SerializeField] Image Stone;

    [SerializeField] Sprite Selection;
    [SerializeField] Sprite NonSelection;

    [SerializeField] TMP_Text BombCountText;
    [SerializeField] TMP_Text DirtCountText;
    [SerializeField] TMP_Text StoneCountText;

    [SerializeField] Image expImage;
    [SerializeField] TMP_Text levelText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void UpdateSelection(InstallableType installableType)
    {
        Bomb.sprite = NonSelection;
        Dirt.sprite = NonSelection;
        Stone.sprite = NonSelection;

        switch (installableType)
        {
            case InstallableType.Bomb:
                Bomb.sprite = Selection;
                break;
            case InstallableType.Block_Dirt:
                Dirt.sprite = Selection;
                break;
            case InstallableType.Block_Stone:
                Stone.sprite = Selection;
                break;
        }
    }

    public void UpdateResourceUI(int currentBomb, int maxBomb, int dirt, int stone)
    {
        BombCountText.text = $"{currentBomb} / {maxBomb}";
        DirtCountText.text = dirt.ToString();
        StoneCountText.text = stone.ToString();
    }

    public void UpdateExpImage(int currentExp, int requiredExp)
    {
        expImage.fillAmount = (float)currentExp / requiredExp;
    }

    public void UpdateLevelText(int level, int currentExp, int requiredExp)
    {
        levelText.text = $"Level{level} ({currentExp}/{requiredExp})";
    }
}
