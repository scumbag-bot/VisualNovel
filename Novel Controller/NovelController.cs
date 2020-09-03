using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NovelController : MonoBehaviour
{
    public static NovelController instance;
    List<string> data = new List<string>();
    public string variable;
    void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        LoadGameFile(0);
    }

    public void LoadGameFile(int gameFileIndex)
    {
        string filePath = FileManager.savPath + "Resources/gamefile/" + gameFileIndex.ToString() + ".txt";

        if (!System.IO.File .Exists (filePath ))
        {
            ///this line is for saving file/game data
            FileManager.SaveJSON(filePath, new GAMEFILE());
        }
        ///load saved game data
        GAMEFILE gameFile = FileManager.LoadJSON<GAMEFILE>(filePath);
        /// load the chapter file
        data = FileManager.LoadFile(FileManager.savPath + "Resources/Story/" + gameFile.chapterName);
        cacheLastSpeaker = gameFile.cachedLastSpeaker;
        chapterProgress = gameFile.chapterProgress;

        if (handlingChapterFile != null)
            StopCoroutine(handlingChapterFile);
        handlingChapterFile = StartCoroutine(HandlingChapterFile());

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow ))
        {
            Next();
        }
    }

    public void loadChapterFile(string fileName)
    {
        data = FileManager.LoadFile(FileManager.savPath + "Resources/Story/" + fileName);
        cacheLastSpeaker = "";

        if (handlingChapterFile != null)
            StopCoroutine(handlingChapterFile);
        handlingChapterFile = StartCoroutine(HandlingChapterFile());

        Next();
    }
    bool _next = false;
    public void Next()
    {
        _next = true;
    }

    [HideInInspector]
    int chapterProgress = 0;
    Coroutine handlingChapterFile = null;
    IEnumerator HandlingChapterFile()
    {
        ///the progress trough the line in this chapter
        chapterProgress  = 0;
        while (chapterProgress  < data.Count )
        {
            if(_next )
            {
                string line = data[chapterProgress];
                if (line.StartsWith ("choice"))
                {
                    yield return HandlingChoiceLine(line);
                    chapterProgress++;
                }
                else
                {
                    handleLine(line );
                    chapterProgress++;
                    while (isHandlingLine)
                    {
                        yield return new WaitForEndOfFrame();
                    }
                }
            }   
            yield return new WaitForEndOfFrame();
        }
        handlingChapterFile = null;
    }
    /// <summary>
    /// Handling choice command
    /// </summary>
    /// <param name="choice"></param>
    /// <returns></returns>
    IEnumerator HandlingChoiceLine(string line)
    {
        string title = line .Split('"')[1];
        List<string> choices = new List<string>();
        List<string> actions = new List<string>();

        while (true)
        {
            chapterProgress++;
            line = data[chapterProgress];

            if (line == "{")
                continue;

            line = line.Replace("\t","");
            if (line != "}")
            {
                choices.Add(line.Split('"')[1]);
                string act = data[chapterProgress + 1].Replace("\t","");
                print(act);
                actions.Add(act);
                chapterProgress++;
            }
            else
            {
                break;
            }
        }
        if (choices.Count > 0)
        {
            ChoiceScreen.Show(title, choices.ToArray());
            yield return new WaitForEndOfFrame();

            while (ChoiceScreen.isWaitingChoiceToBeMade)
                yield return new WaitForEndOfFrame();

            // choice is made. execute the paired actions
            string action = actions[ChoiceScreen.lastChoiceMade.index];
            handleLine(action);

            while (isHandlingLine)
                yield return new WaitForEndOfFrame();
        }
        else
        {
            Debug.LogError("invalid Choice operation");
        }
    }

    void handleLine(string rawLine)
    {
        CLM.LINE line = CLM.interpret(rawLine);
        StopHandlingLine();
        handlingLine = StartCoroutine(HandlingLine(line));
    }

    void StopHandlingLine()
    {
        if (isHandlingLine)
            StopCoroutine(handlingLine);
        handlingLine = null;
    }
    [HideInInspector]
    public string cacheLastSpeaker = "";
    public bool isHandlingLine { get { return handlingLine != null; } }
    Coroutine handlingLine = null;
    IEnumerator HandlingLine(CLM.LINE line)
    {
        _next = false;
        int lineProgress = 0; /// progress trough the line
        while (lineProgress < line.segments.Count )
        {
            _next = false; /// reset at the start of the loop
            CLM.LINE.SEGMENT segment = line.segments[lineProgress];
            /// always running automatically at the beginning of line, and wait for the input for procedding
            if (lineProgress > 0)
            {
                if (segment.trigger == CLM.LINE.SEGMENT.TRIGGER.autoDelay )
                {
                    for (float  timer = segment.autoDelayTime ; timer >= 0; timer -= Time.deltaTime )
                    {
                        yield return new WaitForEndOfFrame();
                        if (_next)
                            break; /// allow to termination the delay when "next" is triggered, prevent from unskippable timer
                    }
                }
                else
                {
                    while (!_next)
                        yield return new WaitForEndOfFrame();
                }
            }
            _next = false;

            segment.Run();
            while (segment.isRunning)
            {
                yield return new WaitForEndOfFrame();
                if (_next)
                {
                    if (!segment.architect.skip)
                        segment.architect.skip = true;
                    else
                        segment.ForceFinish();
                    _next = false;
                }
            }
            lineProgress++;

            yield return new WaitForEndOfFrame();
        }
        for (int i = 0; i < line.actions.Count ; i++)
        {
            handleActions(line.actions[i]);
        }
        handlingLine = null;
    }


