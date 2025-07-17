using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text firstName;
    [SerializeField] private TMP_Text secondName;
    [SerializeField] private TMP_Text thirdName;
    [SerializeField] private TMP_Text fourthName;

    public void ShowResults()
    {
        for (int i = 0; i < PlayerRanks.instance.playerNames.Count; i++)
        {
            switch (i)
            {
                case 0:
                    firstName.text = PlayerRanks.instance.playerNames[i].Value.ToString();
                    break;
                case 1:
                    secondName.text = PlayerRanks.instance.playerNames[i].Value.ToString();
                    break;
                case 2:
                    thirdName.text = PlayerRanks.instance.playerNames[i].Value.ToString();
                    break;
                case 3:
                    fourthName.text = PlayerRanks.instance.playerNames[i].Value.ToString();
                    break;
            }
        }
    }
}