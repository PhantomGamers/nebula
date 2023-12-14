﻿#region

using NebulaWorld.MonoBehaviours.Local;
using UnityEngine;

#endregion

namespace NebulaWorld.Chat;

public interface IChatLinkHandler
{
    void OnClick(string data);

    void OnHover(string data, ChatLinkTrigger trigger, ref MonoBehaviour tipObject);

    string GetIconName(string data);

    string GetDisplayRichText(string data);
}
