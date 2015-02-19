/*
* Energy Bar Toolkit by Mad Pixel Machine
* http://www.madpixelmachine.com
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace EnergyBarToolkit {

[ExecuteInEditMode]
public class EnergyBarPresentationColorize : MonoBehaviour {

    // ===========================================================
    // Constants
    // ===========================================================

    // ===========================================================
    // Fields
    // ===========================================================
    
    public Rect position;
    public EnergyBarRenderer b1, b2, b3, b4, b5, b6;
    public bool colorized;
    
    public Color b1a, b1b, b2a, b2b, b3a, b3b, b4a, b4b, b5a, b5b, b6a, b6b;

    // ===========================================================
    // Methods for/from SuperClass/Interfaces
    // ===========================================================

    // ===========================================================
    // Methods
    // ===========================================================

    void Start() {
    }

    void Update() {
    }
    
    void OnGUI() {
        if (colorized = GUI.Toggle(position, colorized, "Colorize!")) {
            b1.textureForegroundColor = b1a;
            b1.textureBarColor = b1b;
            b2.textureForegroundColor = b2a;
            b2.textureBarColor = b2b;
            b3.textureForegroundColor = b3a;
            b3.textureBarColor = b3b;
            b4.textureForegroundColor = b4a;
            b4.textureBarColor = b4b;
            b5.textureForegroundColor = b5a;
            b5.textureBarColor = b5b;
            b6.textureForegroundColor = b6a;
            b6.textureBarColor = b6b;
        } else {
            foreach (var b in new EnergyBarRenderer[] { b1, b2, b3, b4, b5, b6 }) {
                b.textureBarColor = Color.white;
                b.textureForegroundColor = Color.white;
            }
        }
    }

    // ===========================================================
    // Static Methods
    // ===========================================================

    // ===========================================================
    // Inner and Anonymous Classes
    // ===========================================================

}

} // namespace
