using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GlobalF : MonoBehaviour
{
    public static bool TransitionImages(ref Image activeImage, ref List<Image> allImages, float speed, bool smooth, bool fasterInTime = false)
    {
        bool anyValueChange = false;
        speed *= Time.deltaTime;
        for (int i = allImages.Count - 1; i >= 0; i--)
        {
            Image image = allImages[i];
            if (image == activeImage)
            {
                if (image.color.a < 1f)
                {
                    float spd = fasterInTime ? speed * 2 : speed;
                    image.color = SetAlpha(image.color, smooth ? Mathf.Lerp(image.color.a, 1f, spd) : Mathf.MoveTowards(image.color.a, 1f, spd));
                    anyValueChange  = true;
                }
            }
            else
            {
                if (image.color.a > 0)
                {
                    image.color = SetAlpha(image.color, smooth ? Mathf.Lerp(image.color.a, 0f, speed) : Mathf.MoveTowards(image.color.a, 0f, speed));
                    anyValueChange  = true;
                }
                else
                {
                    allImages.RemoveAt(i);
                    DestroyImmediate(image.gameObject);
                    continue;
                }
            }
        }

        return anyValueChange;
    }

    public static Color SetAlpha(Color color, float alpha)
    {
        return new Color(color.r, color.g, color.b, alpha);
    }
}
