/*
* Energy Bar Toolkit by Mad Pixel Machine
* http://www.madpixelmachine.com
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class BarOptionsGUIMulti : MonoBehaviour {

    // ===========================================================
    // Constants
    // ===========================================================

    // ===========================================================
    // Fields
    // ===========================================================
    
    public Rect position = new Rect(10, 100, 200, 100);
    public GameObject[] energyBars;
    
    
    public GUISkin skin;
    
    public float valueF = 0.25f;
    

    // ===========================================================
    // Constructors (Including Static Constructors)
    // ===========================================================

    // ===========================================================
    // Getters / Setters
    // ===========================================================

    // ===========================================================
    // Methods for/from SuperClass/Interfaces
    // ===========================================================

    void Start() {
    }

    void Update() {
    }
    
    void OnGUI() {
        GUILayout.BeginArea(position);
        GUILayout.Label("Change Value:");
        valueF = GUILayout.HorizontalSlider(valueF, 0.0f, 1.0f);
        GUILayout.EndArea();
        
        foreach (var go in energyBars) {
            var bar = go.GetComponent<EnergyBar>();
            bar.ValueF = valueF;
        }
    }

    // ===========================================================
    // Methods
    // ===========================================================

    // ===========================================================
    // Static Methods
    // ===========================================================

    // ===========================================================
    // Inner and Anonymous Classes
    // ===========================================================

}