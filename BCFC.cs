using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class BCFC : MonoBehaviour
{
    public static BCFC instance;
    public VideoClip videoClip;
    public LAYER background = new LAYER ();
    public LAYER cinematic = new LAYER();
    public LAYER foreground = new LAYER();
    void Awake()
    {
        instance = this;
    }
    [System.Serializable ]
    public class LAYER
    {
        public GameObject root;
        public GameObject newImageObjectReference;
        public RawImage activeImage;
        public List<RawImage> allImage = new List<RawImage>();
        public Coroutine specialTransitionCoroutine = null;
        public void setTexture(Texture texture)
        {
            if (texture != null )
            {
                if (activeImage == null)
                    createNewActiveImage();
                activeImage.texture = texture;
                activeImage.color = GlobalF.SetAlpha(activeImage.color, 1f);
            }
            else
            {
                if (activeImage != null)
                {
                    allImage.Remove(activeImage);
                    GameObject.DestroyImmediate(activeImage.gameObject );
                    activeImage = null;
                }
            }
        }
        void createNewActiveImage()
        {
            root.SetActive(true);
            GameObject ob = Instantiate(newImageObjectReference, root.transform) as GameObject;
            ob.SetActive(true);
            RawImage raw = ob.GetComponent<RawImage>();
            activeImage = raw;
            allImage.Add(raw);
        }

    }
}
