#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using Hackathon.Core;
using Hackathon.Map;
using Hackathon.RoomOne;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Hackathon.Editor
{
    public sealed class WorldMapPlaytest : MonoBehaviour
    {
        bool hadSave, backedUp;
        string backup;
        int checks;

        [UnityEditor.MenuItem("Tools/World Map/Run Navigation Playtest")]
        public static void Begin()
        {
            if (!Application.isPlaying || !FindAnyObjectByType<WorldMapController>())
                throw new InvalidOperationException("Open WorldMap and enter Play mode before testing.");
            if (FindAnyObjectByType<WorldMapPlaytest>()) throw new InvalidOperationException("A test is already running.");
            new GameObject("World Map verification").AddComponent<WorldMapPlaytest>();
        }

        IEnumerator Start()
        {
            DontDestroyOnLoad(gameObject);
            hadSave = PlayerPrefs.HasKey(RoomOneSave.Key); backup = PlayerPrefs.GetString(RoomOneSave.Key); backedUp = true;
            try
            {
                var fresh = new RoomOneProgress(); fresh.Normalize(); RoomOneSave.Store(fresh);
                yield return SceneManager.LoadSceneAsync(WorldNavigation.MapScene);
                var map = FindAnyObjectByType<WorldMapController>();
                Check(map && map.Progress != null && !map.Progress.isCleared, "Map loads a fresh progress state");
                Check(GameObject.Find("Terrain from PR 2").transform.childCount > 30, "PR terrain instantiated in map");
                foreach (var renderer in FindObjectsByType<Renderer>())
                    foreach (var material in renderer.sharedMaterials)
                        Check(material && material.shader.isSupported, "Map material renders: " + renderer.name);
                Check(FindObjectsByType<AudioListener>().Length == 1, "One active audio listener");
                Check(Application.CanStreamedLevelBeLoaded(WorldNavigation.RoomOneScene), "Room 1 included in build scenes");
                map.enabled = false; map.robo.acceptInput = false;
                yield return new WaitForSeconds(.3f);
                Vector3 start = map.robo.transform.position;
                map.robo.WalkTo(new Vector3(-4, .06f, -.45f));
                float deadline = Time.realtimeSinceStartup + 8;
                while (map.robo.HasDestination && Time.realtimeSinceStartup < deadline) yield return null;
                Check(Vector3.Distance(start, map.robo.transform.position) > 2, "Robot walks on imported terrain");
                Check(!map.robo.HasDestination && map.NearWorkshop, "Robot reaches workshop entrance");
                Check(map.robo.transform.position.y > -.1f && map.robo.transform.position.y < .3f, "Robot stays grounded on PR mesh colliders");
                // Walk out toward the ocean and verify the physical coast barrier.
                map.robo.WalkTo(new Vector3(-4, 0, -10));
                yield return new WaitForSeconds(3);
                Check(map.robo.transform.position.z > -6 && map.robo.transform.position.y > -.2f, "Coast collision prevents leaving island");
                map.robo.StopWalking();
                map.SelectRoom(2);
                Check(map.SelectedRoom == 2 && SceneManager.GetActiveScene().path == WorldNavigation.MapScene, "Future room selection does not open gameplay");
                map.SelectRoom(1); map.EnterRoomOne();
                yield return null; yield return null;
                var room = FindAnyObjectByType<RoomOneController>();
                Check(room && !FindAnyObjectByType<WorldMapController>(), "Map enters Room 1 without duplicate world");
                Check(room.IsPlaying && !room.ReturnToMap(), "Goal playback blocks scene transition");
                deadline = Time.realtimeSinceStartup + 5;
                while (room.IsPlaying && Time.realtimeSinceStartup < deadline) yield return null;
                Check(!room.IsPlaying, "Room 1 goal preview completes");
                room.Discover("robot"); room.Discover("box");
                Check(room.PlaceCard("robot", 0) && room.PlaceCard("lift", 1) && room.PlaceCard("box", 2), "Room 1 word cards remain functional");
                Check(room.RunSentence(), "Room 1 goal experiment starts");
                Check(!room.ReturnToMap(), "In-flight experiment cannot lose progress through navigation");
                while (room.IsPlaying) yield return null;
                Check(room.Progress.isCleared && room.Modal == "clear", "Room 1 completes and offers return to map");
                int history = room.Progress.executionHistory.Count;
                Check(room.ReturnToMap(), "Clear returns to real map scene");
                yield return null; yield return null;
                map = FindAnyObjectByType<WorldMapController>();
                Check(map && map.Progress.isCleared, "Map reflects saved room clear");
                Check(map.Progress.discoveredWords.Count == 2 && map.Progress.discoveredCollections.Count == 1, "Map reflects saved words and collection");
                Check(FindObjectsByType<AudioListener>().Length == 1, "Returning does not duplicate listeners");
                map.EnterRoomOne(); yield return null; yield return null;
                room = FindAnyObjectByType<RoomOneController>();
                Check(room.Progress.isCleared && room.Progress.executionHistory.Count == history, "Re-entry preserves progress and experiment history");
                while (room.IsPlaying) yield return null;
                Check(room.Progress.executionHistory.Count == history, "Re-entry goal preview does not add history");
                Restore();
                yield return SceneManager.LoadSceneAsync(WorldNavigation.MapScene);
                Check(hadSave ? PlayerPrefs.GetString(RoomOneSave.Key) == backup : !PlayerPrefs.HasKey(RoomOneSave.Key), "Original saved progress restored exactly");
                File.WriteAllText("Temp/WorldMap-playtest.txt", checks + " checks passed. PR materials, grounded movement, coast collision, Map > Room 1 > Map > Room 1, clear/progress/history persistence, and exact save restoration.");
                Debug.Log("[WorldMap] " + checks + " playtest checks passed.");
            }
            finally { Restore(); Destroy(gameObject); }
        }

        void Check(bool ok, string description)
        {
            if (!ok)
            {
                File.WriteAllText("Temp/WorldMap-playtest.txt", "FAILED: " + description);
                throw new InvalidOperationException(description);
            }
            checks++;
        }
        void Restore()
        {
            if (!backedUp) return;
            if (hadSave) PlayerPrefs.SetString(RoomOneSave.Key, backup); else PlayerPrefs.DeleteKey(RoomOneSave.Key);
            PlayerPrefs.Save(); backedUp = false;
        }
        void OnDestroy() { Restore(); }
    }
}

#endif
