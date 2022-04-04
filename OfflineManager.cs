using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.LakeWalk.MadGuns
{
    public class OfflineManager : MonoBehaviour
    {
        public GameObject playerPrefab;
        public Transform spawnPoint;
        void Start()
        {
            SpawnPlayer();
        }

        private void SpawnPlayer()
        {
            Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
        }

        public void OnEnable()
        {
            CanvasManager.instance.SwitchToMenuHUD(false);
        }
    }
}