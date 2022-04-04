﻿using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace Com.LakeWalk.MadGuns
{
    public static class SaveSystem
    {
        public static void SavePlayer(GameManager player)
        {

            string path = Application.persistentDataPath + "/player.save";
            FileStream stream = new FileStream(path, FileMode.Create);

            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                PlayerData data = new PlayerData(player);

                formatter.Serialize(stream, data);
            }
            catch (SerializationException e)
            {
                Debug.LogError("There was an issue seriallizing this data: " + e.Message);
            }
            finally
            {
                stream.Close();
            }

        }

        public static PlayerData LoadPlayer()
        {
            string path = Application.persistentDataPath + "/player.save";
            if (File.Exists(path))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream stream = new FileStream(path, FileMode.Open);

                PlayerData data = formatter.Deserialize(stream) as PlayerData;
                stream.Close();

                return data;
            }
            else
            {
                Debug.LogError("Save file not found in" + path);
                return null;
            }
        }
    }
}