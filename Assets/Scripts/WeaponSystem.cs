/// <summary>
/// WeaponSystem.cs
/// Author: MutantGopher
/// This script manages weapon switching.  It's recommended that you attach this to a parent GameObject of all your weapons, but this is not necessary.
/// This script allows the player to switch weapons in two ways, by pressing the numbers corresponding to each weapon, or by scrolling with the mouse.
/// </summary>

using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponSystem : MonoBehaviour
{
    public GameObject[] weapons;                // The array that holds all the weapons that the player has
    public int startingWeaponIndex = 0;         // The weapon index that the player will start with
    private int weaponIndex;                    // The current index of the active weapon

    // Use this for initialization
    void Start()
    {
        // Make sure the starting active weapon is the one selected by the user in startingWeaponIndex
        weaponIndex = startingWeaponIndex;
        SetActiveWeapon(weaponIndex);
    }

    void OnGUI()
    {


    }

    public void SetActiveWeapon(int index)
    {
        // Make sure this weapon exists before trying to switch to it
        if (index >= weapons.Length || index < 0)
        {
            Debug.LogWarning("Tried to switch to a weapon that does not exist.  Make sure you have all the correct weapons in your weapons array.");
            return;
        }

        // Send a messsage so that users can do other actions whenever this happens
        SendMessageUpwards("OnEasyWeaponsSwitch", SendMessageOptions.DontRequireReceiver);

        // Make sure the weaponIndex references the correct weapon
        weaponIndex = index;

        // Make sure beam game objects aren't left over after weapon switching
        Weapon weapon = weapons[index].GetComponent<Weapon>();
        weapon.StopBeam();
        weapon.isFiring = false;


        // Start be deactivating all weapons
        for (int i = 0; i < weapons.Length; i++)
        {
            weapons[i].SetActive(false);
        }

        // Activate the one weapon that we want
        weapons[index].SetActive(true);
    }

    public void NextWeapon()
    {
        weaponIndex++;
        if (weaponIndex > weapons.Length - 1)
            weaponIndex = 0;
        SetActiveWeapon(weaponIndex);
    }


    public void PreviousWeapon()
    {
        weaponIndex--;
        if (weaponIndex < 0)
            weaponIndex = weapons.Length - 1;
        SetActiveWeapon(weaponIndex);
    }

    public void OnNextWeapon(InputValue inputValue)
    {
        NextWeapon();
    }

    public void OnPreviousWeapon(InputValue inputValue)
    {
        PreviousWeapon();
    }

    public void SwitchInput(bool virtualSwitchState)
    {
        if (virtualSwitchState)
        {
            NextWeapon();
        }
    }

    public void OnWeaponSwap1(InputValue inputValue)
    {
        SetActiveWeapon(0);
    }

    public void OnWeaponSwap2(InputValue inputValue)
    {
        SetActiveWeapon(1);
    }

    public void OnWeaponSwap3(InputValue inputValue)
    {
        SetActiveWeapon(2);
    }

    public void OnWeaponSwap4(InputValue inputValue)
    {
        SetActiveWeapon(3);
    }

    public void OnWeaponSwap5(InputValue inputValue)
    {
        SetActiveWeapon(4);
    }

    public void OnWeaponSwap6(InputValue inputValue)
    {
        SetActiveWeapon(5);
    }

    public void OnWeaponSwap7(InputValue inputValue)
    {
        SetActiveWeapon(6);
    }

    public void OnWeaponSwap8(InputValue inputValue)
    {
        SetActiveWeapon(7);
    }

    public void OnScrollWeapon (InputValue inputValue)
    {
        Vector2 scrollValue = inputValue.Get<Vector2>();
        if (scrollValue.y > 0f)
        {
            NextWeapon();
        }
        else if (scrollValue.y < 0f)
        {
            PreviousWeapon();
        }
    }

}
