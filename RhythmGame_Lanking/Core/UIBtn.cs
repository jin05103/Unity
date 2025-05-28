using UnityEngine;

public class UIBtn : MonoBehaviour
{
    public void Resume()
    {
        GameManager.instance.ResumeGame();
    }

    public void Stop()
    {
        GameManager.instance.StopGame();
    }
}
