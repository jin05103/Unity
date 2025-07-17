using Firebase.Firestore;

[FirestoreData]
public class UserData
{
    [FirestoreProperty] public string Nickname { get; set; }
    [FirestoreProperty] public string Email { get; set; }

    [FirestoreProperty] public int Rank { get; set; }
    [FirestoreProperty] public int Wins { get; set; }
    [FirestoreProperty] public int Losses { get; set; }
    [FirestoreProperty] public int Coins { get; set; }
    [FirestoreProperty] public int Gems { get; set; }

    [FirestoreProperty] public bool IsNetworkAuthenticated { get; set; } = false;

    [FirestoreProperty] public Timestamp CreatedAt { get; set; }
}
