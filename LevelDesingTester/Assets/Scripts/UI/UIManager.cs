using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("WeaponSelector")]
    [SerializeField] Button[] weaponsHolders;
    [SerializeField] Button weaponSelected;
    [SerializeField] GameObject weaponsPanel;

    //InputAction
    PlayerInput playerInput;
    InputAction swapWeapon;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        swapWeapon = playerInput.currentActionMap["SwapWeapon"];
    }
    private void OnEnable()
    {
        playerInput.currentActionMap.Enable();
    }
    public void OnButtonClick(int index)
    {        
        if (index >= 0 && index < weaponsHolders.Length)
        {
            weaponSelected.image.sprite = weaponsHolders[index].image.sprite;
        }
    }

    void Start()
    {        
        for (int i = 0; i < weaponsHolders.Length; i++)
        {
            int index = i; 
            weaponsHolders[i].onClick.AddListener(() => OnButtonClick(index));
        }
    }

    private void Update()
    {
        if(swapWeapon.triggered)
        {
            transform.Find("WeaponSelector").gameObject.SetActive(true);
        }
    }
    public void OpenWeaponInventory()
    {
        if (!weaponsPanel.activeSelf)
        {
            weaponsPanel.SetActive(true);
        }else
        {
            weaponsPanel.SetActive(false);
        }
        
    }
}
