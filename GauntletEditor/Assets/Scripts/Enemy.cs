﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : GameScriptable
{
    public enum Type
    {
        Collider,
        CloseRangeAttacker,
        ProjectileThrower
    }
}
