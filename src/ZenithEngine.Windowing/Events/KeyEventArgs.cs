﻿using ZenithEngine.Windowing.Enums;

namespace ZenithEngine.Windowing.Events;

public class KeyEventArgs(Key key, KeyModifiers modifiers) : EventArgs
{
    public Key Key { get; } = key;

    public KeyModifiers Modifiers { get; } = modifiers;
}

