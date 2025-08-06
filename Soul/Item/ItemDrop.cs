using UnityEngine;

public class ItemDrop : MonoBehaviour
{
    [SerializeField] GameObject itemPrefab;

    public void DropItem(DropEntry dropEntry, Vector3 dropPosition)
    {
        if (dropEntry.item != null)
        {
            if (Random.Range(0f, 1f) <= dropEntry.dropRate)
            {
                GameObject item = Instantiate(itemPrefab, dropPosition + new Vector3(0, 0.6f, 0), Quaternion.identity);
                WeaponPickUp weaponPickUp = item.GetComponent<WeaponPickUp>();
                if (weaponPickUp != null)
                {
                    weaponPickUp.weaponItem = dropEntry.item;
                }
            }
        }
    }
}
