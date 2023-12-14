﻿#region

using System;
using NebulaModel.DataStructures;
using UnityEngine.UI;

#endregion

namespace NebulaWorld.Factory;

public class StorageManager : IDisposable
{
    public readonly ToggleSwitch IsIncomingRequest;
    public Slider ActiveBansSlider;
    public Text ActiveBansValueText;
    public StorageComponent ActiveStorageComponent;
    public UIStorageGrid ActiveUIStorageGrid;
    public Text ActiveWindowTitle;
    public bool IsHumanInput = false;
    public bool WindowOpened = false;

    public StorageManager()
    {
        IsIncomingRequest = new ToggleSwitch();
    }

    public void Dispose()
    {
    }
}
