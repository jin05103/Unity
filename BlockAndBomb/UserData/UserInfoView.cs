using UnityEngine;
using TMPro;

public class UserInfoView : MonoBehaviour
{
    [SerializeField] TMP_Text IDText;
    [SerializeField] TMP_Text statsText;

    public void SetUserInfo(string nickname, int wins, int losses)
    {
        IDText.text = $"ID: {nickname}";
        statsText.text = $"W: {wins}  L: {losses}";
    }

    public void ResetView()
    {
        IDText.text = "ID: -";
        statsText.text = "W: 0  L: 0";
    }
}