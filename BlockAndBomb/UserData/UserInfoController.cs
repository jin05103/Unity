using UnityEngine;

public class UserInfoController : MonoBehaviour
{
    [SerializeField] UserInfoView userInfoView;

    void Start()
    {
        InitializeUserInfo();
        FirebaseManager.Instance.OnUserDataChanged += OnUserDataChanged;
    }

    void OnDestroy()
    {
        if (FirebaseManager.Instance != null)
            FirebaseManager.Instance.OnUserDataChanged -= OnUserDataChanged;
    }

    void InitializeUserInfo()
    {
        var userData = FirebaseManager.Instance.CurrentUserData;
        if (userData != null)
            userInfoView.SetUserInfo(userData.Nickname, userData.Wins, userData.Losses);
        else
            userInfoView.ResetView();
    }

    void OnUserDataChanged(UserData data)
    {
        userInfoView.SetUserInfo(data.Nickname, data.Wins, data.Losses);
    }
}
