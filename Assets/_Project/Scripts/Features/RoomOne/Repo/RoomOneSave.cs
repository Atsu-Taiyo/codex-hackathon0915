using System;
using UnityEngine;

namespace Hackathon.RoomOne
{
    public static class RoomOneSave
    {
        public const string Key = "Hackathon.RoomOne.Progress.v1";
        public static RoomOneProgress Load(bool swahili = false)
        {
            RoomOneProgress progress = null;
            try { progress = JsonUtility.FromJson<RoomOneProgress>(PlayerPrefs.GetString(swahili ? Key + ".sw" : Key, "{}")); }
            catch (Exception ex) { Debug.LogWarning("[RoomOne] Save could not be read: " + ex.Message); }
            if (progress == null) progress = new RoomOneProgress();
            progress.Normalize();
            return progress;
        }
        public static void Store(RoomOneProgress progress, bool swahili = false)
        {
            PlayerPrefs.SetString(swahili ? Key + ".sw" : Key, JsonUtility.ToJson(progress));
            PlayerPrefs.Save();
        }
    }
}
