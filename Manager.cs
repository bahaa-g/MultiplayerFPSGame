using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using TMPro;

namespace Com.LakeWalk.MadGuns
{
    public class PlayerInfo
    {
        public ProfileData profile;
        public int actor;
        public short kills;
        public short deaths;
        public bool blueTeam;// Master cleints team

        public PlayerInfo(ProfileData p, int a, short k, short d, bool t)
        {
            this.profile = p;
            this.actor = a;
            this.kills = k;
            this.deaths = d;
            this.blueTeam = t;
        }
    }

    public enum GameState
    {
        Waiting = 0,
        Starting = 1,
        Playing = 2,
        Ending = 3
    }

    public class Manager : MonoBehaviourPunCallbacks, IOnEventCallback
    {
        //public static Manager instance;
        public List<PlayerInfo> playerInfo = new List<PlayerInfo>();
        public int myind;// The local player's info inside that list
        private GameState state = GameState.Waiting;
        public int killcount = 3;
        public bool perpetual = false;
        [SerializeField]
        private TextMeshProUGUI ui_mykills;
        [SerializeField]
        private TextMeshProUGUI ui_mydeaths;

        public int nextPlayersTeam;
        public string player_prefab_string;
        public Transform[] spawnPointsTeam1;
        public Transform[] spawnPointsTeam2;
        public Transform spawnPoint;
        public bool teamsSet = false;
        public int currentTeam;
        private bool allyIsDead;// Is true when one of your teamates is dead
        private Vector3[] deadAllyPos = new Vector3[5];// The Position of the dead ally
        // HitIndicator
        private float pointerTimer;// The timer of the pointer
        private float pointerLifeTime = 8f;// The duration of the pointer after being active

        private GameObject clone;// The clone of the player, you just spawned
        private int numberOfDeadAllies;
        private FireAndRecoil ShootScript;
        public Transform ui_leaderboard;
        [SerializeField]
        private int blueTeamCount = 0;// The number of the master cleints teammates
        public bool blueTeam { get; private set; }// Determines whether if we are with master cleint's team
        private bool playerAdded;
        public Transform killFeedContainer;

        public string testingIsMaster = "normal";

        private List<GameObject> enemyPlayers;// Contains the GameObjects of all the enemy players
        private List<GameObject> teamMates;// Contains the GameObjects of all your teammates

        private int blueTeamScore;
        private int redTeamScore;

        public enum EventCodes : byte
        {
            NewPlayer,
            UpdatePlayers,
            ChangeStat,
            NewMatch
        }

  //      void Awake()
		//{
  //          instance = this;
		//}

        private void Start()
        {
            //mapcam.SetActive(false);
            blueTeamCount = 0;
            blueTeam = true;
            ValidateConnection();
            InitializeUI();
            NewPlayer_S(Launcher.myProfile);
            //Spawn();
            //photonView.RPC("RPC_GetTeam", RpcTarget.MasterClient);
            //NewPlayer_S(Launcher.myProfile);
            Debug.Log("array length "+deadAllyPos.Length);

            if (PhotonNetwork.IsMasterClient)
            {
                playerAdded = true;
                testingIsMaster = "Master";
                Spawn();
            }
        }

        //private void Update()
        //{
        //    //if (allyIsDead)
        //    //{
        //    //    pointerTimer += Time.deltaTime;
        //    //    if (pointerTimer >= pointerLifeTime)
        //    //    {
        //    //        allyIsDead = false;
        //    //        pointerTimer = 0f;
        //    //        numberOfDeadAllies = 0;
        //    //    }

        //    //    for (int i = 0; i <= numberOfDeadAllies; i++)
        //    //    {
        //    //        UpdateDeadAllyPosition(deadAllyPos[i], CanvasManager.instance.deadAllyPointer[i]);
        //    //    }
        //    //}
        //}

