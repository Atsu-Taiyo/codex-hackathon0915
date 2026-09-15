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
            foreach (string action in RoomOneRules.Actions)
            {
                room.Replay(new SentenceMeaning { subject = "robot", action = action, target = "box" });
                float timeout = Time.realtimeSinceStartup + 8;
                for (int index = 0; index < 3; index++)
                {
                    // The first quarter-second is a neutral pose, before the requested clip starts.
                    if (index == 0) yield return new WaitForSecondsRealtime(.35f);
                    while (room.CurrentFrame != index && Time.realtimeSinceStartup < timeout) yield return null;
                    if (!room.IsPlaying || room.CurrentFrame != index) throw new Exception("CloudRobot frame missing: " + action + " " + index);
                    yield return new WaitForEndOfFrame();
                    ScreenCapture.CaptureScreenshot(Path.GetFullPath("Temp/RoomOne-cloud-" + action + "-" + index + ".png"));
                    yield return new WaitForSecondsRealtime(.1f);
                }
                while (room.IsPlaying) yield return null;
            }
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
            room.ResetCards();
            File.WriteAllText("Temp/RoomOne-arttest.txt", "PASS: all 21 CloudRobot frames rendered and captured; new workshop idle captured; replay left history/clear unchanged.");
            Debug.Log("[RoomOne] All seven CloudRobot animations and transparency render test passed.");
        }
    }
}
