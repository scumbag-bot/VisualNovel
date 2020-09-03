using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable ]
public class GAMEFILE 
{
    public string chapterName;
    public int chapterProgress = 0;
    public string cachedLastSpeaker;

    public GAMEFILE()
    {
        this.chapterName = "prolog_1";
        this.chapterProgress =0;
        this.cachedLastSpeaker = "";
    }
}
