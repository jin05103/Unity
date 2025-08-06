using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum UIState
{
    none, escape, inventory, level, setting,
    weaponSelect, shieldSelect
}

public class UIManager : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] AnimationController animationController;
    [SerializeField] PlayerInventory playerInventory;
    [SerializeField] InventoryWindowUI inventoryWindowUI;
    [SerializeField] PlayerStats playerStats;
    [SerializeField] InputsHandler _input;
    [SerializeField] GameObject escapePanel;

    [SerializeField] GameObject InventoryButton;
    [SerializeField] GameObject levelButton;
    [SerializeField] GameObject gameOptionsButton;

    [SerializeField] GameObject InventoryPanel;
    [SerializeField] GameObject levelPanel;
    [SerializeField] GameObject gameOptionsPanel;

    [SerializeField] GameObject[] WeaponSlots;
    [SerializeField] GameObject[] ShieldSlots;

    [SerializeField] GameObject WeaponSelectPanel;
    [SerializeField] GameObject shieldSelectPanel;

    [SerializeField] GameObject weaponPrefabs;
    [SerializeField] GameObject shieldPrefabs;

    public GameObject restPotionText;
    [SerializeField] GameObject currencyText;

    [SerializeField] TMP_Text levelText;
    [SerializeField] TMP_Text requireText;
    [SerializeField] TMP_Text healthText;
    [SerializeField] TMP_Text staminaText;
    [SerializeField] TMP_Text strengthText;
    [SerializeField] TMP_Text agilityText;
    [SerializeField] Button healthButton;
    [SerializeField] Button staminaButton;
    [SerializeField] Button strengthButton;
    [SerializeField] Button agilityButton;

    UIState state;

    WeaponItem selectedWeapon;
    WeaponItem equipedWeapon;
    GameObject selectedWeaponBtn;
    GameObject equipedWeaponBtn;

    private void Start()
    {

        state = UIState.none;

        escapePanel.SetActive(false);

        InventoryButton.GetComponent<Button>().onClick.AddListener(InventoryButtonClicked);
        levelButton.GetComponent<Button>().onClick.AddListener(LevelButtonClicked);
        gameOptionsButton.GetComponent<Button>().onClick.AddListener(gameOptionsButtonClicked);

        healthButton.onClick.AddListener(() => LevelUp(StatType.Health));
        staminaButton.onClick.AddListener(() => LevelUp(StatType.Stamina));
        strengthButton.onClick.AddListener(() => LevelUp(StatType.Strength));
        agilityButton.onClick.AddListener(() => LevelUp(StatType.Agility));

        for (int i = 0; i < WeaponSlots.Length; i++)
        {
            WeaponSlots[i].GetComponent<HandEquipmentSlotUI>().SetIcon(playerInventory.weaponInRightHandSlots[i]);
        }
        for (int i = 0; i < ShieldSlots.Length; i++)
        {
            ShieldSlots[i].GetComponent<HandEquipmentSlotUI>().SetIcon(playerInventory.weaponInLeftHandSlots[i]);
        }

        LevelUpUIUpdate();

        InventoryPanel.SetActive(false);
        levelPanel.SetActive(false);
        gameOptionsPanel.SetActive(false);
        WeaponSelectPanel.SetActive(false);
        shieldSelectPanel.SetActive(false);
    }

    private void Update()
    {
        if (_input.escape)
        {
            _input.escape = false;
            switch (state)
            {
                case UIState.none:
                    state = UIState.escape;
                    escapePanel.SetActive(true);
                    Cursor.lockState = CursorLockMode.None;
                    break;
                case UIState.escape:
                    state = UIState.none;
                    escapePanel.SetActive(false);
                    Cursor.lockState = CursorLockMode.Locked;
                    break;
                case UIState.inventory:
                    InventoryPanel.SetActive(false);
                    state = UIState.none;
                    Cursor.lockState = CursorLockMode.Locked;
                    break;
                case UIState.level:
                    levelPanel.SetActive(false);
                    state = UIState.none;
                    Cursor.lockState = CursorLockMode.Locked;
                    break;
                case UIState.setting:
                    gameOptionsPanel.SetActive(false);
                    state = UIState.none;
                    Cursor.lockState = CursorLockMode.Locked;
                    break;
                case UIState.weaponSelect:
                    WeaponSelectPanel.SetActive(false);
                    state = UIState.inventory;
                    InventoryPanel.SetActive(true);
                    break;
                case UIState.shieldSelect:
                    shieldSelectPanel.SetActive(false);
                    state = UIState.inventory;
                    InventoryPanel.SetActive(true);
                    break;
            }
        }
    }

    public void InventoryButtonClicked()
    {
        state = UIState.inventory;
        escapePanel.SetActive(false);
        InventoryPanel.SetActive(true);
        levelPanel.SetActive(false);
        gameOptionsPanel.SetActive(false);
    }

    public void LevelButtonClicked()
    {
        state = UIState.level;
        escapePanel.SetActive(false);
        InventoryPanel.SetActive(false);
        levelPanel.SetActive(true);
        gameOptionsPanel.SetActive(false);
    }

    public void gameOptionsButtonClicked()
    {
        state = UIState.setting;
        escapePanel.SetActive(false);
        InventoryPanel.SetActive(false);
        levelPanel.SetActive(false);
        gameOptionsPanel.SetActive(true);
    }

    public void WeaponSelectButtonClicked()
    {
        state = UIState.weaponSelect;

        equipedWeapon = null;
        selectedWeapon = null;
        selectedWeaponBtn = null;
        equipedWeaponBtn = null;

        foreach (Transform child in WeaponSelectPanel.transform)
        {
            Destroy(child.gameObject);
        }

        int num = inventoryWindowUI.GetTrue();
        selectedWeapon = playerInventory.weaponInRightHandSlots[num];
        equipedWeapon = equipedWeapon = playerInventory.weaponInRightHandSlots[1 - num];

        // if (num == 0)
        // {
        //     equipedWeapon = playerInventory.weaponInRightHandSlots[1];
        // }
        // else
        // {
        //     equipedWeapon = playerInventory.weaponInRightHandSlots[0];
        // }

        for (int i = 0; i < playerInventory.weaponsInventory.Count; i++)
        {
            WeaponItem weaponItem = playerInventory.weaponsInventory[i];
            if (weaponItem.weaponType == WeaponType.sword || weaponItem.weaponType == WeaponType.heavySword || weaponItem.weaponType == WeaponType.bow)
            {
                GameObject btn = Instantiate(weaponPrefabs, WeaponSelectPanel.transform);
                btn.GetComponent<Image>().sprite = weaponItem.itemIcon;
                btn.GetComponent<Equipment>().weaponItem = weaponItem;
                Equipment eq = btn.GetComponent<Equipment>();
                if (selectedWeapon == weaponItem)
                {
                    selectedWeaponBtn = btn;
                    btn.transform.GetChild(0).gameObject.SetActive(true);
                    eq.selectedWeapon = true;
                }
                else if (equipedWeapon == weaponItem)
                {
                    equipedWeaponBtn = btn;
                    btn.transform.GetChild(1).gameObject.SetActive(true);
                    eq.equipedWeapon = true;
                }

                btn.GetComponent<Button>().onClick.AddListener(() => WeaponBtnClicked(btn));
            }
        }

        InventoryPanel.SetActive(false);
        WeaponSelectPanel.SetActive(true);
    }

    public void ShieldSelectButtonClicked()
    {
        state = UIState.shieldSelect;

        equipedWeapon = null;
        selectedWeapon = null;
        selectedWeaponBtn = null;
        equipedWeaponBtn = null;

        foreach (Transform child in shieldSelectPanel.transform)
        {
            Destroy(child.gameObject);
        }

        int num = inventoryWindowUI.GetTrue();
        selectedWeapon = playerInventory.weaponInLeftHandSlots[num];
        equipedWeapon = playerInventory.weaponInLeftHandSlots[1 - num];

        // if (num == 0)
        // {
        //     equipedWeapon = playerInventory.weaponInLeftHandSlots[1];
        // }
        // else
        // {
        //     equipedWeapon = playerInventory.weaponInLeftHandSlots[0];
        // }

        for (int i = 0; i < playerInventory.weaponsInventory.Count; i++)
        {
            WeaponItem weaponItem = playerInventory.weaponsInventory[i];
            if (weaponItem.weaponType == WeaponType.shield)
            {
                GameObject btn = Instantiate(shieldPrefabs, shieldSelectPanel.transform);
                btn.GetComponent<Image>().sprite = weaponItem.itemIcon;
                btn.GetComponent<Equipment>().weaponItem = weaponItem;
                Equipment eq = btn.GetComponent<Equipment>();
                if (selectedWeapon == weaponItem)
                {
                    selectedWeaponBtn = btn;
                    btn.transform.GetChild(0).gameObject.SetActive(true);
                    eq.selectedWeapon = true;
                }
                else if (equipedWeapon == weaponItem)
                {
                    equipedWeaponBtn = btn;
                    btn.transform.GetChild(1).gameObject.SetActive(true);
                    eq.equipedWeapon = true;
                }

                btn.GetComponent<Button>().onClick.AddListener(() => ShieldBtnClicked(btn));
            }
        }

        InventoryPanel.SetActive(false);
        shieldSelectPanel.SetActive(true);
    }

    public void WeaponBtnClicked(GameObject btn)
    {
        int num = inventoryWindowUI.GetTrue();

        //선택된 무기가 있다면
        if (selectedWeaponBtn != null)
        {
            if (btn == selectedWeaponBtn)
            {
                //장착 해제
                selectedWeaponBtn.transform.GetChild(0).gameObject.SetActive(false);
                selectedWeaponBtn = null;
                playerInventory.weaponInRightHandSlots[num] = playerInventory.unarmedWeapon;
            }
            else if (btn == equipedWeaponBtn)
            {
                //선택된 무기 해제 + 장착된 무기를 선택된 무기로
                selectedWeaponBtn.transform.GetChild(0).gameObject.SetActive(false);
                equipedWeaponBtn.transform.GetChild(1).gameObject.SetActive(false);
                equipedWeaponBtn.transform.GetChild(0).gameObject.SetActive(true);
                equipedWeaponBtn = null;
                selectedWeaponBtn = btn;
                playerInventory.weaponInRightHandSlots[num] = selectedWeaponBtn.GetComponent<Equipment>().weaponItem;
                playerInventory.weaponInRightHandSlots[-num + 1] = playerInventory.unarmedWeapon;
            }
            else
            {
                //선택된 무기 해제 + 선택한 무기 장착
                selectedWeaponBtn.transform.GetChild(0).gameObject.SetActive(false);
                selectedWeaponBtn = btn;
                selectedWeaponBtn.transform.GetChild(0).gameObject.SetActive(true);
                playerInventory.weaponInRightHandSlots[num] = selectedWeaponBtn.GetComponent<Equipment>().weaponItem;
            }
        }
        //선택된 무기 없다면
        else
        {
            if (btn == equipedWeaponBtn)
            {
                //장착된 무기를 선택된 무기로
                equipedWeaponBtn.transform.GetChild(1).gameObject.SetActive(false);
                equipedWeaponBtn.transform.GetChild(0).gameObject.SetActive(true);
                equipedWeaponBtn = null;
                selectedWeaponBtn = btn;
                playerInventory.weaponInRightHandSlots[num] = selectedWeaponBtn.GetComponent<Equipment>().weaponItem;
                playerInventory.weaponInRightHandSlots[-num + 1] = playerInventory.unarmedWeapon;
            }
            else
            {
                //선택한 무기 장착
                selectedWeaponBtn = btn;
                selectedWeaponBtn.transform.GetChild(0).gameObject.SetActive(true);
                playerInventory.weaponInRightHandSlots[num] = selectedWeaponBtn.GetComponent<Equipment>().weaponItem;
            }
        }

        //현재 사용중인 무기가 변경된 경우 무기, 애니메이션 변경
        if (playerInventory.currentRightWeaponIndex != -1 && playerInventory.rightWeapon != playerInventory.weaponInRightHandSlots[playerInventory.currentRightWeaponIndex])
        {
            playerInventory.rightWeapon = playerInventory.weaponInRightHandSlots[playerInventory.currentRightWeaponIndex];
            playerInventory.weaponSlotManager.LoadWeaponOnSlot(playerInventory.rightWeapon, false);
            if (playerController.twoHanded)
            {
                playerController.twoHanded = false;

                if (playerInventory.GetWeaponType(true) == WeaponType.shield)
                {
                    playerController.shieldSlot.SetActive(true);
                }
            }

            animationController.ChangeWeaponLayer(playerInventory.rightWeapon.weaponType, false, false);
        }

        //퀵슬롯 & 장비창 변경
        for (int i = 0; i < WeaponSlots.Length; i++)
        {
            WeaponSlots[i].GetComponent<HandEquipmentSlotUI>().SetIcon(playerInventory.weaponInRightHandSlots[i]);
        }
    }

    public void ShieldBtnClicked(GameObject btn)
    {
        int num = inventoryWindowUI.GetTrue();

        //선택된 무기가 있다면
        if (selectedWeaponBtn != null)
        {
            if (btn == selectedWeaponBtn)
            {
                //장착 해제
                selectedWeaponBtn.transform.GetChild(0).gameObject.SetActive(false);
                selectedWeaponBtn = null;
                playerInventory.weaponInLeftHandSlots[num] = playerInventory.unarmedWeapon;
            }
            else if (btn == equipedWeaponBtn)
            {
                //선택된 무기 해제 + 장착된 무기를 선택된 무기로
                selectedWeaponBtn.transform.GetChild(0).gameObject.SetActive(false);
                equipedWeaponBtn.transform.GetChild(1).gameObject.SetActive(false);
                equipedWeaponBtn.transform.GetChild(0).gameObject.SetActive(true);
                equipedWeaponBtn = null;
                selectedWeaponBtn = btn;
                playerInventory.weaponInLeftHandSlots[num] = selectedWeaponBtn.GetComponent<Equipment>().weaponItem;
                playerInventory.weaponInLeftHandSlots[-num + 1] = playerInventory.unarmedWeapon;
            }
            else
            {
                //선택된 무기 해제 + 선택한 무기 장착
                selectedWeaponBtn.transform.GetChild(0).gameObject.SetActive(false);
                selectedWeaponBtn = btn;
                selectedWeaponBtn.transform.GetChild(0).gameObject.SetActive(true);
                playerInventory.weaponInLeftHandSlots[num] = selectedWeaponBtn.GetComponent<Equipment>().weaponItem;
            }
        }
        //선택된 무기 없다면
        else
        {
            if (btn == equipedWeaponBtn)
            {
                //장착된 무기를 선택된 무기로
                equipedWeaponBtn.transform.GetChild(1).gameObject.SetActive(false);
                equipedWeaponBtn.transform.GetChild(0).gameObject.SetActive(true);
                equipedWeaponBtn = null;
                selectedWeaponBtn = btn;
                playerInventory.weaponInLeftHandSlots[num] = selectedWeaponBtn.GetComponent<Equipment>().weaponItem;
                playerInventory.weaponInLeftHandSlots[-num + 1] = playerInventory.unarmedWeapon;
            }
            else
            {
                //선택한 무기 장착
                selectedWeaponBtn = btn;
                selectedWeaponBtn.transform.GetChild(0).gameObject.SetActive(true);
                playerInventory.weaponInLeftHandSlots[num] = selectedWeaponBtn.GetComponent<Equipment>().weaponItem;
            }
        }

        //현재 사용중인 무기가 변경된 경우 무기, 애니메이션 변경
        if (playerInventory.currentLeftWeaponIndex != -1 && playerInventory.leftWeapon != playerInventory.weaponInLeftHandSlots[playerInventory.currentLeftWeaponIndex])
        {
            playerInventory.leftWeapon = playerInventory.weaponInLeftHandSlots[playerInventory.currentLeftWeaponIndex];
            playerInventory.weaponSlotManager.LoadWeaponOnSlot(playerInventory.leftWeapon, true);
            // if (playerController.twoHanded)
            // {
            //     playerController.twoHanded = false;
            //     animationController.ChangeWeaponLayer(playerInventory.rightWeapon.weaponType, false, false);
            // }

        }

        //퀵슬롯 & 장비창 변경
        for (int i = 0; i < ShieldSlots.Length; i++)
        {
            ShieldSlots[i].GetComponent<HandEquipmentSlotUI>().SetIcon(playerInventory.weaponInLeftHandSlots[i]);
        }
    }

    public void UpdatePanel()
    {
        for (int i = 0; i < WeaponSlots.Length; i++)
        {
            WeaponSlots[i].GetComponent<HandEquipmentSlotUI>().SetIcon(playerInventory.weaponInRightHandSlots[i]);
        }
        
        for (int i = 0; i < ShieldSlots.Length; i++)
        {
            ShieldSlots[i].GetComponent<HandEquipmentSlotUI>().SetIcon(playerInventory.weaponInLeftHandSlots[i]);
        }
    }

    public void PotionTextUpdate(int amount)
    {
        restPotionText.SetActive(true);
        restPotionText.GetComponent<TMP_Text>().text = amount.ToString();
    }

    public void CurrencyTextUpdate(int amount)
    {
        currencyText.GetComponent<TMP_Text>().text = amount.ToString();
    }

    public void LevelUp(StatType statType)
    {
        int require = playerStats.stats.GetUpgradeCost();
        if (playerStats.stats.currency >= require)
        {
            playerStats.stats.LevelUp(statType);
            playerStats.stats.currency -= require;
            if (statType == StatType.Health)
            {
                playerStats.HealthUpdate();
            }
            else if (statType == StatType.Stamina)
            {
                playerStats.StaminaUpdate();
            }
            LevelUpUIUpdate();
        }
        else
        {
            Debug.Log("Not enough currency!");
        }
    }

    public void LevelUpUIUpdate()
    {
        levelText.text = "LV. " + playerStats.stats.Level.ToString();
        requireText.text = playerStats.stats.GetUpgradeCost().ToString();
        healthText.text = playerStats.stats.health.ToString();
        staminaText.text = playerStats.stats.stamina.ToString();
        strengthText.text = playerStats.stats.strength.ToString();
        agilityText.text = playerStats.stats.agility.ToString();

        CurrencyTextUpdate(playerStats.stats.currency);
    }
}