/// <summary>
/// ACTION LINE
/// </summary>
/// <param name="action"></param>
///////////////////////////////////////////////////////////////////////////////
    public void handleActions(string action)
    {
        print("handle action [" + action + "]");

        string[] data = action.Split('(', ')');
        switch (data[0])
        {
            case ("setBackground"):
                command_setLayerImage(data[1], BCFC.instance.background);
                break;
            case ("setForeground"):
                command_setLayerImage(data[1], BCFC.instance.foreground);
                break;
            case ("setCinematic"):
                command_setLayerImage(data[1], BCFC.instance.cinematic);
                break;
            case ("playMusic"):
                command_playMusic(data[1]);
                break;
            case ("playSound"):
                command_playSFX(data[1]);
                break;
            case ("setExpression"):
                command_setExpression(data[1]);
                break;
            case ("move"):
                command_moveCharacter(data[1]);
                break;
            case ("setPosition"):
                command_setPosition(data[1]);
                break;
            case ("flip"):
                command_flip(data[1]);
                break;
            case ("faceLeft"):
                command_faceLeft(data[1]);
                break;
            case ("faceRight"):
                command_faceRight(data[1]);
                break;
            case ("transBackground"):
                command_transitionLayer (data[1], BCFC .instance.background);
                break;
            case ("transForeground"):
                command_transitionLayer (data[1], BCFC .instance.foreground );
                break;
            case ("transCinematic"):
                command_transitionLayer (data[1], BCFC.instance.cinematic );
                break;
            case ("Show"):
                command_showCharacter(data[1]);
                break;
            case ("Load"):
                command_Load(data[1]);
                break;
            case ("Jump"):
                command_Jump(data[1]);
                break;
        }
    }


    void command_Jump(string lineNumber)
    {
        chapterProgress = int.Parse(lineNumber);
        Next();
    }
    void command_Load(string chapterName)
    {
        NovelController.instance.loadChapterFile(chapterName);
    }
    void command_setLayerImage(string data, BCFC.LAYER layer)
    {
        string texName = data.Contains(",") ? data.Split(',')[0] : data;
        Texture2D tex = texName == "null" ? null : Resources.Load("Images/Background/" + texName) as Texture2D;
        float spd = 2.0f;
        bool smooth = false ;
        if (data.Contains (","))
        {
            string[] parameter = data.Split(',');
            foreach (string  p in parameter )
            {
                float fVal = 2.0f;
                bool bVal = false;
                if (float.TryParse(p, out fVal ))
                {
                    spd = fVal; continue;
                }
                if (bool.TryParse (p, out bVal ))
                {
                    smooth = bVal; continue;
                }
            }
        }
        layer.setTexture(tex );
    }
    void command_playSFX(string data)
    {
        AudioClip clip = Resources.Load("Audio/SFX/" + data) as AudioClip;
        if (clip != null)
            AudioManager.instance.PlaySFX(clip);
        else
            Debug.LogError("Error file not exist -" + data);
    }
    void command_playMusic(string data)
    {
        AudioClip clip = Resources.Load("Audio/SFX/" + data) as AudioClip;
        if (clip != null)
            AudioManager.instance.PlaySFX(clip);
        else
            Debug.LogError("Error file not exist -" + data);
    }
    void command_setExpression(string data)
    {
        string[] parameter = data.Split (',');
        string character = parameter[0];
        string expression = parameter[1];
        Character c = CharacterManager.instance.GetCharacter(character);
        c.setBody(parameter[1]);
    }
    void command_moveCharacter(string data)
    {
        string[] parameter = data.Split(',');
        string character = parameter[0];
        float locationX = float.Parse(parameter[1]);
        float locationY = float.Parse(parameter[2]);
        float speed = parameter.Length >= 4 ? float.Parse (parameter[3]) : 1f;
        bool smooth = parameter.Length == 5 ? bool.Parse(parameter[4]) : true ;

        Character c = CharacterManager.instance.GetCharacter(character );
        c.moveTo(new Vector2(locationX, locationY), speed, smooth);
    }
    void command_setPosition(string data)
    {
        string[] parameter = data.Split(',');
        string character = parameter[0];
        float locationX = float.Parse(parameter[1]);
        float locationY = float.Parse(parameter[2]);

        Character c = CharacterManager.instance.GetCharacter(character);
        c.setPosition(new Vector2(locationX, locationY));
    }
    void command_flip(string data)
    {
        Character c = CharacterManager.instance.GetCharacter(data);
        c.flip();
    }
    void command_faceLeft(string data)
    {
        Character c = CharacterManager.instance.GetCharacter(data);
        c.faceLeft();
    }
    void command_faceRight(string data)
    {
        Character c = CharacterManager.instance.GetCharacter(data);
        c.faceRight();
    }
    void command_transitionLayer(string data, BCFC.LAYER layer)
    {
        string[] parameter = data.Split(',');
        string targetTexName = parameter[0];
        string transitionEffectName = parameter[1];
        float speed = parameter.Length >= 3 ? float.Parse(parameter[2]) : 3f;
        bool smooth = parameter.Length == 4 ? bool.Parse(parameter[3]) : true;
        Texture2D targetTex = targetTexName == null ? null : Resources.Load("Images/Background/" + targetTexName) as Texture2D;
        Texture2D transitionEffect = transitionEffectName == null ? null : Resources.Load("Images/TransitionEffect" + transitionEffectName) as Texture2D;
        TransitionMaster.TransitionLayer(layer, targetTex, transitionEffect, speed, smooth);
    }
    void command_showCharacter(string data)
    {
        Character c = CharacterManager.instance.GetCharacter(data);
    }
}
