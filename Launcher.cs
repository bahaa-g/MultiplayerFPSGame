using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;


namespace Com.LakeWalk.MadGuns
{
    [System.Serializable]
    public class ProfileData
    {
        public string usernameInput;
        public int level;
        public int xp;

        public ProfileData()
        {
            this.usernameInput = "";
            this.level = 1;
            this.xp = 0;
        }

        public ProfileData(string u, int l, int x)
        {
            this.usernameInput = u;
            this.level = l;
            this.xp = x;
        }
    }

    public class Launcher : MonoBehaviourPunCallbacks
    {
        public TMP_InputField usernameField;
        public TextMeshProUGUI username;// The name of the player in the profile
        public static ProfileData myProfile = new ProfileData();
        public GameObject EnterNameBG;// This panel restricts the player from continueing the game when he enters an invalid username
        public GameObject waitForConnection;

        public void Awake()
        {
            PhotonNetwork.AutomaticallySyncScene = true;
            string name = PlayerPrefs.GetString("Username", "NewUser");
            myProfile.usernameInput = name;
            username.text = name;
            //myProfile = Data.LoadProfile();
            //if (!string.IsNullOrEmpty(myProfile.username))
            //{
            //    usernameField.text = myProfile.username;
            //}
            
            Connect();
        }

        private void Start()
        {
            CanvasManager.instance.SwitchToMenuHUD(true);
        }
        //PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = this.maxPlayersPerRoom});


        public override void OnConnectedToMaster()
        {
            Debug.Log("CONNECTED!");
            waitForConnection.SetActive(false);
            //PhotonNetwork.JoinLobby();
            base.OnConnectedToMaster();
        }

        public override void OnJoinedRoom()
        {
            StartGame();

            base.OnJoinedRoom();
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            Create();

            base.OnJoinRandomFailed(returnCode, message);
        }

        public void Connect()
        {
            waitForConnection.SetActive(true);
            Debug.Log("Trying to Connect...");
            PhotonNetwork.GameVersion = "0.0.0";
            PhotonNetwork.ConnectUsingSettings();
        }

        public void Join()// When the player presses play button
        {
            PhotonNetwork.JoinRandomRoom();
        }

        public void Create()// Create  a room
        {
           /* RoomOptions options = new RoomOptions();
            options.MaxPlayers = (byte)maxPlayersSlider.value;

            ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable();
            properties.Add("map", 0);
            options.CustomRoomProperties = properties;
            */
            //PhotonNetwork.CreateRoom(roomnameField.text, options);
            PhotonNetwork.CreateRoom("");
        }

        public void StartGame()
        {
            //VerifyUsername();

            if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
            {
                //Data.SaveProfile(myProfile);
                PhotonNetwork.LoadLevel(1);
            }
        }

        private void VerifyUsername()// Not being used
        {
            if (string.IsNullOrEmpty(usernameField.text))
            {
                myProfile.usernameInput = "RANDOM_USER_" + Random.Range(100, 1000);
            }
            else
            {
                myProfile.usernameInput = usernameField.text;
            }
        }

        public void ChangeName(TextMeshProUGUI input)
        {
            if (string.IsNullOrEmpty(input.text))
            {
                EnterNameBG.SetActive(true);
            }
            else
            {
                username.text = input.text;
                PlayerPrefs.SetString("Username", input.text);// Saves the player's name
                myProfile.usernameInput = input.text;// Loads the new name into the profile
                EnterNameBG.SetActive(false);
            }
            
        }

        public void PracticeOffline()
        {
            SceneManager.LoadScene(2, LoadSceneMode.Single);
            Debug.Log("Join Offline room");
        }

    //public override void OnEnable()
    //{
    //    CanvasManager.instance.SwitchToMenuHUD(true);
    //    PhotonNetwork.AddCallbackTarget(this);
    //}

    //public new void OnDisable()
    //{
    //    CanvasManager.instance.SwitchToMenuHUD(false);
    //    PhotonNetwork.RemoveCallbackTarget(this);
    //    Debug.Log("Switch UI");
    //}
}
}