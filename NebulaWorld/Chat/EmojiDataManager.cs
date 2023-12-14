﻿#region

using System.Collections;
using System.Collections.Generic;
using NebulaModel.Utils;
using UnityEngine;

#endregion

namespace NebulaWorld.Chat;

public struct Emoji
{
    public string ShortName;
    public string Category;
    public string UnifiedCode;

    public int SheetX;
    public int SheetY;
    public int SortOrder;

    public Emoji(Dictionary<string, object> dict)
    {
        ShortName = (string)dict["short_name"];
        Category = (string)dict["category"];
        UnifiedCode = (string)dict["unified"];

        SheetX = (int)(long)dict["sheet_x"];
        SheetY = (int)(long)dict["sheet_y"];

        SortOrder = (int)(long)dict["sort_order"];
    }
}

public static class EmojiDataManager
{
    public static Dictionary<string, List<Emoji>> emojies = new();
    private static bool isLoaded;

    private static void Add(Emoji emoji)
    {
        if (emojies.ContainsKey(emoji.Category))
        {
            emojies[emoji.Category].Add(emoji);
        }
        else
        {
            emojies[emoji.Category] = new List<Emoji>(new[] { emoji });
        }
    }


    public static void ParseData(TextAsset asset)
    {
        if (isLoaded)
        {
            return;
        }

        var json = "{\"frames\":" + asset.text + "}";

        if (MiniJson.Deserialize(json) is Dictionary<string, object> jObject)
        {
            var array = jObject.ContainsKey("frames") ? jObject["frames"] as IList : null;
            if (array != null)
            {
                foreach (var rawJObject in array)
                {
                    if (!(rawJObject is Dictionary<string, object> emojiData))
                    {
                        continue;
                    }

                    var emoji = new Emoji(emojiData);
                    if (emoji.Category.Equals("People & Body"))
                    {
                        emoji.Category = "Smileys & Emotion";
                    }
                    Add(emoji);
                }
            }
        }

        foreach (var kv in emojies)
        {
            kv.Value.Sort((emoji1, emoji2) => emoji1.SortOrder.CompareTo(emoji2.SortOrder));
        }

        isLoaded = true;
    }
}
