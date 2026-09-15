using System;
using System.Collections.Generic;

namespace Hackathon.RoomOne
{
    public static class RoomOneRules
    {
        public static readonly string[] Actions = { "push", "pull", "lift", "open", "shake", "break" };
        public static readonly string[] Verbs = { "pushes", "pulls", "lifts", "opens", "shakes", "breaks" };
        public static string DisplayVerb(string action)
        {
            int index = Array.IndexOf(Actions, action);
            return index >= 0 ? Verbs[index] : action;
        }
        public static bool IsNoun(string word) => word == "robot" || word == "box";
        public static bool TryInterpret(IList<string> cards, out SentenceMeaning meaning, out int badSlot)
        {
            meaning = null;
            badSlot = -1;
            if (cards == null || cards.Count != 3) { badSlot = 0; return false; }
            if (!IsNoun(cards[0])) { badSlot = 0; return false; }
            if (Array.IndexOf(Actions, cards[1]) < 0) { badSlot = 1; return false; }
            if (!IsNoun(cards[2])) { badSlot = 2; return false; }
            meaning = new SentenceMeaning { subject = cards[0], action = cards[1], target = cards[2] };
            return true;
        }
        public static bool IsGoal(SentenceMeaning m) => m.subject == "robot" && m.action == "lift" && m.target == "box";
        public static bool IsBoxLift(SentenceMeaning m) => m != null && m.subject == "box" && m.action == "lift" && m.target == "robot";
        public static bool IsCollection(SentenceMeaning m) => m.subject == "robot" && m.target == "box" && Array.IndexOf(Actions, m.action) >= 0;
        public static void Complete(RoomOneProgress progress, SentenceMeaning meaning, bool record)
        {
            if (!record) return;
            progress.executionHistory.Add(meaning);
            if (IsCollection(meaning) && !progress.discoveredCollections.Contains(meaning.Key))
                progress.discoveredCollections.Add(meaning.Key);
            progress.isCleared |= IsGoal(meaning);
        }
    }
}
