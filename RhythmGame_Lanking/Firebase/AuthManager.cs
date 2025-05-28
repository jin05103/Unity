using UnityEngine;
using Firebase.Auth;
using Firebase.Firestore;
using TMPro;
using System.Threading.Tasks;
using System.Collections.Generic;

public class AuthManager : MonoBehaviour
{
    [SerializeField] TMP_InputField SignInEmail;
    [SerializeField] TMP_InputField SignInPassword;
    [SerializeField] TMP_InputField SignUpID;
    [SerializeField] TMP_InputField SignUpEmail;
    [SerializeField] TMP_InputField SignUpPassword;
    [SerializeField] TMP_InputField SignUpConfirmPassword;
    [SerializeField] GameObject SignInPanel;
    [SerializeField] GameObject SignUpPanel;
    [SerializeField] GameObject messagePanel;
    [SerializeField] TMP_Text messageText;

    FirebaseAuth auth;
    FirebaseFirestore db;

    private void Start()
    {
        auth = FirebaseManager.Instance.Auth;
        db = FirebaseManager.Instance.Firestore;
        ShowLoginPanel();
        ClearInputFields();
    }


    async Task<bool> SignIn()
    {
        try
        {
            var result = await auth.SignInWithEmailAndPasswordAsync(SignInEmail.text, SignInPassword.text);
            Debug.LogFormat("User signed in successfully: {0} ({1})", result.User.DisplayName, result.User.UserId);
            return true;
        }
        catch (System.Exception ex)
        {
            // Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + ex.Message);
            messageText.text = ex.Message;
            messagePanel.SetActive(true);
            return false;
        }
    }

    async Task<bool> SignUp()
    {
        if (SignUpPassword.text != SignUpConfirmPassword.text)
        {
            Debug.LogError("Passwords do not match");
            messageText.text = "Passwords do not match.";
            messagePanel.SetActive(true);
            return false;
        }

        var query = db.Collection("users").WhereEqualTo("nickname", SignUpID.text);
        var querySnapshot = await query.GetSnapshotAsync();
        if (querySnapshot.Count > 0)
        {
            Debug.LogError("ID already exists");
            messageText.text = "ID already exists.";
            messagePanel.SetActive(true);
            return false;
        }

        try
        {
            var result = await auth.CreateUserWithEmailAndPasswordAsync(SignUpEmail.text, SignUpPassword.text);

            await result.User.UpdateUserProfileAsync(new Firebase.Auth.UserProfile
            {
                DisplayName = SignUpID.text
            });

            DocumentReference userDoc = db.Collection("users").Document(result.User.UserId);
            var userData = new Dictionary<string, object>
        {
            { "nickname", SignUpID.text },
            { "email", SignUpEmail.text }
            // 필요에 따라 추가 필드들을 여기서 설정할 수 있습니다.
        };
            await userDoc.SetAsync(userData);

            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error: " + ex.Message);
            messageText.text = ex.Message;
            messagePanel.SetActive(true);
            return false;
        }
    }

    void ShowLoginPanel()
    {
        SignInPanel.SetActive(true);
        SignUpPanel.SetActive(false);
    }

    public void ClearInputFields()
    {
        SignInEmail.text = "";
        SignInPassword.text = "";
        SignUpID.text = "";
        SignUpEmail.text = "";
        SignUpPassword.text = "";
        SignUpConfirmPassword.text = "";
    }

    public async void BtnSignInPanelSignIn()
    {
        bool isLoggedIn = await SignIn();
        if (isLoggedIn)
        {
            ClearInputFields();
            Debug.Log("Login successful! Proceeding to game preparation...");

            FirebaseManager.Instance.SignIn(auth.CurrentUser);
        }
        // else
        // {
        //     messageText.text = "Login failed. Please check your email and password.";
        //     messagePanel.SetActive(true);
        // }
    }

    public void BtnSignInPanelSignUp()
    {
        ClearInputFields();
        SignInPanel.SetActive(false);
        SignUpPanel.SetActive(true);
    }

    public async void BtnSignUpPanelSignUp()
    {
        bool isSignedUp = await SignUp();
        if (isSignedUp)
        {
            SignUpPanel.SetActive(false);
            SignInPanel.SetActive(true);
            messageText.text = "Sign-up successful! Please log in.";
            messagePanel.SetActive(true);
        }
    }

    public void BtnSignUpPanelCancel()
    {
        ClearInputFields();
        SignInPanel.SetActive(true);
        SignUpPanel.SetActive(false);
    }

    public void BtnCloseMessagePanel()
    {
        messagePanel.SetActive(false);
    }
}
