using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static AmmoRack;

public class HUDController
{
    private VisualElement root;

    class AmmoUISlot
    {
        public Image image;
        public Label name;
        public Label quantity;
    }


    // Ammo
    private Label currentShellName;
    private Label reloadTimeLeft;
    private Label selectedShell;

    private Image mgAmmoSlotImage;
    private Label mgAmmoSlotName;
    private Label mgAmmoSlotQuantity;

    private VisualElement ammoSlotsPanel;
    private VisualTreeAsset ammoSlotPrefab;
    private Dictionary<AmmoData, AmmoUISlot> ammoSlots = new Dictionary<AmmoData, AmmoUISlot>();

    



    //Engine
    private Label rpm;
    private VisualElement rpmBar;
    private Label speed;
    private Label fuel;
    private VisualElement fuelBar;
    private Label gear;



    public HUDController(VisualElement root, VisualTreeAsset ammoSlot)
    {
        this.root = root;
        ammoSlotPrefab = ammoSlot;

        currentShellName = root.Q<Label>("loaded-shell");
        reloadTimeLeft = root.Q<Label>("reload-time");
        selectedShell = root.Q<Label>("next-ammo");

        mgAmmoSlotImage = root.Q<Image>("mgAmmoImg");
        mgAmmoSlotName = root.Q<Label>("mgAmmoName");
        mgAmmoSlotQuantity = root.Q<Label>("mgAmmoQuantity");

        ammoSlotsPanel = root.Q<VisualElement>("AmmoRack-panel");


        rpm = root.Q<Label>("rpm-value");
        rpmBar = root.Q<VisualElement>("rpm-bar");
        speed = root.Q<Label>("speed-value");
        fuel = root.Q<Label>("fuel-value");
        fuelBar = root.Q<VisualElement>("fuel-bar");
        gear = root.Q<Label>("gear-value");



        ammoSlotsPanel.Clear();
    }

    #region Engine
    public void ChangeEngineRpm(float RPM, float percentage)
    {
        rpm.text = Mathf.RoundToInt(RPM).ToString();
        rpmBar.style.width = new Length(Mathf.Clamp(percentage, 0, 100), LengthUnit.Percent);

        if (percentage < 65) rpmBar.style.backgroundColor = Color.green;
        else if (percentage < 85) rpmBar.style.backgroundColor = Color.orange;
        else rpmBar.style.backgroundColor = Color.red;
    } 
    public void ChangeSpeed(int speed) => this.speed.text = speed.ToString();
    public void ChangeFuelPercentage(float liters, float percentage)
    {
        fuel.text = Mathf.RoundToInt(liters).ToString();
        fuelBar.style.width = new Length(Mathf.Clamp(percentage, 0, 100), LengthUnit.Percent);
    }
    public void ChangeGear(int gear) => this.gear.text = (gear == 99? "-" : gear.ToString());
    #endregion


    #region Ammo
    public void ChangeMgAmmoQuantity(MgAmmoBeltSlot slot)
    {
        //mgAmmoSlotImage
        if (mgAmmoSlotName.text != slot.beltData.beltName) mgAmmoSlotName.text = slot.beltData.beltName;
        mgAmmoSlotQuantity.text = slot.quantity.ToString();
    }
    public void ChangeAmmoQuantity(AmmoSlot[] slots)
    {
        foreach (AmmoSlot slot in slots)
        {
            if (ammoSlots.ContainsKey(slot.data))
            {
                // aktualizacja
                ammoSlots[slot.data].quantity.text = slot.quantity.ToString();
            }
            else // stworzenie nowego
            {
                VisualElement slotElement = ammoSlotPrefab.Instantiate();
                AmmoUISlot uiSlot = new AmmoUISlot();

                uiSlot.image = slotElement.Q<Image>("Icon");
                uiSlot.name = slotElement.Q<Label>("Name");
                uiSlot.quantity = slotElement.Q<Label>("Quantity");

                uiSlot.name.text = slot.data.ammoName;
                uiSlot.quantity.text = slot.quantity.ToString();
                // Dodać ikone

                ammoSlotsPanel.Add(slotElement);
                ammoSlots.Add(slot.data, uiSlot);

            }
        }
    }
    public void ChangeReloadData(float remaining, float total) => 
        reloadTimeLeft.text = (Mathf.RoundToInt(remaining * 100f) / 100f).ToString() ;
    public void ChangeLoadedShell(AmmoData shell) => currentShellName.text = shell.ammoName;
    public void ChangeSelectedShell(AmmoData shell) => selectedShell.text = shell.ammoName;
    #endregion

    public void Dispose()
    {
        ammoSlots.Clear();
    }
}
