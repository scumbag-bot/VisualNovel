using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueEvents : MonoBehaviour
{
    public static void HandleEvents(string _Events, CLM.LINE.SEGMENT segment)
    {
        if (_Events .Contains ("("))
        {
            string[] actions = _Events.Split(' ');
            for (int i = 0; i < actions.Length ; i++)
            {
                NovelController.instance.handleActions(actions[i]);
            }
            return;
        }
        string[] eventData = _Events.Split(' ');
        print("data" + eventData[0]);
        switch (eventData[0])
        {
            case "txtSpd":
                EVENTS_txtSpd(eventData[1], segment);
                print(eventData[1]);
                break;
            case "/txtSpd":
                segment.architect.speed = 1;
                segment.architect.charactersPerFrame = 1;
                break;
        }
    }
    static void EVENTS_txtSpd(string data, CLM.LINE.SEGMENT seg)
    {
        string[] parts = data.Split(',');
        float delay = float.Parse(parts[0]);
        int characterPerFrame = int.Parse(parts[1]);

        seg.architect.charactersPerFrame = characterPerFrame;
        seg.architect.speed = delay;
    }
}
