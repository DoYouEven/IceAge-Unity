/*
* Energy Bar Toolkit by Mad Pixel Machine
* http://www.madpixelmachine.com
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace EnergyBarToolkit {
 
[ExecuteInEditMode]   
public class MultiBarSliders : MonoBehaviour {

    // ===========================================================
    // Constants
    // ===========================================================

    // ===========================================================
    // Fields
    // ===========================================================
    
    public Bar[] bars;
    public Rect area = new Rect(10, 10, 140, 400);

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
        if (bars == null) {
            return;
        }
        
        GUILayout.BeginArea(area);
    
        foreach (Bar bar in bars) {
            GUILayout.Label(bar.label);
            float val = 0;
            if (bar.bar != null) {
                val = bar.bar.ValueF;
            }
            val = GUILayout.HorizontalSlider(val, 0, 1);
            if (Application.isPlaying) {
                bar.bar.ValueF = val;
            }
        }
        
        GUILayout.EndArea();
    }

    // ===========================================================
    // Static Methods
    // ===========================================================

    // ===========================================================
    // Inner and Anonymous Classes
    // ===========================================================
    
    [System.Serializable]
    public class Bar {
        public EnergyBar bar;
        public string label = "Edit Me!";
    }

}

} // namespace