        public override void OnEnable()
        {
            PhotonNetwork.AddCallbackTarget(this);
            CanvasManager.instance.SwitchToMenuHUD(false);
            //PhotonTeamsManager.PlayerLeftTeam += OnPlayerLeftTeam;
            //PhotonTeamsManager.PlayerJoinedTeam += OnPlayerJoinedTeam;
        }

        //public override void OnDisable()
        //{
        //    PhotonNetwork.RemoveCallbackTarget(this);
        //    //PhotonTeamsManager.PlayerLeftTeam -= OnPlayerLeftTeam;
        //    //PhotonTeamsManager.PlayerJoinedTeam -= OnPlayerJoinedTeam;
        //}

        //private void OnPlayerLeftTeam(Player player, PhotonTeam team)
        //{
        //    Debug.LogFormat("Player {0} left team {1}", player, team);
        //}

        //private void OnPlayerJoinedTeam(Player player, PhotonTeam team)
        //{
        //    Debug.LogFormat("Player {0} joined team {1}", player, team);
        //}

        public void OnEvent(EventData photonEvent)
        {
            if (photonEvent.Code >= 200) return;

            EventCodes e = (EventCodes)photonEvent.Code;
            object[] o = (object[])photonEvent.CustomData;

            switch (e)
            {
                case EventCodes.NewPlayer:
                    NewPlayer_R(o);
                    break;

                case EventCodes.UpdatePlayers:
                    UpdatePlayers_R(o);
                    break;

                case EventCodes.ChangeStat:
                    ChangeStat_R(o);
                    break;

                case EventCodes.NewMatch:
                    NewMatch_R();
                    break;

                //case EventCodes.RefreshTimer:
                //    RefreshTimer_R(o);
                //    break;
            }
            Debug.Log("Event Called");
        }

        public void ScoreBoardButton()
        {
            Leaderboard(ui_leaderboard);
        }

        private void InitializeUI()
        {
            ui_mykills = CanvasManager.instance.kills;
            ui_mydeaths = CanvasManager.instance.deaths;
            //ui_mykills = GameObject.Find("Canvas/InGameUI/Kills").GetComponent<TextMeshProUGUI>();
            //ui_mydeaths = GameObject.Find("Canvas/InGameUI/Deaths").GetComponent<TextMeshProUGUI>();
            //ui_leaderboard = GameObject.Find("HUD").transform.Find("Leaderboard").transform;
            //ui_endgame = GameObject.Find("Canvas").transform.Find("End Game").transform;

            RefreshMyStats();
        }


        private void RefreshMyStats()// Refreshes the ui when getting killed or when killing
        {
            if (playerInfo.Count > myind)
            {
                ui_mykills.text = $"{playerInfo[myind].kills}";
                ui_mydeaths.text = $"{playerInfo[myind].deaths} deaths";
                Debug.Log("Display UI");
            }
            else
            {
                ui_mykills.text = "0";
                ui_mydeaths.text = "0 deaths";
            }
            //Debug.Log("my index is "+myind);
            //Debug.Log("player count is " + playerInfo.Count);
            //Debug.Log("Kills: " + playerInfo[myind].kills + "Deaths: " + playerInfo[myind].deaths);
        }

        private void Leaderboard(Transform p_lb)
        {
            // clean up
            for (int i = 2; i < p_lb.childCount; i++)
            {
                Destroy(p_lb.GetChild(i).gameObject);
            }

            // set details
           // p_lb.Find("Header/Mode").GetComponent<Text>().text = "FREE FOR ALL";
            //p_lb.Find("Header/Map").GetComponent<Text>().text = "Battlefield";

            // cache prefab
            GameObject playercard = p_lb.GetChild(1).gameObject;
            playercard.SetActive(false);

            // sort
            List<PlayerInfo> sorted = SortPlayers(playerInfo);

            // display
            //bool t_alternateColors = false;
            foreach (PlayerInfo a in sorted)
            {
                GameObject newcard = Instantiate(playercard, p_lb) as GameObject;

                //if (t_alternateColors) newcard.GetComponent<Image>().color = new Color32(0, 0, 0, 180);
                //t_alternateColors = !t_alternateColors;

                newcard.transform.Find("Level").GetComponent<TextMeshProUGUI>().text = a.profile.level.ToString("00");
                newcard.transform.Find("Username").GetComponent<TextMeshProUGUI>().text = a.profile.usernameInput;
                //newcard.transform.Find("Score Value").GetComponent<Text>().text = (a.kills * 100).ToString();
                newcard.transform.Find("Kills").GetComponent<TextMeshProUGUI>().text = a.kills.ToString();
                newcard.transform.Find("Deaths").GetComponent<TextMeshProUGUI>().text = a.deaths.ToString();

                newcard.SetActive(true);
            }

            // activate
            p_lb.gameObject.SetActive(true);
        }

