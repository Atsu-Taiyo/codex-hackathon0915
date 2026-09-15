using System;
using UnityEngine;

namespace Hackathon.RoomOne
{
    /// <summary>Direct references keep PR sprites available in both Editor and player builds.</summary>
    public sealed class RoomOneArtwork : ScriptableObject
    {
        [Serializable]
        public sealed class Clip
        {
            public string id;
            public Texture2D[] frames;
        }

        public Texture2D background;
        public Texture2D robot;
        public Texture2D box;
        public Clip[] clips;

        public Texture2D Frame(string id, int index)
        {
            if (clips == null) return null;
            foreach (var clip in clips)
                if (clip.id == id && clip.frames != null && index >= 0 && index < clip.frames.Length)
                    return clip.frames[index];
            return null;
        }

        public bool IsComplete
        {
            get
            {
                if (!background || !robot || !box) return false;
                foreach (string action in RoomOneRules.Actions)
                    for (int i = 0; i < 3; i++)
                        if (!Frame("robot:" + action + ":box", i)) return false;
                for (int i = 0; i < 3; i++)
                    if (!Frame("box:lift:robot", i)) return false;
                return true;
            }
        }
    }
}
