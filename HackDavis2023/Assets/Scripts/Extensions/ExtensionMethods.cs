using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtensionMethods
{
    public static void SetVisible(this CanvasGroup group, bool visible)
    {
        group.alpha = visible ? 1 : 0;
        group.blocksRaycasts = visible;
    }

    public static TimeSpan GetTypeWriterDelay(this char c)
    {
        if (c == '.' || c == ',' || c == ';' || c == '?' || c == '!')
        {
            return TimeSpan.FromSeconds(0.17);
        }
        else if(c == ' ')
            return TimeSpan.FromSeconds(0.07);  
        return TimeSpan.FromSeconds(0.05);
    }
}
