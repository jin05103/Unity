using UnityEngine;
using Firebase.Auth;
using Firebase.Firestore;
using TMPro;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class AuthManager : MonoBehaviour
{
    [SerializeField] TMP_InputField SignInEmail;
    [SerializeField] TMP_InputField SignInPassword;
    [SerializeField] Button SignInPanelSignInButton;
    [SerializeField] Button SignInPanelSignUpButton;
    [SerializeField] TMP_InputField SignUpID;
    [SerializeField] TMP_InputField SignUpEmail;
    [SerializeField] TMP_InputField SignUpPassword;
    [SerializeField] TMP_InputField SignUpConfirmPassword;
    [SerializeField] Button SignUpPanelSignUpButton;
    [SerializeField] Button SignUpPanelCancelButton;
    [SerializeField] GameObject SignInPanel;
    [SerializeField] GameObject SignUpPanel;
    [SerializeField] GameObject messagePanel;
    [SerializeField] Button messagePanelCloseButton;
    [SerializeField] TMP_Text messageText;

    FirebaseAuth auth;
    FirebaseFirestore db;

    private void Start()
    {
        auth = FirebaseManager.Instance.Auth;
        db = FirebaseManager.Instance.Firestore;
        ShowLoginPanel();
        ClearInputFields();
        SignInPanelSignInButton.onClick.AddListener(BtnSignInPanelSignIn);
        SignInPanelSignUpButton.onClick.AddListener(BtnSignInPanelSignUp);
        SignUpPanelSignUpButton.onClick.AddListener(BtnSignUpPanelSignUp);
        SignUpPanelCancelButton.onClick.AddListener(BtnSignUpPanelCancel);
        messagePanel.SetActive(false);
        messageText.text = "";
        messagePanelCloseButton.onClick.AddListener(BtnCloseMessagePanel);
    }


    async Task<bool> SignIn()
    {
        try
        {
            var result = await auth.SignInWithEmailAndPasswordAsync(SignInEmail.text, SignInPassword.text);
            Debug.LogFormat("User signed in successfully: {0} ({1})", result.User.DisplayName, result.User.UserId);

            FirebaseManager.Instance.SetCurrentUser(result.User);
            DocumentReference userDoc = db.Collection("users").Document(result.User.UserId);
            UserData userData = await userDoc.GetSnapshotAsync().ContinueWith(task =>
            {
                if (task.IsCompleted && task.Result.Exists)
                {
                    return task.Result.ConvertTo<UserData>();
                }
                return null;
            });

            bool isUserDataLoaded = await FirebaseManager.Instance.FetchCurrentUserData();
            if (!isUserDataLoaded)
            {
                messageText.text = "Failed to load user data.";
                messagePanel.SetActive(true);
                return false;
            }

            if (!FirebaseManager.Instance.CurrentUserData.IsNetworkAuthenticated)
            {
                bool IsNetworkAuthenticated = await NetworkBootstrap.Instance.SignUpWithUsernamePasswordAsync(result.User.UserId, SignUpPassword.text);
                if (!IsNetworkAuthenticated)
                {
                    messageText.text = "Failed to authenticate with the network.";
                    messagePanel.SetActive(true);
                    return false;
                }
                FirebaseManager.Instance.CurrentUserData.IsNetworkAuthenticated = true;

                await userDoc.SetAsync(FirebaseManager.Instance.CurrentUserData, SetOptions.MergeAll);
            }

            bool success = await NetworkBootstrap.Instance.SignInWithUsernamePasswordAsync(userData.Nickname, SignInPassword.text);
            if (!success)
            {
                messageText.text = "Failed to sign in to the network.";
                messagePanel.SetActive(true);
                return false;
            }

            return true;
        }
        catch (System.Exception ex)
        {
            FirebaseManager.Instance.SetCurrentUser(null);
            messageText.text = ex.Message;
            messagePanel.SetActive(true);
            return false;
        }
    }

    async Task<bool> SignUp()
    {
        bool isValidNickname = IsValidNickname(SignUpID.text);
        if (!isValidNickname)
        {
            Debug.LogError("Invalid nickname");
            messageText.text = "Invalid nickname. It must be 3-20 characters long and can only contain letters, digits, '.', '-', '_', or '@'.";
            messagePanel.SetActive(true);
            return false;
        }

        bool isValidPassword = IsValidPassword(SignUpPassword.text);
        if (!isValidPassword)
        {
            Debug.LogError("Invalid password");
            messageText.text = "Invalid password. It must be 8-30 characters long and contain at least one uppercase letter, one lowercase letter, one digit, and one special character.";
            messagePanel.SetActive(true);
            return false;
        }

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
            var userData = new UserData
            {
                Nickname = SignUpID.text,
                Email = SignUpEmail.text,
                Rank = 0,
                Wins = 0,
                Losses = 0,
                Coins = 0,
                Gems = 0,
                IsNetworkAuthenticated = false,
                CreatedAt = Timestamp.GetCurrentTimestamp()
            };
            await userDoc.SetAsync(userData);


            bool success = await NetworkBootstrap.Instance.SignUpWithUsernamePasswordAsync(SignUpID.text, SignUpPassword.text);
            if (success)
            {
                userData.IsNetworkAuthenticated = true;
            }
            else
            {
                userData.IsNetworkAuthenticated = false;
            }

            await userDoc.SetAsync(userData, SetOptions.MergeAll);
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

    bool IsValidNickname(string nickname)
    {
        if (string.IsNullOrEmpty(nickname)) return false;
        if (nickname.Length < 3 || nickname.Length > 20) return false;

        foreach (char c in nickname)
        {
            if (!(char.IsLetterOrDigit(c) || c == '.' || c == '-' || c == '_' || c == '@'))
                return false;
        }
        return true;
    }

    bool IsValidPassword(string password)
{
    if (string.IsNullOrEmpty(password)) return false;
    if (password.Length < 8 || password.Length > 30) return false;
    bool hasUpper = Regex.IsMatch(password, "[A-Z]");
    bool hasLower = Regex.IsMatch(password, "[a-z]");
    bool hasDigit = Regex.IsMatch(password, @"\d");
    bool hasSymbol = Regex.IsMatch(password, @"[!@#$%^&*(),.?:{}|<>_\-\[\]\\\/+=;']");

    return hasUpper && hasLower && hasDigit && hasSymbol;
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
        SignInPanelSignInButton.interactable = false;
        SignInPanelSignUpButton.interactable = false;

        bool success = await SignIn();

        SignInPanelSignInButton.interactable = true;
        SignInPanelSignUpButton.interactable = true;

        if (success)
        {
            ClearInputFields();
            Debug.Log("Login + UserData Load successful. Proceeding...");

            FirebaseManager.Instance.SignIn(auth.CurrentUser);
        }
    }

    public void BtnSignInPanelSignUp()
    {
        ClearInputFields();
        SignInPanel.SetActive(false);
        SignUpPanel.SetActive(true);
    }

    public async void BtnSignUpPanelSignUp()
    {
        SignUpPanelSignUpButton.interactable = false;
        SignUpPanelCancelButton.interactable = false;

        bool success = await SignUp();

        SignUpPanelSignUpButton.interactable = true;
        SignUpPanelCancelButton.interactable = true;

        if (success)
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
