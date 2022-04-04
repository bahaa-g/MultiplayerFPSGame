using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
//using UnityEngine.InputSystem;

namespace Com.LakeWalk.MadGuns
{
    public class CanvasManager : MonoBehaviour
    {
        public static CanvasManager instance;

        public Canvas canvas;
        //public FixedButton JumpButton;
        public FixedTouchField TouchField;
        //public Fire fire;
        //public ADS ads;
        //public BuyWeapons buyWeapons;
        public RectTransform[] buttons;
        public RectTransform crossHair;
        public EditSettings editSettings;
        private int lastADSIndex = 0;
        public GameObject crossHairContainer;
        public Image crossHair1;
        public Image crossHair2;
        public Image crossHair3;
        public Image crossHair4;
        public GameObject hitPointer;// Indicates where the player got shot from
        public GameObject[] deadAllyPointer;// Indicates the position your ally when dead
        public GameObject killImage;
        public GameObject headShotImage;// Is Active when you hit a headshot
        public GameObject scopeImage;// The scope for the sniper
        public GameObject MainMenuHUD;// This is disabled when in a match
        public GameObject InGameHUD;// This is disabled when in the main menu
        public GameObject inGameControls;// Contains the in game controls

        public TextMeshProUGUI bulletsInMagText;
        public TextMeshProUGUI bulletsCarriedText;

        public TextMeshProUGUI kills;
        public TextMeshProUGUI deaths;

        public TextMeshProUGUI blueTeamScore;
        public TextMeshProUGUI redTeamScore;

        public Slider healthBar;
        // Fps counter
        public Text fpsRect;
        private float fps;
        public TextMeshProUGUI healthTestingText;

        public Text deltaText;
        //private bool activateHUD = true;

        //public InputMaster controls;

        void Awake()
        {
          //  controls = new InputMaster();
            //controls.Menu.Buttons.performed += ctx => Launcher.Join();
            //controls.Menu.

            if (instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            canvas = this.GetComponent<Canvas>();
            //Touchscreen.current.touches
        }

        void Start()
		{
			editSettings.UpdateButtons(buttons);
			
			if (PlayerPrefs.GetFloat("CrossPosX") != 0)// Checks if the crossHair have been edited before if not it will just load the default values
			{
               editSettings.UpdateCrossHair();
            }

            StartCoroutine(RecalculateFPS());
		}

        public void EnableADS(int index)
		{
            lastADSIndex = index;
		}

        private IEnumerator RecalculateFPS()// Shows the current frames per second
        {
            while (true)
            {
                fps = 1 / Time.deltaTime;
                fpsRect.text = fps.ToString("0.0");
                yield return new WaitForSeconds(0.5f);
            }
        }

        public void EnableScope(bool activate)
        {
            scopeImage.SetActive(activate);
        }

        public void SwitchToMenuHUD(bool activateHUD)
        {
            //activateHUD = !activateHUD;
            MainMenuHUD.SetActive(activateHUD);
            InGameHUD.SetActive(!activateHUD);
        }

        public void CrossHairHitColor(bool hitColor)
        {
            Color color;
            if (hitColor)// if you hit someone this color should be displayed
            {
                color = Color.yellow;// This should have the ability to be modified
            }
            else// This is the normal crosshait color
            {
                color = Color.white;// This should have the ability to be modified
            }
            crossHair1.color = color;
            crossHair2.color = color;
            crossHair3.color = color;
            crossHair4.color = color;
        }

        public void ChangeHealth(int modification)
        {
            BotScript.maxHealth += modification;
            healthTestingText.text = BotScript.maxHealth.ToString();
        }

        public void ShowKillImage()
        {
            StartCoroutine(KillImageTimeOut());
        }

        IEnumerator KillImageTimeOut()
        {
            killImage.SetActive(true);
            yield return new WaitForSeconds(3f);
            killImage.SetActive(false);
        }

        //private void OnEnable() => controls.Enable();

        //private void OnDisable() => controls.Disable();

        public void LeaveRoom()
        {
            Manager.LeaveRoom();
        }
    }
}