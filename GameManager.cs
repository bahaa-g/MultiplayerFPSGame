using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.LakeWalk.MadGuns
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager instance;
        public RotateWeaponView RotateView;
        //public Text NOV;

        public int moneyAmount = 0;
        public bool[] boughtcheck;

        public int number;

        [SerializeField]
        private List<int> info;
        private int index;
        private int weaponIndex;// Represents the index of the weapon in weapons[]
        private Material currentSkin;
        [SerializeField]
        private Material[] skins;// Contains all the skins for all the weapons
        public GameObject[] weapons;
        public GameObject weaponHolder;
        //public int[] skinIndex;// Represents the skin index in array skins[] for each weapon 

        public GameObject equipButton;
        public GameObject watchAdButton;
        public GameObject buyButton;
        public Color32 bgColor;

        public Slider watchedAds;// This slider represents the amount of videos you watched

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            //LoadPlayer();
            //PlayerPrefs.GetInt("NOV", 0);
        }

        //private void Start()
        //{
        //    //NOV.text = PlayerPrefs.GetInt("NOV").ToString();
        //    AudioManager.instance.Play("ThemeSong");
        //}

        private void FixedUpdate()
        {
            if (RotateView.Pressed)
            {
                Vector2 rotate = RotateView.TouchDist;
                weaponHolder.transform.rotation *= Quaternion.Euler(rotate.y, -rotate.x, 0f);
            }
        }

        public void SavePlayer()
        {
            SaveSystem.SavePlayer(this);
        }

        public void LoadPlayer()
        {

            PlayerData data = SaveSystem.LoadPlayer();


            moneyAmount = data.moneyAmount;
            boughtcheck = new bool[6];
            for (int i = 0; i < boughtcheck.Length; i++)
            {
                boughtcheck[i] = data.boughtcheck[i];
            }

            number = data.number;

        }

        public void SelectWeapon(int weaponIndex)
        {
            this.weaponIndex = weaponIndex;
            weapons[weaponIndex].SetActive(true);// Activates the weapon that you have selected
        }

        public void SelectSkin(int index)// Select the index of the array info[]
        {
            this.index = index;
            string skinInfo = info[index].ToString();

            if (skinInfo[2] == '1')
            {
                Renderer rend = weapons[weaponIndex].GetComponent<Renderer>();
                rend.enabled = true;
                rend.sharedMaterial = skins[index];
                watchAdButton.SetActive(false);
                buyButton.SetActive(false);
                equipButton.SetActive(true);
            }
            else
            {
                watchAdButton.SetActive(true);
                buyButton.SetActive(true);
                equipButton.SetActive(false);
            }
        }

        public void EquipSkin()// The info integer consists of 
        {
            // SavePlayer();// It should save that you equiped a new skin
        }

        public void WatchVideo()// Loads an advertisement
        {

        }

        public void VideoEnded()// Decreases the number of times you still have to watch inorder to unlock a skin
        {
            string skinInfo = info[index].ToString();// Copy the integer value to a string
            int currentAds = skinInfo[1];// Gets the current amount of watched videos and sets it to an integer
            currentAds -= 1;// The integer is then decremented by 1
            skinInfo[1].Equals(currentAds.ToString());// And then it is set back to the string
            info[1] = int.Parse(skinInfo);// And then the value of the string is set to the integer value
            watchedAds.maxValue = skinInfo[0];
            watchedAds.value = currentAds;

            if (currentAds <= 0)// If you watched the required amount of videos to unlock this skin then the last value in the integer will be set to 1
            {
                skinInfo[2].Equals('1');// This means that this skin is bought
            }
        }


        /*void OnApplicationFocus(bool focus)// When the player exits the game without removing it from cashe
        {
            isPaused = !focus;
            SavePlayer();
        }

        void OnApplicationPause(bool pause)
        {
            isPaused = pause;
            SavePlayer();
        }*/

        //void OnApplicationQuit()
        //{
        //    //SavePlayer();
        //}

        public void ChangeColor()
        {
            Camera.main.backgroundColor = bgColor;
        }

        public void MainMenuColor()
        {
            Camera.main.backgroundColor = Color.white;
        }        
    }
}