        private void KillFeed(string killer, string dead, bool isTeammate)// It should take also the number of the weapon and if it is a headshot or not
        {
            GameObject killFeed = killFeedContainer.GetChild(0).gameObject;
            GameObject killFeedCard = Instantiate(killFeed, killFeedContainer) as GameObject;
            Destroy(killFeedCard, 5f);
            killFeedCard.transform.Find("Killer").GetComponent<TextMeshProUGUI>().text = killer;
            killFeedCard.transform.Find("Dead").GetComponent<TextMeshProUGUI>().text = dead;
            GameObject KillerBG = killFeed.transform.GetChild(0).gameObject;

            if (isTeammate)
            {
                killFeedCard.GetComponent<Image>().color = Color.red;
                KillerBG.GetComponent<Image>().color = Color.blue;
            }
            else
            {
                killFeedCard.GetComponent<Image>().color = Color.blue;
                KillerBG.GetComponent<Image>().color = Color.red;
            }
            killFeedCard.SetActive(true);
        }

        private List<PlayerInfo> SortPlayers(List<PlayerInfo> p_info)
        {
            List<PlayerInfo> sorted = new List<PlayerInfo>();

            while (sorted.Count < p_info.Count)
            {
                // set defaults
                short highest = -1;
                PlayerInfo selection = p_info[0];

                // grab next highest player
                foreach (PlayerInfo a in p_info)
                {
                    if (sorted.Contains(a)) continue;
                    if (a.kills > highest)
                    {
                        selection = a;
                        highest = a.kills;
                    }
                }

                // add player
                sorted.Add(selection);
            }

            return sorted;
        }

        private void StateCheck()
        {
            if (state == GameState.Ending)
            {
                EndGame();
            }
        }

        private void ScoreCheck()
        {
            // define temporary variables
            bool detectwin = false;

            // check to see if any player has met the win conditions
            foreach (PlayerInfo a in playerInfo)
            {
                // free for all
                if (a.kills >= killcount)
                {
                    detectwin = true;
                    break;
                }
            }

            // did we find a winner?
            if (detectwin)
            {
                // are we the master client? is the game still going?
                if (PhotonNetwork.IsMasterClient && state != GameState.Ending)
                {
                    // if so, tell the other players that a winner has been detected
                    UpdatePlayers_S((int)GameState.Ending, playerInfo);
                }
            }
        }

        private void EndGame()
        {
            // set game state to ending
            state = GameState.Ending;

            // disable room
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.DestroyAll();

                if (!perpetual)
                {
                    PhotonNetwork.CurrentRoom.IsVisible = false;
                    PhotonNetwork.CurrentRoom.IsOpen = false;
                }
            }

            // activate map camera
            //mapcam.SetActive(true);

            //// show end game ui
            //ui_endgame.gameObject.SetActive(true);
            //Leaderboard(ui_endgame.Find("Leaderboard"));

            //// wait X seconds and then return to main menu
            //StartCoroutine(End(6f));
        }



