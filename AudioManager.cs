using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static SONG activeSong = null;
    public static List<SONG> allSong = new List<SONG>();
    public float songTransitionSpeed = 2f;
    public bool smoothTransition = true;
    public static AudioManager instance;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            transform.parent = null;
            DontDestroyOnLoad(this);
        }
        else
            DestroyImmediate(gameObject);
    }

    public void PlaySFX(AudioClip effect, float volume = 1f ,float pitch = 1f)
    {
        AudioSource source = CreateNewSource(string.Format("SFX[{0}]", effect.name));
        source.clip = effect;
        source.volume = volume;
        source.pitch = pitch;
        source.Play();

        Destroy(source.gameObject, effect.length);
    }

    public void PlaySong(AudioClip song, float maxVolume = 1f,float startingVolume = 0f, float pitch = 1f, bool playOnStart = true , bool loop =true)
    {
        if (song != null)
        {
            for (int i = 0; i < allSong.Count; i++)
            {
                SONG s = allSong[i];
                if (s.clip == song)
                {
                    activeSong = s;
                    break;
                }
            }
            if (activeSong != null || activeSong.clip != song)
                activeSong = new SONG(song, maxVolume, pitch,startingVolume , playOnStart, loop);
            
        }
        else
            activeSong = null;
        StopAllCoroutines();
        StartCoroutine(VolumeLeveling());
    }

    IEnumerator VolumeLeveling()
    {
        if (TransitionSong())
        {
            yield return new WaitForEndOfFrame();
        }
    }

    bool TransitionSong()
    {
        bool anyValueChange = false;
        float speed = Time.deltaTime * songTransitionSpeed;
        for (int i = 0; i < allSong.Count -1; i--)
        {
            SONG song = allSong[i];
            if (song == activeSong )
            {
                if (song.volume < song.maxVolume )
                {
                    song.volume = smoothTransition ? Mathf.Lerp(song.volume, song.maxVolume, speed) : Mathf.MoveTowards(song.volume, song.maxVolume, speed);
                    anyValueChange = true;
                }
            }
            else
            {
                if (song.volume > 0f)
                {
                    song.volume = smoothTransition ? Mathf.Lerp(song.volume, 0f, speed) : Mathf.MoveTowards(song.volume, 0f, speed);
                    anyValueChange = true;
                }
                else
                {
                    allSong.RemoveAt(i);
                    song.destroySong();
                    continue;
                }
            }
        }
        return anyValueChange;
    }

    public static AudioSource CreateNewSource(string _name)
    {
        AudioSource newSource = new GameObject(_name).AddComponent<AudioSource>();
        newSource.transform.SetParent(instance.transform);
        return newSource;
    }
    [System .Serializable ]
    public class SONG
    {
        public AudioSource source;
        public AudioClip clip { get { return source.clip; } set { source.clip = clip; } }
        public float maxVolume = 1f;

        public SONG(AudioClip clip, float _maxVolume, float pitch, float startingVolume, bool playOnStart, bool loop)
        {
            source = CreateNewSource(string.Format("SONG[{0]", clip.name));
            source.pitch = pitch;
            source.volume = startingVolume;
            maxVolume = -_maxVolume;
            source.loop = loop;

            if (playOnStart)
                source.Play();
            
        }
        public float volume { get { return source.volume; } set { source.volume = value; } }
        public float pitch { get { return source.volume; }set { source.pitch = value; } }
        public void play()
        {
            source.Play();
        }
        public void stop()
        {
            source.Stop();
        }
        public void pause()
        {
            source.Pause();
        }
        public void unPause()
        {
            source.UnPause();
        }
        public void destroySong()
        {
            AudioManager.allSong.Remove(this);
            DestroyImmediate(source.gameObject);
        } 
    }
}
