using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ConnectionButtons : MonoBehaviour
{
    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
    }

    public void StartJoin()
    {
        NetworkManager.Singleton.StartClient();
    }
}
