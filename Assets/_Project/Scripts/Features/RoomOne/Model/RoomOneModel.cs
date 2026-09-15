using System;
using System.Collections.Generic;

namespace Hackathon.RoomOne
{
    [Serializable]
    public sealed class SentenceMeaning
    {
        public string subject;
        public string action;
        public string target;
        public string Key => subject + ":" + action + ":" + target;
        public string Display => "The " + subject + " " + RoomOneRules.DisplayVerb(action) + " the " + target + ".";
    }

    [Serializable]
    public sealed class RoomOneProgress
    {
        public int version = 1;
        public bool isUnlocked = true;
        public bool isCleared;
        public List<string> discoveredWords = new List<string>();
        public List<string> globalVocabulary = new List<string>();
        public List<string> discoveredCollections = new List<string>();
        public List<SentenceMeaning> executionHistory = new List<SentenceMeaning>();

        public void Normalize()
        {
            if (discoveredWords == null) discoveredWords = new List<string>();
            if (globalVocabulary == null) globalVocabulary = new List<string>();
            if (discoveredCollections == null) discoveredCollections = new List<string>();
            if (executionHistory == null) executionHistory = new List<SentenceMeaning>();
            foreach (string verb in RoomOneRules.Actions)
                if (!globalVocabulary.Contains(verb)) globalVocabulary.Add(verb);
            foreach (string word in discoveredWords)
                if (!globalVocabulary.Contains(word)) globalVocabulary.Add(word);
        }
    }
}
