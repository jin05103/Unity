using UnityEngine;

public class Wall : MonoBehaviour
{
    [SerializeField] GameObject insideCollider;

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (insideCollider.GetComponent<WallChild>().isHere)
            {
                GetComponent<Collider>().isTrigger = false;
            }
        }
    }
}
