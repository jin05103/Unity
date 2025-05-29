using UnityEngine;

public class WallChild : MonoBehaviour
{
    public bool isHere;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isHere = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isHere = false;
        }
    }
}
