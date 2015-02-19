/*
* Energy Bar Toolkit by Mad Pixel Machine
* http://www.madpixelmachine.com
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using EnergyBarToolkit;

[ExecuteInEditMode]
public class BarOptionsGUI : MonoBehaviour {

    // ===========================================================
    // Constants
    // ===========================================================
    
    public const string FormatHelp =
        @"Format Help:
        
{cur} - current int value
{min} - minimal value
{max} - maximum value
{cur%} - current % value (0 - 100)
{cur2%} - current % value with precision of tens (0.0 - 100.0)

Examples:
{cur}/{max} - 27/100
{cur%} % - 27 %";

    // ===========================================================
    // Fields
    // ===========================================================
    
    public GameObject bar;
    public Rect position = new Rect(10, 100, 200, 100);
    public GUISkin skin;
    
    public bool allowBurn = true;
    public bool allowChangeBarColor = false;
    public bool changeBurnCollorAuto = true;
    
    public bool allowChangeDirection = false;
    
    private EnergyBar energyBar;
    private EnergyBarRenderer energyBarRenderer;

    // ===========================================================
    // Constructors (Including Static Constructors)
    // ===========================================================

    // ===========================================================
    // Getters / Setters
    // ===========================================================

    // ===========================================================
    // Methods
    // ===========================================================
    
    void Start() {
    }
    
    void OnGUI() {
        GUI.skin = skin;
    
        energyBar = bar.GetComponent<EnergyBar>();
        energyBarRenderer = bar.GetComponent<EnergyBarRenderer>();
    
        GUILayout.BeginArea(position);
    
        GUILayout.Label("Change Value:");
        energyBar.valueCurrent = (int) GUILayout.HorizontalSlider(energyBar.valueCurrent, energyBar.valueMin, energyBar.valueMax);
    
        if (energyBarRenderer != null) {
            if (allowBurn) {
                energyBarRenderer.effectBurn = GUILayout.Toggle(energyBarRenderer.effectBurn, "Burn Effect (on subtraction)", skin.toggle);
            }
            
            energyBarRenderer.effectSmoothChange = GUILayout.Toggle(energyBarRenderer.effectSmoothChange, "Smooth Change", skin.toggle);
            energyBarRenderer.effectBlink = GUILayout.Toggle(energyBarRenderer.effectBlink, "Blink when energy is low (NEW!)", skin.toggle);
            
            if (allowChangeBarColor) {
                GUILayout.Space(10);
                GUILayout.BeginHorizontal();
                if (GUILayout.Toggle(energyBarRenderer.textureBarColorType == EnergyBarRenderer.ColorType.Solid, "Solid Color")) {
                    energyBarRenderer.textureBarColorType = EnergyBarRenderer.ColorType.Solid;
                }
                
                if (GUILayout.Toggle(energyBarRenderer.textureBarColorType == EnergyBarRenderer.ColorType.Gradient, "Gradient (NEW!)")) {
                    energyBarRenderer.textureBarColorType = EnergyBarRenderer.ColorType.Gradient;
                }
                GUILayout.EndHorizontal();
                
                GUI.enabled = energyBarRenderer.textureBarColorType == EnergyBarRenderer.ColorType.Solid;
                GUILayout.BeginHorizontal();
                GUILayout.Label("R:");
                energyBarRenderer.textureBarColor.r = GUILayout.HorizontalSlider(energyBarRenderer.textureBarColor.r, 0, 1);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("G:");
                energyBarRenderer.textureBarColor.g = GUILayout.HorizontalSlider(energyBarRenderer.textureBarColor.g, 0, 1);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("B:");
                energyBarRenderer.textureBarColor.b = GUILayout.HorizontalSlider(energyBarRenderer.textureBarColor.b, 0, 1);
                GUILayout.EndHorizontal();
                GUI.enabled = false;
                
                if (changeBurnCollorAuto) {
                    energyBarRenderer.effectBurnTextureBarColor = energyBarRenderer.textureBarColor;
                    energyBarRenderer.effectBurnTextureBarColor.a = 0.5f;
                }
            }
            
            if (allowChangeDirection) {
                GUILayout.Space(10);
                GUILayout.Label("Direction: ");
                GUILayout.BeginHorizontal();
                if (GUILayout.Toggle(energyBarRenderer.growDirection == EnergyBarRenderer.GrowDirection.LeftToRight, "L -> R")) {
                    energyBarRenderer.growDirection = EnergyBarRenderer.GrowDirection.LeftToRight;
                }
                if (GUILayout.Toggle(energyBarRenderer.growDirection == EnergyBarRenderer.GrowDirection.TopToBottom, "T -> B")) {
                    energyBarRenderer.growDirection = EnergyBarRenderer.GrowDirection.TopToBottom;
                }
                if (GUILayout.Toggle(energyBarRenderer.growDirection == EnergyBarRenderer.GrowDirection.RightToLeft, "R -> L")) {
                    energyBarRenderer.growDirection = EnergyBarRenderer.GrowDirection.RightToLeft;
                }
                if (GUILayout.Toggle(energyBarRenderer.growDirection == EnergyBarRenderer.GrowDirection.BottomToTop, "B -> T")) {
                    energyBarRenderer.growDirection = EnergyBarRenderer.GrowDirection.BottomToTop;
                }
                GUILayout.EndHorizontal();
            }
            
            if (energyBarRenderer.labelEnabled) {
                GUILayout.Space(20);
                GUILayout.Label("Label Format:");
                energyBarRenderer.labelFormat = GUILayout.TextField(energyBarRenderer.labelFormat);
                GUILayout.Label(FormatHelp);
            }
        }
        
        GUILayout.EndArea();
        
        GUI.skin = null;
    }

    // ===========================================================
    // Static Methods
    // ===========================================================

    // ===========================================================
    // Inner and Anonymous Classes
    // ===========================================================

}