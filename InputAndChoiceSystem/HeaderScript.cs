﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HeaderScript : MonoBehaviour
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
        }
        if (isRevealing)
            StopCoroutine(revealing);
        revealing = StartCoroutine(Revealing());

        cachedLastPosition = true;
    }
    public void Hide()
    {
        if (isRevealing)
            StopCoroutine(revealing);
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

        while (banner.color.a < 1 || banner.transform.position != cachedBannerOriginalPosition)
        {
            banner.color = GlobalF.SetAlpha(banner.color, Mathf.MoveTowards(banner.color.a, 1, fadeSpeed * Time.unscaledDeltaTime));
            titleText.color = GlobalF.SetAlpha(titleText.color, banner.color.a);

            banner.transform.position = Vector3.MoveTowards(banner.transform.position, cachedBannerOriginalPosition, 11 * fadeSpeed * Time.unscaledDeltaTime);
            yield return new WaitForEndOfFrame();
        }

    }
}
