using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase.Firestore;
using System.Linq;
using Newtonsoft.Json;

public class ListLoader : MonoBehaviour
{
    FirebaseFirestore db;
    string UID;
    string nickName;

    public GameObject chartItemPrefab;
    public Transform chartListContent;

    public RankingPopup rankingPopup; 

    private async void Start()
    {
        UID = FirebaseManager.Instance.Auth.CurrentUser.UserId;
        nickName = FirebaseManager.Instance.Auth.CurrentUser.DisplayName;
        db = FirebaseManager.Instance.Firestore;

        rankingPopup.gameObject.SetActive(false);

        await LoadChartList();
    }

    private async Task LoadChartList()
    {
        var snapshot = await db.Collection("songs").GetSnapshotAsync();
        foreach (var doc in snapshot.Documents)
        {
            var itemGO = Instantiate(chartItemPrefab, chartListContent);
            var chartItem = itemGO.GetComponent<ChartItem>();
            chartItem.SetData(doc);

            // 클릭 시 랭킹 팝업 띄우기
            chartItem.Button.onClick.AddListener(() =>
                ShowRankingPopup(chartItem)
            );
        }
    }

    private void ShowRankingPopup(ChartItem chartItem)
    {
        // 기존에 흩어진 아이템들 정리
        rankingPopup.ClearList();

        // 팝업 활성화 및 데이터 로드
        rankingPopup.gameObject.SetActive(true);
        rankingPopup.Show(chartItem, UID);
    }
}
