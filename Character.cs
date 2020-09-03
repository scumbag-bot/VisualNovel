using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System .Serializable ]
public class Character 
{
    DIalogueSystem   dialogue;
    public string characterName;
    public bool isKnown = false;
    [HideInInspector]public RectTransform root;
    //public bool isMultilayerCharacter{ get { return renderers.renderer == null; } }
    public bool enabled { get { return root.gameObject.activeInHierarchy;} set { root.gameObject.SetActive(value); } }
    Vector2 targetPosition;
    public Vector2 anchorPadding { get { return root.anchorMax - root.anchorMin; } }

    public void say(string speech, bool add = false)
    {
        if (enabled == false)
            enabled = true;
        dialogue.Say(speech, characterName, add);
    }

    Coroutine moving;
    bool isMoving { get { return moving != null; } }

    public void moveTo(Vector2 target, float speed, bool smooth = true )
    {
        stopMoving();
        moving = CharacterManager.instance.StartCoroutine(Moving(target ,speed , smooth ));
    }

    public void stopMoving(bool ArriveAtTargetPositionImmediately= false )
    {
        if (isMoving)
        {
            CharacterManager.instance.StopCoroutine(moving);
            if (ArriveAtTargetPositionImmediately)
                setPosition(targetPosition);
        }
        moving = null;
    }

    public void setPosition(Vector2 target)
    {
        targetPosition = target;
        Vector2 padding = anchorPadding;
        float maxX = 1f - padding.x;
        float maxY = 1f - padding.y;
        Vector2 minAnchorTarget = new Vector2(maxX * targetPosition.x, maxY * targetPosition.y);

        root.anchorMin = minAnchorTarget;
        root.anchorMax = root.anchorMin + padding;

    }

    IEnumerator Moving(Vector2 target, float speed, bool smooth = true )
    {
        targetPosition = target;
        Vector2 padding = anchorPadding;
        float maxX = 1f - padding.x;
        float maxY = 1f - padding.y;
        Vector2 minAnchorTarget = new Vector2(maxX * targetPosition.x, maxY * targetPosition.y);
        speed *= Time.deltaTime;
        while (root.anchorMin != minAnchorTarget)
        {
            root.anchorMin = (!smooth) ? Vector2.MoveTowards(root.anchorMin, minAnchorTarget, speed) : Vector2.Lerp(root.anchorMin, minAnchorTarget, speed);
            root.anchorMax = root.anchorMin + padding;
            yield return new WaitForEndOfFrame();
        }
        stopMoving(); 
    }
    // Start Transition 
    public Sprite getSprite( string expression)
    {
        Sprite sprites = Resources.Load<Sprite>("Images/Character/" + characterName + "/" + expression);
        Debug.Log(characterName);
        return sprites;
    }
    public void setBody(string expression)
    {
        renderers.renderer.sprite = getSprite(expression);
    }
    public bool isTransitioninBody { get { return transitioningBody  != null; } }
    Coroutine transitioningBody = null;
    public void TransitionBody(Sprite sprite, float speed, bool smooth = true)
    {
        stopTransitioning ();
        CharacterManager.instance.StartCoroutine(TransitioningBody (sprite, speed, smooth));
    }
    void stopTransitioning()
    {
        if (isTransitioninBody )
        {
            CharacterManager.instance.StopCoroutine(transitioningBody );
        }
        transitioningBody  = null;
    }

    IEnumerator TransitioningBody(Sprite sprite, float speed, bool smooth =true )
    {
        for (int i = 0; i < renderers.allBodyRenderers.Count ; i++)
        {
            Image image = renderers.allBodyRenderers[i];
            if (image.sprite == image )
            {
                renderers.renderer = image;
                break; 
            }
        }
        if (renderers.renderer.sprite != sprite )
        {
            Image image = GameObject.Instantiate(renderers.renderer.gameObject , renderers.renderer.transform.parent ).GetComponent<Image>();
            renderers.allBodyRenderers.Add(image);
            renderers.renderer = image;
            image.color = GlobalF.SetAlpha(image.color, 0f);
            image.sprite = sprite;
        }
        while (GlobalF .TransitionImages(ref renderers.renderer, ref renderers.allBodyRenderers , speed , smooth))
        {
            yield return new WaitForEndOfFrame();
        }
        stopTransitioning ();
    }

    // End transition
    public void flip()
    {
        root.localScale = new Vector3(root.localScale.x * -1, 1f, 1f);
    }

    public void faceLeft()
    {
        root.localScale = Vector3.one;
    }

    public void faceRight()
    {
        root.localScale = new Vector3(-1f, 1f, 1f);
    }

    public Character(string _name, bool enabledOnStart = true )
    {
        CharacterManager cm = CharacterManager.instance;
        GameObject preflab = Resources.Load("Character/Character[" + _name + "]") as GameObject;
        GameObject ob = GameObject .Instantiate(preflab, cm.characterPanel);

        root = ob.GetComponent<RectTransform>();
        characterName  = _name;
        renderers.renderer = ob.GetComponentInChildren<Image>();
        renderers.allBodyRenderers.Add(renderers.renderer);
        
        dialogue = DIalogueSystem.instance;
        enabled = enabledOnStart;
    }

    [System.Serializable ]
    public class Renderers
    {
        public Image renderer;
        public List<Image> allBodyRenderers = new List<Image>(); 
    }
    public Renderers renderers = new Renderers();

}
