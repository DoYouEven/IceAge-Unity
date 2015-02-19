/*
* Energy Bar Toolkit by Mad Pixel Machine
* http://www.madpixelmachine.com
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace EnergyBarToolkit {

public class EnergyBarCommons {

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

    // ===========================================================
    // Static Methods
    // ===========================================================
    
    public static void SmoothDisplayValue(ref float displayValue, float target, float speed) {
        if (!Application.isPlaying) {
            // do not smooth if in edit mode
            displayValue = target;
            return;
        }
        
        float deltaTotal = target - displayValue;
        if (deltaTotal == 0) {
            return;
        }
        
        float delta;
        
        if (deltaTotal < 0) {
            delta = -speed;
        } else {
            delta = speed;
        }
        
        delta *= Time.deltaTime;
        
        if (Mathf.Abs(delta) > Mathf.Abs(deltaTotal)) {
            displayValue = target;
        } else {
            displayValue += delta;
        }
    }
    
    public static bool Blink(float val, float blinkVal, float rate, ref float accum) {
        if (val <= blinkVal) {
    
            float rate2 = rate * 2;
        
            if (rate > 0) {
                accum += Time.deltaTime;
                int times = (int) (accum / (1 / rate));
                accum -= (1 / rate) * times;
            }
            
            return accum > (1 / rate2);
        } else {
            return false;
        }
    }

    // ===========================================================
    // Inner and Anonymous Classes
    // ===========================================================

}

} // namespace