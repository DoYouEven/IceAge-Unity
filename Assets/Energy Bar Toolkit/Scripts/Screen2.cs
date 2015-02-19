/*
* Energy Bar Toolkit by Mad Pixel Machine
* http://www.madpixelmachine.com
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace EnergyBarToolkit {

public class Screen2 {

    // ===========================================================
    // Constants
    // ===========================================================

    // ===========================================================
    // Fields
    // ===========================================================

    // ===========================================================
    // Methods for/from SuperClass/Interfaces
    // ===========================================================

    // ===========================================================
    // Methods
    // ===========================================================

    public static int width {
        get {
            #if UNITY_EDITOR
            string[] res = UnityStats.screenRes.Split('x');
            return int.Parse(res[0]);
            #else
            return Screen.width;
            #endif
        }
    }
    
    public static int height {
        get {
            #if UNITY_EDITOR
            string[] res = UnityStats.screenRes.Split('x');
            return int.Parse(res[1]);
            #else
            return Screen.height;
            #endif
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