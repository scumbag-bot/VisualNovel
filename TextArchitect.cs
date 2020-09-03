using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextArchitect
{
    /// <summary>
    /// The current text built by the text architect up to this point in time.
    /// </summary>
    /// <value>The current text.</value>
    private static Dictionary<TextMeshProUGUI, TextArchitect> activeArchitects = new Dictionary<TextMeshProUGUI, TextArchitect>();
	public string currentText {get {return _currentText;}}
	private string _currentText = "";

	private string preText;
	private string targetText;

	public int charactersPerFrame = 1;
	public  float speed = 1f;
	public bool skip = false;

	public bool isConstructing {get{return buildProcess != null;}}
	Coroutine buildProcess = null;

    TextMeshProUGUI tmpro;

	public TextArchitect(TextMeshProUGUI tmpro, string targetText, string preText = "", int charactersPerFrame = 1, float speed = 1f)
	{
        this.tmpro = tmpro;
		this.targetText = targetText;
		this.preText = preText;
		this.charactersPerFrame = charactersPerFrame;
        this.speed = Mathf.Clamp(speed, 1f, 30f);
        Initiate();
	}

	public void Stop()
	{
		if (isConstructing)
		{
            DIalogueSystem.instance.StopCoroutine(buildProcess);
		}
		buildProcess = null;
	}

	IEnumerator Construction()
	{
		int runsThisFrame = 0;
        tmpro.text = "";
        tmpro.text += preText;

        tmpro.ForceMeshUpdate();
        TMP_TextInfo inf = tmpro.textInfo;
        int vis = inf.characterCount;

        tmpro.text  += targetText;

        tmpro.ForceMeshUpdate();
        inf = tmpro.textInfo;
        int max = inf.characterCount;

        tmpro.maxVisibleCharacters = vis;
        int cpf = charactersPerFrame ;
        while (vis < max)
        {
            if (skip)
            {
                speed = 1;
                cpf  = charactersPerFrame < 5 ? 5 : charactersPerFrame + 3;    
            }
            while (runsThisFrame < cpf  )
            {
                vis++;
                tmpro.maxVisibleCharacters = vis;
                runsThisFrame++;
            }
            runsThisFrame = 0;
            yield return new WaitForSeconds(0.01f * speed);
        }

        //end the build process, construction is done.
        Terminate();
	}

    void Initiate()
    {
        TextArchitect existingArchitect = null;
        if (activeArchitects.TryGetValue(tmpro, out existingArchitect))
            existingArchitect.Terminate();

        buildProcess = DIalogueSystem.instance.StartCoroutine(Construction());
        activeArchitects.Add(tmpro, this);
    }

    public void Terminate()
    {
        activeArchitects.Remove(tmpro);
        if (isConstructing)
            DIalogueSystem.instance.StopCoroutine(buildProcess);
        buildProcess = null;
    }
    public void ForceFinish()
    {
        tmpro.maxVisibleCharacters = tmpro.text.Length;
        Terminate();
    }
    /// <summary>
    /// renew the architect by giving new target display, overriding the old one, allow the architect to used multlpe times rather than creating
    /// new one
    /// </summary>
    /// <param name="targ"></param>
    /// <param name="pre"></param>
    public void Renew(string targ, string pre)
    {
        targetText = targ;
        preText = pre;
        skip = false;

        if (isConstructing)
            DIalogueSystem.instance.StopCoroutine(buildProcess);
        buildProcess = DIalogueSystem.instance.StartCoroutine(Construction());
    }

    public void ShowText(string text)
    {
        if (isConstructing)
            DIalogueSystem.instance.StopCoroutine(buildProcess);
        targetText = text;
        tmpro.text = text;
        tmpro.maxVisibleCharacters = tmpro.text.Length;

        if (tmpro == DIalogueSystem.instance.SpeechText )
        {
            DIalogueSystem.instance.targetSpeech = text;
        }
    }
}