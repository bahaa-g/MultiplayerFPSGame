using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.LakeWalk.MadGuns
{
    [System.Serializable]
    public class PlayerData
    {

        public bool[] boughtcheck;

        public int moneyAmount;
        public int number;



        public PlayerData(GameManager player)
        {

            moneyAmount = player.moneyAmount;
            boughtcheck = new bool[6];
            for (int i = 0; i < boughtcheck.Length; i++)
            {
                boughtcheck[i] = player.boughtcheck[i];
            }


            number = player.number;


        }

    }
}