using UnityEngine;

public class WeaponPickUp : Interactable
{
    public WeaponItem weaponItem;

    public override void Interact(GameObject player)
    {
        base.Interact(player);
        PickUpItem(player);
    }

    private void PickUpItem(GameObject player)
    {
        PlayerInventory playerInventory;
        AnimationController animationController;
        playerInventory = player.GetComponent<PlayerInventory>();
        animationController = player.GetComponent<AnimationController>();

        //인벤토리에 해당 무기가 없다면 추가
        if (!playerInventory.weaponsInventory.Contains(weaponItem))
        {
            playerInventory.weaponsInventory.Add(weaponItem);
        }
        animationController.PlayTargetAnimation("Item_Get", true);
        player.GetComponent<PlayerController>().GetItem(weaponItem);
        Destroy(gameObject);
    }
}
