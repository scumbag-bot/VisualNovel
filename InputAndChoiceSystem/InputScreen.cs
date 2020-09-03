using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InputScreen : MonoBehaviour
{
    public static InputScreen instance;
    public TMP_InputField inputField;
    /// <summary>
    /// current input declared by the player
    /// </summary>
    public static string currentInput { get { return instance.inputField.text; } }
    public GameObject root;
    public TitleHeader header;
    void Awake()
    {
        instance = this;
        Hide();
    }

    public static void Show(string title, bool clearCurrentInput = true )
    {
        instance.root.SetActive(true);
        if (clearCurrentInput)
            instance.inputField.text = "";
        if (title != "")
            instance.header.Show(title);
        else
            instance.header.Hide();

        if (isRevealing)
            instance.StopCoroutine(revealing);
        revealing = instance.StartCoroutine(Revealing());
    }
    public static void Hide()
    {
        instance.root.SetActive(false);
        instance.header.Hide();
    }

    public static bool isWaitingForUserInput { get { return instance.root.gameObject.activeInHierarchy; } }
    public static bool isRevealing { get { return revealing != null; } }
    static Coroutine revealing = null;
    static IEnumerator Revealing()
    {
        instance.inputField.gameObject.SetActive(false);
        while (instance.header.isRevealing)
            yield return new WaitForEndOfFrame();
        instance.inputField.gameObject.SetActive(true);
        revealing = null;
    }
    public void Accept()
    {
        Hide();
    }

    [System.Serializable ]
    public class TitleHeader
    {
        public Image banner;
        public TextMeshProUGUI titleText;
        public string title { get { return titleText.text; } set { titleText.text = value; } }

        public enum DISPLAY_METHOD
        {
            instant,
            slowFade,
            typeWritter,
            floatingSlowFade
        }
        public DISPLAY_METHOD displayMethod = DISPLAY_METHOD.instant;
        public float fadeSpeed = 1f;
        public void Show(string displayTitle)
        {
            title = displayTitle;
            if (!cachedLastPosition)
            {
                cachedBannerOriginalPosition = banner.transform.position;
                print("jalan");
            }
            if (isRevealing)
                instance.StopCoroutine(revealing);
            revealing = instance.StartCoroutine(Revealing());
            
                cachedLastPosition = true;
        }
        public void Hide()
        {
            if (isRevealing)
                instance.StopCoroutine(revealing);
            revealing = null;
            if (cachedLastPosition)
                banner.transform.position = cachedBannerOriginalPosition;
            banner.enabled = false;
            titleText.enabled = false;
        }

        public bool isRevealing { get { return revealing != null; } }
        Coroutine revealing = null;
        IEnumerator Revealing()
        {
            banner.enabled = true;
            titleText.enabled = true;

            switch (displayMethod)
            {
                case DISPLAY_METHOD.instant:
                    banner.color = GlobalF.SetAlpha(banner.color, 1);
                    titleText.color = GlobalF.SetAlpha(titleText.color, 1);
                    break;
                case DISPLAY_METHOD.slowFade:
                    yield return SlowFade();
                    break;
                case DISPLAY_METHOD.typeWritter:
                    yield return typeWritter();
                    break;
                case DISPLAY_METHOD.floatingSlowFade:
                    yield return floatingFade();
                    break;
            }
            revealing = null;
        }

        IEnumerator SlowFade()
        {
            banner.color = GlobalF.SetAlpha(banner.color, 1);
            titleText.color = GlobalF.SetAlpha(titleText.color, 1);
            while (banner.color.a < 1)
            {
                banner.color = GlobalF.SetAlpha(banner.color, Mathf.MoveTowards(banner.color.a, 1, fadeSpeed * Time.unscaledDeltaTime));
                titleText.color = GlobalF.SetAlpha(titleText.color, Mathf.MoveTowards(titleText.color.a, 1, fadeSpeed * Time.unscaledDeltaTime));
                yield return new WaitForEndOfFrame();
            }
        }
        IEnumerator typeWritter()
        {
            banner.color = GlobalF.SetAlpha(banner.color, 1);
            titleText.color = GlobalF.SetAlpha(titleText.color, 1);
            TextArchitect architect = new TextArchitect(titleText, title);
            while (architect.isConstructing)
                yield return new WaitForEndOfFrame();
        }
        bool cachedLastPosition = false;
        Vector3 cachedBannerOriginalPosition = Vector3.zero;
        IEnumerator floatingFade()
        {
            banner.color = GlobalF.SetAlpha(banner.color, 0);
            titleText.color = GlobalF.SetAlpha(titleText.color, 0);

            float amount = 25f * ((float)Screen.height / 720f);
            Vector3 downPos = new Vector3(0, amount, 0);
            banner.transform.position = cachedBannerOriginalPosition - downPos;

            while (banner.color.a < 1 || banner.transform.position != cachedBannerOriginalPosition )
            {
                banner.color = GlobalF.SetAlpha(banner.color, Mathf.MoveTowards(banner.color.a, 1, fadeSpeed * Time.unscaledDeltaTime));
                titleText.color = GlobalF.SetAlpha(titleText.color, banner.color.a);

                banner.transform.position = Vector3.MoveTowards(banner.transform.position, cachedBannerOriginalPosition, 11 * fadeSpeed * Time.unscaledDeltaTime);
                yield return new WaitForEndOfFrame();
            }

        }
    }
    public Image banner { get { return header.banner; } }
    public TextMeshProUGUI titleText { get { return header.titleText; } }
}
