using UnityEngine;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Storage;
using UnityEngine.SceneManagement;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance { get; private set; }
    public FirebaseAuth Auth { get; private set; }
    public FirebaseUser User { get; private set; }
    public FirebaseFirestore Firestore { get; private set; }
    public FirebaseStorage Storage { get; private set; }

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
        Storage = FirebaseStorage.DefaultInstance;
    }

    void OnDestroy()
    {
        Auth = null;
        Firestore = null;
        Storage = null;
    }

    public void SignIn(FirebaseUser user)
    {
        User = user;
        SceneManager.LoadScene("Main");
    }

    public void SignOut()
    {
        Auth.SignOut();
        User = null;
        SceneManager.LoadScene("Sign");
    }

    public StorageReference GetStorageRef()
    {
        return Storage.GetReferenceFromUrl("gs://rhythmmaker-1f978.firebasestorage.app");
    }
}