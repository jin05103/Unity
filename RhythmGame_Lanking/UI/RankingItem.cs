using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RankingItem : MonoBehaviour
{
    [SerializeField] TMP_Text rankText;
    [SerializeField] TMP_Text nickNameText;
    [SerializeField] TMP_Text rateText;
    [SerializeField] Image   highlightBackground;

    public void SetData(int rank, string nickname, float rate, bool isCurrentUser)
    {
        rankText.text     = rank.ToString();
        nickNameText.text = nickname;
        rateText.text     = rate.ToString("F2");
        highlightBackground.gameObject.SetActive(isCurrentUser);
    }
}