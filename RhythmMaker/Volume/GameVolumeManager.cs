using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameVolumeManager : MonoBehaviour
{
    [SerializeField] Slider volumeSlider;
    [SerializeField] TMP_Text volumeText;
    [SerializeField] AudioSource audioSource;

    private void Start()
    {
        volumeSlider.value = PlayerPrefs.GetInt("Volume", 100);
        audioSource.volume = volumeSlider.value / 100f;
        volumeText.text = volumeSlider.value.ToString();
        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    public void SetVolume(float volume)
    {
        audioSource.volume = volume / 100f;
        volumeText.text = volume.ToString();
        PlayerPrefs.SetInt("Volume", (int)volume);
        PlayerPrefs.Save();
    }
}
