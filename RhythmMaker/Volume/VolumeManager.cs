using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VolumeManager : MonoBehaviour
{
    [SerializeField] Slider volumeSlider;
    [SerializeField] TMP_Text volumeText;

    private void Start()
    {
        volumeSlider.value = PlayerPrefs.GetInt("Volume", 100);
        volumeText.text = volumeSlider.value.ToString();
        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    public void SetVolume(float volume)
    {
        volumeText.text = volume.ToString();
        PlayerPrefs.SetInt("Volume", (int)volume);
        PlayerPrefs.Save();
    }
}
