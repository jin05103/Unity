using UnityEngine;
using UnityEngine.Rendering;

public class YSorting : MonoBehaviour
{
    [SerializeField] SortingGroup sortingGroup;

    void Update()
    {
        if (sortingGroup != null)
        {
            sortingGroup.sortingOrder = -Mathf.RoundToInt(transform.position.y) * 2 + 1;
        }
    }
}