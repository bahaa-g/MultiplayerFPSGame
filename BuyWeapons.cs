using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.LakeWalk.MadGuns
{
    public class BuyWeapons : MonoBehaviour
    {
        public static BuyWeapons instance;
        //public Button[] weapons;
        //public Button[] gadgets;
        public int index;// The selected weapon
        public static bool buttonPressed;
        //public Weapon[] weaponsContainer;
        private Object weapon;// This holds the current weapon that you equiped
        public List<Object> weapons;// This list contains all the rifles and gadgets

        void Awake()
        {
            instance = this;
        }

        //public void chooseWeapon(int index)
        //{
        //    this.index = index;
        //    buttonPressed = true;
        //    weapons[index].interactable = false;
        //}
        public void chooseWeapon(Object weapon)// Contains the GameObject that holds the specified Weapon class
        {
            //this.index = weapon.weaponIndex;// This where it should be placed in the weapon list in the FireAndRecoil script
            this.weapon = weapon;
            buttonPressed = true;
            FireAndRecoil.weaponSelected = true;
        }

        public void ChooseWeapon(int index)// Each button has an index which refers to the Object found in the weapons array
        {
            this.index = index;
            buttonPressed = true;
            FireAndRecoil.weaponSelected = true;
        }

        public Object GetWeapon(int index)// This is called from the FireAndRecoil script
		{
            return weapons[index];
        }

        public void DisableButton(Button button)
        {
            button.interactable = false;
        }

        public void PurchaseWeapon(int weaponIndex, int weaponIndexType)// For gadgets
        {
            //Manager.instance.ShootScript.
        }
    }
}