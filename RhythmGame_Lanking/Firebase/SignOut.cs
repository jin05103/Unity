using UnityEngine;
using Firebase.Auth;

public class SignOut : MonoBehaviour
{
    public void OnSignOutButtonClicked()
    {
        FirebaseManager.Instance.SignOut();
    }
}
