/*
* Copyright (c) Mad Pixel Machine
* http://www.madpixelmachine.com/
*/

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace EnergyBarToolkit {

public class ColorBind : MonoBehaviour {

    #region Fields

    public EnergyBarBase.Tex tex;

    private MadSprite sprite;

    #endregion

    #region Slots

    void OnEnable() {
        sprite = GetComponent<MadSprite>();
    }

    void Update() {
        if (tex != null) {
            sprite.tint = tex.color;
        }
    }

    #endregion
}
}