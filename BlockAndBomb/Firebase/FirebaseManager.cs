using UnityEngine;
using Firebase.Auth;
using Firebase.Firestore;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance { get; private set; }
    public FirebaseAuth Auth { get; private set; }
    public FirebaseUser User { get; private set; }
    public FirebaseFirestore Firestore { get; private set; }

    public UserData CurrentUserData { get; private set; } = new UserData();

    public event Action<UserData> OnUserDataChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeFirebase();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeFirebase()
    {
        Auth = FirebaseAuth.DefaultInstance;
        Firestore = FirebaseFirestore.DefaultInstance;
    }

    void OnDestroy()
    {
        Auth = null;
        Firestore = null;
    }

    public void SetCurrentUser(FirebaseUser user)
    {
        User = user;
    }

    public void SignIn(FirebaseUser user)
    {
        User = user;
        SceneManager.LoadScene("MainMenu");
    }

    public void SignOut()
    {
        Auth.SignOut();
        User = null;
        NetworkBootstrap.Instance.SignOut();
        SceneManager.LoadScene("Login");
    }

    public async Task<bool> FetchCurrentUserData()
    {
        try
        {
            DocumentReference docRef = Firestore.Collection("users").Document(User.UserId);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
            if (snapshot.Exists)
            {
                CurrentUserData = snapshot.ConvertTo<UserData>();
                return true;
            }
            else
            {
                Debug.LogWarning("User document not found.");
                return false;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("FetchCurrentUserData Error: " + ex.Message);
            return false;
        }
    }

    public void SetCurrentUserData(UserData userData)
    {
        CurrentUserData = userData;
        OnUserDataChanged?.Invoke(CurrentUserData);
    }

    public async Task UpdateCurrentUserData(UserData newUserData)
    {
        SetCurrentUserData(newUserData);

        try
        {
            DocumentReference docRef = Firestore.Collection("users").Document(User.UserId);
            await docRef.SetAsync(CurrentUserData, SetOptions.MergeAll);
        }
        catch (Exception ex)
        {
            Debug.LogError("Firestore 저장 실패: " + ex.Message);
        }
    }
}