        public void Spawn()
        {
            Transform spawn;
            if (blueTeam)
            {
                spawn = spawnPointsTeam1[Random.Range(0, spawnPointsTeam1.Length)];
                
            }
            else
            {   
                spawn = spawnPointsTeam2[Random.Range(0, spawnPointsTeam2.Length)];
            }
            Debug.Log("spawned in blue team:" + blueTeam);

            if (PhotonNetwork.IsConnected)
            {
                clone = PhotonNetwork.Instantiate(player_prefab_string, spawn.position, spawn.rotation);
                ShootScript = clone.GetComponent<FireAndRecoil>();
                Debug.Log("is connected: " + PhotonNetwork.IsConnected);
            }
        }
        

        public void LocateDeadAlly(Vector3 deadPlayerPos)// Also add an integer for the layer to determine whether the dead player is on the same team or not
        {
            deadAllyPos[numberOfDeadAllies] = deadPlayerPos;
            allyIsDead = true;
            pointerTimer = 0f;
            StartCoroutine(WaitAfterAllyIsDead(numberOfDeadAllies));
            numberOfDeadAllies++;
        }

        private void UpdateDeadAllyPosition(Vector3 deadPlayerPos, GameObject deadAllyPointer)
        {
            if (deadPlayerPos != Vector3.zero)
            {
                Vector3 magnitude = Vector3.Normalize(deadPlayerPos - clone.transform.position);
                float angle = Mathf.Atan2(magnitude.z, magnitude.x) * Mathf.Rad2Deg;
                deadAllyPointer.transform.rotation = Quaternion.Euler(0f, 0f, angle + clone.transform.eulerAngles.y);
            }
        }

        IEnumerator WaitAfterAllyIsDead(int i)
        {
            CanvasManager.instance.deadAllyPointer[i].SetActive(true);
            yield return new WaitForSeconds(8f);
            CanvasManager.instance.deadAllyPointer[i].SetActive(false);
        }

        private void ValidateConnection()
        {
            if (PhotonNetwork.IsConnected) return;
            SceneManager.LoadScene(0);
        }

        public void LaunchGadget()// Check if being used
        {
            ShootScript.CallGadgetRPC();
            Debug.Log("Throw the gadget");
        }

        #region Events

