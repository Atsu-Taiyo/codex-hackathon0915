using System;
using System.Collections;
using System.IO;
using Hackathon.RoomOne;
using UnityEngine;

namespace Hackathon.Editor
{
    public static class RoomOneArtPlaytest
    {
        public static IEnumerator Run()
        {
            var room = UnityEngine.Object.FindAnyObjectByType<RoomOneController>();
            while (room.IsPlaying) yield return null;
            int history = room.Progress.executionHistory.Count;
            bool cleared = room.Progress.isCleared;
            room.WatchGoal();
            while (room.IsPlaying) yield return null;
            ScreenCapture.CaptureScreenshot(Path.GetFullPath("Temp/RoomOne-transparent-idle.png"));
            yield return new WaitForSecondsRealtime(.15f);
            room.Cards[0] = "box"; room.Cards[1] = "lift"; room.Cards[2] = "robot";
            room.Replay(new SentenceMeaning { subject = "box", action = "lift", target = "robot" });
            float deadline = Time.realtimeSinceStartup + 8;
            for (int frame = 0; frame < 3; frame++)
            {
                while ((!room.UsesBoxLiftAnimation || room.CurrentFrame != frame) && Time.realtimeSinceStartup < deadline) yield return null;
                if (!room.UsesBoxLiftAnimation || room.CurrentFrame != frame) throw new Exception("Reverse lift frame missing: " + frame);
                yield return new WaitForEndOfFrame();
                ScreenCapture.CaptureScreenshot(Path.GetFullPath("Temp/RoomOne-box-lift-" + frame + ".png"));
                yield return new WaitForSecondsRealtime(.1f);
            }
            while (room.IsPlaying) yield return null;
            if (room.Progress.executionHistory.Count != history || room.Progress.isCleared != cleared) throw new Exception("Replay modified progress");
            File.WriteAllText("Temp/RoomOne-arttest.txt", "PASS: dedicated box-lift frames 0,1,2 rendered; replay left history/clear unchanged; four screenshots captured.");
            Debug.Log("[RoomOne] Reverse lift and transparency render test passed.");
        }
    }
}
