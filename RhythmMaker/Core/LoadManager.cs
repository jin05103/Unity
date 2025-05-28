using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Newtonsoft.Json;
using System.Linq;
using System.Collections;

public class LoadManager : MonoBehaviour
{
    [SerializeField] Transform listParent;
    [SerializeField] GameObject listItemPrefab;

    [SerializeField] TMP_InputField searchInputField;
    [SerializeField] Button nextPageButton;
    [SerializeField] Button prevPageButton;
    [SerializeField] TMP_Text pageText;

    List<ABC> songFileList = new List<ABC>();
    List<ABC> filteredList = new List<ABC>();
    string jsonFolderPath;

    int currentPage = 0;
    public int itemsPerPage = 10;

    float time;
    public bool canClick;

    void Awake()
    {
        jsonFolderPath = Application.streamingAssetsPath + "/SheetList";
    }

    private void Start()
    {
        StartCoroutine(clickDelay());
        FindFiles();
        filteredList = songFileList.OrderBy(x => x.songFileInfo.songInfo.title).ToList();
        if (searchInputField != null)
            searchInputField.onValueChanged.AddListener(OnSearchChanged);

        if (nextPageButton != null)
            nextPageButton.onClick.AddListener(OnNextPage);
        if (prevPageButton != null)
            prevPageButton.onClick.AddListener(OnPrevPage);

        PopulateList();
    }

    IEnumerator clickDelay()
    {
        canClick = false;
        yield return new WaitForSeconds(0.2f);
        canClick = true;
    }

    private void FindFiles()
    {
        if (!Directory.Exists(jsonFolderPath))
        {
            Debug.LogError("폴더를 찾을 수 없습니다: " + jsonFolderPath);
            return;
        }

        // 하위 폴더 포함하여 검색
        string[] jsonFiles = Directory.GetFiles(jsonFolderPath, "*.json", SearchOption.AllDirectories);

        foreach (string filePath in jsonFiles)
        {
            string jsonContent = File.ReadAllText(filePath);
            SongFileInfo songFileInfo = JsonConvert.DeserializeObject<SongFileInfo>(jsonContent);
            ABC abc = new ABC();
            abc.songFileInfo = songFileInfo;
            abc.path = filePath;
            songFileList.Add(abc);
        }
    }


    private void PopulateList()
    {
        foreach (Transform child in listParent)
        {
            Destroy(child.gameObject);
        }

        int startIndex = currentPage * itemsPerPage;
        int endIndex = Mathf.Min(startIndex + itemsPerPage, filteredList.Count);

        for (int i = startIndex; i < endIndex; i++)
        {
            GameObject listObj = Instantiate(listItemPrefab, listParent);
            listObj.GetComponent<UIItem>().SetData(filteredList[i], this);
        }
    }

    private void OnSearchChanged(string searchTerm)
    {
        if (string.IsNullOrEmpty(searchTerm))
        {
            filteredList = songFileList;
        }
        else
        {
            filteredList = songFileList
                .Where(info => info.songFileInfo.songInfo.title.IndexOf(searchTerm, System.StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();
        }

        filteredList = filteredList.OrderBy(x => x.songFileInfo.songInfo.title).ToList();

        currentPage = 0;
        PopulateList();
    }

    private void OnNextPage()
    {
        if ((currentPage + 1) * itemsPerPage < filteredList.Count)
        {
            currentPage++;
            PopulateList();
            pageText.text = (currentPage + 1).ToString();
        }
    }

    private void OnPrevPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            PopulateList();
            pageText.text = (currentPage + 1).ToString();
        }
    }
}

[System.Serializable]
public class NoteData
{
    public int line;
    public float duration;
}

[System.Serializable]
public class SongInfo
{
    public string title;
    public int bpm;
    public int lineCount;
    public float duration;
    public string tag;
    public string path;
}

[System.Serializable]
public class GameStats
{
    public int maxCombo;
    public float maxPercentage;
}


[System.Serializable]
public class SongFileInfo
{
    public SongInfo songInfo;
    public GameStats gameStats;
    public Dictionary<int, List<NoteData>> notes;
}

[System.Serializable]
public class ABC
{
    public SongFileInfo songFileInfo;
    public string path;
}