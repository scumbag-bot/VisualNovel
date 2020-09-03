using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DIalogueSystem : MonoBehaviour
{
    public static DIalogueSystem instance;
    public ELEMENTS elements;
    void Awake()
    {
        instance = this;
    }
    public void close()
    {
        StopSpeaking();
        speechPanel.SetActive(false);

    }
    public void Say(string speech, string speaker="", bool additive = false   )
    {
        StopSpeaking();
        if (additive)
            SpeechText.text = targetSpeech;
        speaking = StartCoroutine(Speaking(speech, additive, speaker));                   
    }

    public void StopSpeaking()
    {
        if (isSpeaking )
        {
            StopCoroutine(speaking);
        }
        if (textArchitect != null && textArchitect.isConstructing )
        {
            textArchitect.Stop();
        }
        speaking = null; 
    }

    public bool isSpeaking { get { return speaking != null; } }
    [HideInInspector] public bool isWaitingForUserInput = false;

    public string targetSpeech = "";
    public TextArchitect currentArchitect { get { return textArchitect; } }
    TextArchitect textArchitect = null;
    Coroutine speaking = null;

    IEnumerator Speaking(string speech,bool additive, string speaker = "")
    {
        speechPanel.SetActive(true);

        string additiveSpeech = additive ? SpeechText .text : "";
        targetSpeech = additiveSpeech + speech;
        if (textArchitect == null)
            textArchitect = new TextArchitect(SpeechText, speech, additiveSpeech);
        else
            textArchitect.Renew(speech, additiveSpeech);

        SpeakerNameText.text = DetermineSpeaker (speaker);
        speakerNamePanel.SetActive(SpeakerNameText.text != "");

        isWaitingForUserInput = false;
        while (textArchitect.isConstructing )
        {
            if (Input.GetKey(KeyCode.Space))
                textArchitect.skip = true;
            //print(textArchitect.currentText);
            //SpeechText.text = textArchitect.currentText;
            yield return new WaitForEndOfFrame();
        }
        //SpeechText.text = textArchitect.currentText;
        isWaitingForUserInput = true;
        while (isWaitingForUserInput)
            yield return new WaitForEndOfFrame();

        StopSpeaking();
    }
    string DetermineSpeaker(string s)
    {
        string retVal = SpeakerNameText.text;
        if (s != SpeakerNameText.text && s != "")
        {
            retVal = s.ToLower().Contains("narator") ? "" : s;
        }
        return retVal;
    }
    

    [System.Serializable ]
    public class ELEMENTS
    {
        public GameObject speechPanel;
        public Text SpeakerNameText;
        public TextMeshProUGUI  SpeechText;
        public GameObject spaekerNamePanel;
    }
    public GameObject speechPanel { get { return elements.speechPanel; } }
    public Text SpeakerNameText { get { return elements.SpeakerNameText; } }
    public TextMeshProUGUI SpeechText { get { return elements.SpeechText; } }
    public GameObject speakerNamePanel { get { return elements.spaekerNamePanel; } }
}
