using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NameSelector : MonoBehaviour
{
    [SerializeField] TMP_InputField nameField;
    [SerializeField] Button connectButton;
    [SerializeField] int minNameLength = 3;
    [SerializeField] int maxNameLength = 12;

    public const string playerNameKey = "PlayerName";

    void Start()
    {
        if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            return;
        }

        nameField.text = PlayerPrefs.GetString(playerNameKey, string.Empty);
        HandleNameChanged();
    }

    public void HandleNameChanged()
    {
        connectButton.interactable =
            nameField.text.Length >= minNameLength && nameField.text.Length <= maxNameLength;
    }

    public void Connect()
    {
        PlayerPrefs.SetString(playerNameKey, nameField.text);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
