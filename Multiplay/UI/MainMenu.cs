using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Core;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private TMP_InputField joinCodeField;

    private void Start()
    {
        UnityServices.InitializeAsync();
    }

    public async void StartHost()
    {
        await HostSingleton.Instance.GameManager.StartHostAsync();
    }

    public async void StartClient()
    {
        await ClientSingleton.Instance.GameManager.StartClientAsync(joinCodeField.text);
    }
}
