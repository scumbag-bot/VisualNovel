using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CLM : MonoBehaviour
{
    public static LINE interpret(string rawLine)
    {
        return new LINE(rawLine);
    }
    public class LINE
    {
        ///variable for speaker name
        string speaker = "";
        string lastSegmentsWholeDialogue = "";
        /// <summary>
        /// segments on this line that make up each place on dialogue
        /// </summary>
        public List<SEGMENT> segments = new List<SEGMENT>();
        /// action list that called during or at the end of line 
        public List<string> actions = new List<string>();
        public LINE(string rawLine) 
        {
            string[] dialogueAndActions = rawLine.Split('"');
            char actionSplitter = ' ';
            string[] actionArr = dialogueAndActions.Length == 3 ? dialogueAndActions[2].Split(actionSplitter) : dialogueAndActions[0].Split(actionSplitter);

            if (dialogueAndActions.Length == 3)
            {
                speaker = dialogueAndActions[0] == "" ? NovelController.instance.cacheLastSpeaker : dialogueAndActions[0];
                print(speaker);
                if (speaker[speaker.Length - 1] == ' ')
                    speaker = speaker.Remove(speaker.Length - 1);
                NovelController.instance.cacheLastSpeaker = speaker;
                segmentDialogue(dialogueAndActions[1]);
            }
            for (int i = 0; i < actionArr.Length ; i++)
            {
                actions.Add(actionArr[i]);
            }
        }
        void segmentDialogue(string dialogue)
        {
            segments.Clear();
            string[] parts = dialogue.Split('{', '}');

            for (int i = 0; i < parts.Length ; i++)
            {
                SEGMENT segment = new SEGMENT();
                bool isOdd = i % 2 != 0;
                if (isOdd )
                {
                    string[] commandData = parts[i].Split(' ');
                    switch (commandData[0])
                    {
                        case "a": /// wait for input and append
                            segment.trigger = SEGMENT.TRIGGER.waitClick;
                            segment.pretext = segments.Count > 0 ? segments[segments.Count - 1].dialogue  : "";
                            print("pretext = "+ segment.pretext);
                            break;
                        case "c": /// wait for input and clear
                            segment.trigger = SEGMENT.TRIGGER.waitClickClear;
                            break;
                        case "w": /// wait for time
                            segment.trigger = SEGMENT.TRIGGER.autoDelay;
                            segment.autoDelayTime = float.Parse(commandData[1]);
                            break;
                        case "wa": /// wait forr time and append
                            segment.trigger = SEGMENT.TRIGGER.autoDelay;
                            segment.autoDelayTime = float.Parse(commandData[1]);
                            segment.pretext = segments.Count > 0 ? segments[segments.Count - 1].dialogue : "";
                            print("pretext = "+segment.pretext);
                            break;
                    }
                    i++;
                }
                segment.dialogue = parts[i];
                segment.line = this;

                segments.Add(segment);
            }
        }
        public class SEGMENT
        {
            public LINE line;
            public string dialogue = "";
            public string pretext = "";
            public float autoDelayTime = 0;
            public enum TRIGGER { waitClick, waitClickClear, autoDelay };
            public TRIGGER trigger = TRIGGER.waitClickClear;
            public void Run()
            {
                if (running != null)
                    NovelController.instance.StopCoroutine(running);
                running = NovelController.instance.StartCoroutine(Running());
            }

            public TextArchitect architect = null;
            List<string> allCurrentlyExecutedEvents = new List<string>();
            public bool isRunning { get { return running != null; } }
            Coroutine running = null;
            IEnumerator Running()
            {
                allCurrentlyExecutedEvents.Clear();
                TagManager.Inject(ref dialogue);
                string[] parts = dialogue.Split('[', ']');
                for (int i = 0; i < parts.Length ; i++)
                {
                    bool isOdd = i % 2 != 0;
                    if(isOdd )
                    {
                        DialogueEvents.HandleEvents(parts[i], this);
                        allCurrentlyExecutedEvents.Add(parts[i]);
                        i++;
                    }

                    string targetDialogue = parts[i];
                    print("dialog = " + parts[i ] );
                    if (line.speaker != "narator" && line.speaker != "MC")
                    {
                        Character character = CharacterManager.instance.GetCharacter(line.speaker);
                        character.say(targetDialogue ,i > 0 ? true : pretext != "");
                    }
                    else if(line.speaker == "MC")
                    {
                        DIalogueSystem.instance.Say(targetDialogue, "Adovandria", i > 0 ? true : pretext != "");
                    }
                    else
                    {
                        DIalogueSystem.instance.Say(targetDialogue, line.speaker, i > 0 ? true : pretext != "");
                    }
                    architect = DIalogueSystem.instance.currentArchitect;

                    while (architect.isConstructing)
                        yield return new WaitForEndOfFrame();
                }
                running = null;
            }

            public void ForceFinish()
            {
                if (running != null)
                    NovelController.instance.StopCoroutine(running);
                running = null;
                if (architect != null)
                {
                    architect.ForceFinish();
                    if (pretext == "")
                        line.lastSegmentsWholeDialogue = "";
                    string[] parts = dialogue.Split('[', ']');
                    for (int i = 0; i < parts.Length ; i++)
                    {
                        bool isOdd = i % 2 != 0;
                        if(isOdd)
                        {
                            string e = parts[i];
                            if (allCurrentlyExecutedEvents .Contains (e ))
                            {
                                allCurrentlyExecutedEvents.Remove(e);
                            }
                            else
                            {
                                DialogueEvents.HandleEvents(e , this );
                            }
                            i++;
                        }
                        line.lastSegmentsWholeDialogue  += parts[i];
                    }
                    architect.ShowText(line.lastSegmentsWholeDialogue );
                }
            }
        }
    }
}
