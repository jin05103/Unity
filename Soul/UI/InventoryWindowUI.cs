using UnityEngine;

public class InventoryWindowUI : MonoBehaviour
{
    public bool rightHandSlot01Selected;
    public bool rightHandSlot02Selected;
    public bool leftHandSlot01Selected;
    public bool leftHandSlot02Selected;

    public void SelectRightHandSlot01()
    {
        rightHandSlot01Selected = true;
        rightHandSlot02Selected = false;
        leftHandSlot01Selected = false;
        leftHandSlot02Selected = false;
    }

    public void SelectRightHandSlot02()
    {
        rightHandSlot01Selected = false;
        rightHandSlot02Selected = true;
        leftHandSlot01Selected = false;
        leftHandSlot02Selected = false;
    }

    public void SelectLeftHandSlot01()
    {
        rightHandSlot01Selected = false;
        rightHandSlot02Selected = false;
        leftHandSlot01Selected = true;
        leftHandSlot02Selected = false;
    }

    public void SelectLeftHandSlot02()
    {
        rightHandSlot01Selected = false;
        rightHandSlot02Selected = false;
        leftHandSlot01Selected = false;
        leftHandSlot02Selected = true;
    }

    public int GetTrue()
    {
        if (rightHandSlot01Selected)
        {
            return 0;
        }
        else if (rightHandSlot02Selected)
        {
            return 1;
        }
        else if (leftHandSlot01Selected)
        {
            return 0;
        }
        else
        {
            return 1;
        }
    }
}
