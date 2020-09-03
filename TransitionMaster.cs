using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TransitionMaster : MonoBehaviour
{
    public static TransitionMaster instance;
    public Material transitionMaterialInPrefab;
    public RawImage overlayImage;
    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
        overlayImage.material = new Material(transitionMaterialInPrefab);
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q ))
        {
            ShowScene(!sceneVisible);
        }
    }
    static  bool sceneVisible = true;
    public static void ShowScene(bool show, float speed = 1f, bool smooth = false , Texture2D transitionEffect = null )
    {
        if (transitioningOverlay != null)
            instance.StopCoroutine(transitioningOverlay);
        sceneVisible = show;

        if (transitionEffect != null)
            instance.overlayImage.material.SetTexture("_AlphaTex", transitionEffect);
        transitioningOverlay = instance.StartCoroutine(TransitioningOverlay(show, speed, smooth));
    }
    static Coroutine transitioningOverlay = null;
    static IEnumerator TransitioningOverlay(bool show, float speed, bool smooth)
    {
        speed *= Time.deltaTime;
        float targVal = show ? 1 : 0;
        float curVal = instance.overlayImage.material.GetFloat("_Cutoff");
        while (curVal != targVal )
        {
            curVal = smooth ? Mathf.Lerp(curVal, targVal, speed) : Mathf.MoveTowards(curVal, targVal, speed);
            instance.overlayImage.material.SetFloat("_Cutoff", curVal);
            yield return new WaitForEndOfFrame();
        }
        transitioningOverlay = null;
    }

    //Transisi layer
    public static void TransitionLayer(BCFC.LAYER layer, Texture2D targetImage, Texture2D transitionEffect, float speed = 1f, bool smooth = true  )
    {
        if (layer.specialTransitionCoroutine != null)
            instance.StopCoroutine(layer.specialTransitionCoroutine);

        if (targetImage != null)
            layer.specialTransitionCoroutine = instance.StartCoroutine(TransitioningLayer(layer, targetImage, transitionEffect, speed, smooth));
        else
            layer.specialTransitionCoroutine = instance.StartCoroutine(TransitioningLayerToNull(layer, transitionEffect, speed, smooth));
    }
    static IEnumerator TransitioningLayer(BCFC.LAYER layer, Texture2D targetImage, Texture2D transitionEffect, float speed = 1, bool smooth = false )
    {
        GameObject ob = Instantiate(layer.newImageObjectReference, layer.newImageObjectReference.transform.parent) as GameObject;
        ob.SetActive(true);

        RawImage im = ob.GetComponent<RawImage>();
        im.texture = targetImage;

        layer.activeImage = im;
        layer.allImage.Add(im);

        im.material = new Material(instance.transitionMaterialInPrefab);
        im.material.SetTexture("_ AlphaTex", transitionEffect);
        im.material.SetFloat("_Cutoff", 1);
        float curVal = 1;
        while (curVal > 0)
        {
            curVal = smooth ? Mathf.Lerp(curVal, 0, speed * Time.deltaTime ) : Mathf.MoveTowards(curVal, 0, speed * Time.deltaTime );
            im.material.SetFloat("_Cutoff", curVal);
            yield return new WaitForEndOfFrame();
        }

        if (im != null )
        {
            im.material = null;
            im.color = GlobalF.SetAlpha(im.color, 1);
        }

        for (int i = 0; i < layer.allImage.Count -1; i++)
        {
            if (layer.allImage[i] == layer.activeImage && layer.activeImage != null)
                continue;
            if (layer.allImage[i] != null)
                Destroy(layer.allImage[i].gameObject , 0.01f);

            layer.allImage.RemoveAt(i);
        }
        layer.specialTransitionCoroutine = null;
    }
    static IEnumerator TransitioningLayerToNull(BCFC.LAYER layer, Texture2D transitionEffect, float speed = 1, bool smooth = false)
    {
        List<RawImage> currentImageOnLayer = new List<RawImage>();
        foreach (RawImage  r in layer.allImage )
        {
            r.material = new Material(instance.transitionMaterialInPrefab);
            r.material.SetTexture("_AlphaTex", transitionEffect);
            r.material.SetFloat("_Cutoff", 0);
            currentImageOnLayer.Add(r);
        }
        float curVal = 0;
        while (curVal < 1)
        {
            curVal = smooth ? Mathf.Lerp(curVal, 1, speed * Time.deltaTime ) : Mathf.MoveTowards(curVal, 1, speed * Time.deltaTime );
            for (int i = 0; i < layer.allImage .Count ; i++)
            {
                layer.allImage[i].material.SetFloat("_Cutoff", curVal);
            }
            yield return new WaitForEndOfFrame();
        }
        foreach (RawImage r in currentImageOnLayer)
        {
            layer.allImage.Remove(r);
            if (r != null)
                Destroy(r.gameObject, 0.01f);
        }
        layer.specialTransitionCoroutine = null;
    }
}