        public void NewPlayer_S(ProfileData p)// It sends the new player's information to the master client
        {
            object[] package = new object[7];

            package[0] = p.usernameInput;
            package[1] = p.level;
            package[2] = p.xp;
            package[3] = PhotonNetwork.LocalPlayer.ActorNumber;
            package[4] = (short)0;
            package[5] = (short)0;
            package[6] = true;

            PhotonNetwork.RaiseEvent(
                (byte)EventCodes.NewPlayer,
                package,
                new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient },
                new SendOptions { Reliability = true }
            );
            Debug.Log("Send Player");
            Debug.Log("playerInfo size (Send): " + playerInfo.Count);
            Debug.Log("player info: " + p.usernameInput);
            Debug.Log("actor number: " + PhotonNetwork.LocalPlayer.ActorNumber);
        }
        public void NewPlayer_R(object[] data)// The information is then sent to all the players
        {
            PlayerInfo p = new PlayerInfo(
                new ProfileData(
                    (string)data[0],
                    (int)data[1],
                    (int)data[2]
                ),
                (int)data[3],
                (short)data[4],
                (short)data[5],
                CalculateTeam()
            );

            playerInfo.Add(p);

            UpdatePlayers_S((int)state, playerInfo);
            Debug.Log("Receive Player");
            Debug.Log("playerInfo size (Recieve): "+playerInfo.Count);
            //Debug.Log("New Player's Team is blue: "+CalculateTeam());
        }

        public void UpdatePlayers_S(int state, List<PlayerInfo> info)
        {
            object[] package = new object[info.Count + 1];

            package[0] = state;
            for (int i = 0; i < info.Count; i++)
            {
                object[] piece = new object[7];

                piece[0] = info[i].profile.usernameInput;
                piece[1] = info[i].profile.level;
                piece[2] = info[i].profile.xp;
                piece[3] = info[i].actor;
                piece[4] = info[i].kills;
                piece[5] = info[i].deaths;
                piece[6] = info[i].blueTeam;

                package[i + 1] = piece;
            }

            PhotonNetwork.RaiseEvent(
                (byte)EventCodes.UpdatePlayers,
                package,
                new RaiseEventOptions { Receivers = ReceiverGroup.All },
                new SendOptions { Reliability = true }
            );
            //Debug.Log("Update Player S");
        }
        public void UpdatePlayers_R(object[] data)
        {
            state = (GameState)data[0];
            playerInfo = new List<PlayerInfo>();

            for (int i = 1; i < data.Length; i++)
            {
                object[] extract = (object[])data[i];

                PlayerInfo p = new PlayerInfo(
                    new ProfileData(
                        (string)extract[0],
                        (int)extract[1],
                        (int)extract[2]
                    ),
                    (int)extract[3],
                    (short)extract[4],
                    (short)extract[5],
                    (bool)extract[6]
                );

                playerInfo.Add(p);

                if (PhotonNetwork.LocalPlayer.ActorNumber == p.actor)
                {
                    myind = i - 1;

                    if (!playerAdded)
                    {
                        blueTeam = p.blueTeam;// Whether the player is with the master cleint's team
                        playerAdded = true;
                        Spawn();
                        Debug.Log("is blue Team"+blueTeam);
                    }
                }
            }
            //Debug.Log("Update Player R");
            //Debug.Log("my index is: " + myind);
            //Debug.Log("playerInfo size (Update R): " + playerInfo.Count);
            StateCheck();
        }

        public void ChangeStat_S(int actorKiller, int actorDead)// The two parameters should be the index of the killer and the index of the dead player
        {
            object[] package = new object[] { actorKiller, actorDead };

            PhotonNetwork.RaiseEvent(
                (byte)EventCodes.ChangeStat,
                package,
                new RaiseEventOptions { Receivers = ReceiverGroup.All },
                new SendOptions { Reliability = true }
            );
            Debug.Log("Send Kill");
        }
        public void ChangeStat_R(object[] data)
        {
            int actorKiller = (int)data[0];
            int actorDead = (int)data[1];

            string killerName = "";
            string deadName = "";
            bool isTeammate = true;

            for (int i = 0; i < playerInfo.Count; i++)
            {
                if (playerInfo[i].actor == actorKiller)
                {
                    playerInfo[i].kills++;
                    if (i == myind)
                    {
                        RefreshMyStats();
                        CanvasManager.instance.ShowKillImage();
                    }
                    if (ui_leaderboard.gameObject.activeSelf) Leaderboard(ui_leaderboard);
                    killerName = playerInfo[i].profile.usernameInput;
                    if (playerInfo[i].blueTeam)
                    {
                        isTeammate = true;
                        blueTeamScore++;
                        CanvasManager.instance.blueTeamScore.text = blueTeamScore.ToString();
                    }
                    else
                    {
                        isTeammate = false;
                        redTeamScore++;
                        CanvasManager.instance.redTeamScore.text = redTeamScore.ToString();
                    }
                    Debug.Log("is on the same team: " + isTeammate);
                    Debug.Log("blue team " + playerInfo[i].blueTeam);
                    Debug.Log("my blue team:" + blueTeam);
                    //Debug.Log($"Player {playerInfo[i].profile.usernameInput} : kills = {playerInfo[i].kills}");
                }
                else if (playerInfo[i].actor == actorDead)
                {
                    playerInfo[i].deaths++;
                    if (i == myind) RefreshMyStats();
                    if (ui_leaderboard.gameObject.activeSelf) Leaderboard(ui_leaderboard);
                    deadName = playerInfo[i].profile.usernameInput;
                    //Debug.Log($"Player {playerInfo[i].profile.usernameInput} : deaths = {playerInfo[i].deaths}");
                }
            }
            KillFeed(killerName, deadName, isTeammate);
            //ScoreCheck();
        }

        //public void ChangeStat_S(int actor, byte stat, byte amt)
        //{
        //    object[] package = new object[] { actor, stat, amt };

        //    PhotonNetwork.RaiseEvent(
        //        (byte)EventCodes.ChangeStat,
        //        package,
        //        new RaiseEventOptions { Receivers = ReceiverGroup.All },
        //        new SendOptions { Reliability = true }
        //    );
        //    Debug.Log("Send Kill");
        //}
        //public void ChangeStat_R(object[] data)
        //{
        //    int actor = (int)data[0];
        //    byte stat = (byte)data[1];
        //    byte amt = (byte)data[2];

        //    for (int i = 0; i < playerInfo.Count; i++)
        //    {
        //        if (playerInfo[i].actor == actor)
        //        {
        //            switch (stat)
        //            {
        //                case 0: //kills
        //                    playerInfo[i].kills += amt;
        //                    Debug.Log($"Player {playerInfo[i].profile.usernameInput} : kills = {playerInfo[i].kills}");
        //                    break;

        //                case 1: //deaths
        //                    playerInfo[i].deaths += amt;
        //                    Debug.Log($"Player {playerInfo[i].profile.usernameInput} : deaths = {playerInfo[i].deaths}");
        //                    break;
        //            }

        //            if (i == myind) RefreshMyStats();
        //            if (ui_leaderboard.gameObject.activeSelf) Leaderboard(ui_leaderboard);

        //            return;
        //        }
        //    }
        //    Debug.Log("Recieve Kill");
        //    ScoreCheck();
        //}

        public void NewMatch_S()
        {
            PhotonNetwork.RaiseEvent(
                (byte)EventCodes.NewMatch,
                null,
                new RaiseEventOptions { Receivers = ReceiverGroup.All },
                new SendOptions { Reliability = true }
            );
        }
        public void NewMatch_R()
        {
            // set game state to waiting
            state = GameState.Waiting;

            // deactivate map camera
            //mapcam.SetActive(false);

            // hide end game ui
            //ui_endgame.gameObject.SetActive(false);

            // reset scores
            foreach (PlayerInfo p in playerInfo)
            {
                p.kills = 0;
                p.deaths = 0;
            }

            // reset ui
            RefreshMyStats();

            // spawn
            Spawn();
        }

        private bool CalculateTeam()// Add to Manager
        {
            int half = (playerInfo.Count + 1) / 2;

            Debug.Log("player count before calculating team is:" + playerInfo.Count);
            if (blueTeamCount >= half && half != 0)
            {
                Debug.Log("The player team is red");
                return false;
            }
            else
            {
                blueTeamCount++;
                Debug.Log("The player team is blue");
                return true;
            }
            
        }

        #endregion

        public void AddToTeamMates(GameObject ally)
        {
            teamMates.Add(ally);
        }

        public void AddToEnemies(GameObject enemy)
        {
            enemyPlayers.Add(enemy);
        }

        public Vector3 PushFrom(Vector3 origin, Vector3 target, float pushDistance, float time)
        {
            Vector3 value = target;
            value.x = Mathf.Abs(origin.x) + Mathf.Abs(target.x)/time;// Add the absolute value x of the origin to the target
            if (value.x <= pushDistance)// Check if it is still in the specified distance
            {
                value.y = Mathf.Abs(origin.z) + Mathf.Abs(target.z)/time;// Add the absolute value y of the origin to the target
                return value;// Assign the value to the target
            }

            return target;// If it is outside the specified distance return the target value without changing it
        }

        public override void OnLeftRoom()
        {
            SceneManager.LoadScene(0, LoadSceneMode.Single);// Laods the main menu when player leaves a match
            Debug.Log("leave room");
        }

        public static void LeaveRoom()
        {
            if (FireAndRecoil.isOffline)
                SceneManager.LoadScene(0, LoadSceneMode.Single);
            else
            {
                PhotonNetwork.LeaveRoom();
            }
        }
    }
}