using Unity.Netcode;
using UnityEngine;

public class BoxTrigger : MonoBehaviour
{
    [SerializeField] private BoxCollider2D boxCollider;

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject == NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject()?.gameObject)
        {
            boxCollider.enabled = true;
        }
    }
}
