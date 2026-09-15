using UnityEngine;
using UnityEngine.SceneManagement;

namespace Hackathon.Core
{
    public static class WorldNavigation
    {
        public const string MapScene = "Assets/_Project/Scenes/WorldMap.unity";
        public const string RoomOneScene = "Assets/_Project/Scenes/Bootstrap.unity";

        public static void OpenMap() => Open(MapScene);
        public static void OpenRoomOne() => Open(RoomOneScene);

        static void Open(string path)
        {
            if (SceneManager.GetActiveScene().path == path) return;
            if (!Application.CanStreamedLevelBeLoaded(path))
            {
                Debug.LogError("[WorldNavigation] Scene must be enabled in Build Settings: " + path);
                return;
            }
            SceneManager.LoadScene(path);
        }
    }
}
