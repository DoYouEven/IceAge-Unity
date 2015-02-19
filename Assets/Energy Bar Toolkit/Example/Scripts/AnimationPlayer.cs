/*
* Energy Bar Toolkit by Mad Pixel Machine
* http://www.madpixelmachine.com
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace EnergyBarToolkit {
 
[ExecuteInEditMode]   
public class AnimationPlayer : MonoBehaviour {

    // ===========================================================
    // Constants
    // ===========================================================

    // ===========================================================
    // Fields
    // ===========================================================
    
    public int x = 100;
    public int y = 100;
    public Animation anim;
    public EnergyBar[] animatedBars;

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
        if (GUI.Button(new Rect(x, y, 150, 40), "Play Animation")) {
            foreach (var bar in animatedBars) {
                bar.animationEnabled = true;
            }
            anim.Play();
        }
        
        if (GUI.Button(new Rect(x, y + 50, 150, 40), "Stop Animation")) {
            foreach (var bar in animatedBars) {
                bar.animationEnabled = false;
            }
            anim.Stop();
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
