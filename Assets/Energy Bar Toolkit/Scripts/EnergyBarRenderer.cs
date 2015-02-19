/*
* Energy Bar Toolkit by Mad Pixel Machine
* http://www.madpixelmachine.com
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace EnergyBarToolkit {

[ExecuteInEditMode]
[RequireComponent(typeof(EnergyBar))]
public class EnergyBarRenderer : EnergyBarOnGUIBase {

    // ===========================================================
    // Constants
    // ===========================================================
    
    public static readonly Color Transparent = new Color(1, 1, 1, 0);
    
    // ===========================================================
    // Fields
    // ===========================================================
    public Vector2 screenPosition = new Vector2(10, 10);
    public bool screenPositionNormalized; // indicates that all coordinates are normalized to screen width and height
    
    public Vector2 size = new Vector2(100, 20);
    
    private Vector2 sizeReal;
    private Vector2 screenPositionReal;
    
    public Texture2D textureBackground; // deprecated
    public Color textureBackgroundColor = Color.white; // deprecated
    
    
    public Texture2D textureBar;
    private Texture2D _textureBar;  // here is stored last texture bar set
    
    public ColorType textureBarColorType = ColorType.Solid;
    public Color textureBarColor = Color.white;
    public Gradient textureBarGradient;
    
    public Texture2D textureForeground; // deprecated
    public Color textureForegroundColor = Color.white; // deprecated
    
    public GrowDirection growDirection = GrowDirection.LeftToRight;
    
    // radial parametters
    public RotatePoint radialRotatePoint = RotatePoint.VisibleAreaCenter;
    public Vector2 radialCustomCenter;
    public float radialOffset = 0.0f;
    public float radialLength = 1.0f;
    
    private Rect textureBarVisibleBounds;
    private Rect textureBarVisibleBoundsOrig;
    
    //
    // effects
    //
    
    // blink effect
    public bool effectBlink = false;
    public float effectBlinkValue = 0.2f;
    public float effectBlinkRatePerSecond = 1f;
    public Color effectBlinkColor = new Color(1, 1, 1, 0);
    
    private float effectBlinkAccum;
    private bool effectBlinkVisible = false;
    
    // sprite on edge effect (still not finished feature)
    public bool effectEdge;
    public Texture2D effectEdgeTexture;
    public float effectEdgeIn = 0.2f;
    public float effectEdgeOut = 0.2f;
    public float effectEdgeRotateAngle = 100;
    
    // override this if you want to build your own behaviour
    public EdgeEffectFunction edgeEffectFunction;
    
    // deprecated
    [SerializeField] private bool sizeNormalized;
    [SerializeField] private bool screenPositionCalculateSize = true;
    
    // ===========================================================
    // Constructors (Including Static Constructors)
    // ===========================================================

    // ===========================================================
    // Getters / Setters
    // ===========================================================
    
    private Vector2 ScreenPositionPixels {
        get {
            if (screenPositionNormalized) {
                return new Vector2(screenPosition.x * Screen2.width, screenPosition.y * Screen2.height);
            } else {
                return screenPosition;
            }
        }
    }
    
    public override Vector2 SizePixels {
        get {
            return GetSizePixels(size);
        }
        
        set {
            SetSizePixels(ref size, value);
        }
    }
    
    public override Vector2 TextureSizePixels {
        get {
            if (textureBar == null) {
                return Vector2.one;
            }
            
            return new Vector2(textureBar.width, textureBar.height);
        }
    }
    
    // ===========================================================
    // Methods for/from SuperClass/Interfaces
    // ===========================================================
    
    public override Rect DrawAreaRect {
        get {
//            sizeReal = Round(SizePixels);
//            screenPositionReal = RealPosition(Round(ScreenPositionPixels), SizePixels);
            
            var rect = new Rect(screenPositionReal.x, screenPositionReal.y, sizeReal.x, sizeReal.y);
            return rect;
        }
    }
    

    // ===========================================================
    // Methods
    // ===========================================================
    
    void OnValidate() {
        effectEdgeIn = Mathf.Clamp01(effectEdgeIn);
        effectEdgeOut = Mathf.Clamp01(effectEdgeOut);
    }
    
    protected override void OnEnable() {
        base.OnEnable();
        edgeEffectFunction = GUIDrawEdgeEffectFun;
    }
    
    protected override void Start() {
        base.Start();
        
        if (version == 169) {
            Upgrade_169_171();
        }
        
        // moving deprecated fields to new one
        if (texturesBackground.Length == 0 && textureBackground != null) {
            Array.Resize(ref texturesBackground, 1);
            var tex = new Tex();
            tex.texture = textureBackground;
            tex.color = textureBackgroundColor;
            texturesBackground[0] = tex;
            
            textureBackground = null;
        }
        
        if (texturesForeground.Length == 0 && textureForeground != null) {
            Array.Resize(ref texturesForeground, 1);
            var tex = new Tex();
            tex.texture = textureForeground;
            tex.color = textureForegroundColor;
            texturesForeground[0] = tex;
            
            textureForeground = null;
        }
    }
    
    #region UpgradeProcedures
    void Upgrade_169_171() {
        // I cannot be sure if this component was created on 169 or 170
        // What I am doing is safe-upgrade
        if (sizeNormalized) {
            resizeMode = EnergyBarOnGUIBase.ResizeMode.Stretch;
        } else if (!screenPositionCalculateSize) {
            resizeMode = EnergyBarOnGUIBase.ResizeMode.Fixed;
        }
        
        version = 171;
    }
    #endregion
    
#region Update

    // updates sizing of textures. Need to be called in Update and OnGUI methods
    // in other way it will lead to bad scaling effect in Editor preview
    void UpdateSize() {
        sizeReal = Round(SizePixels);
        screenPositionReal = RealPosition(Round(ScreenPositionPixels), SizePixels);
    }

    protected override void Update() {
        base.Update();

        UpdateSize();
    
        if (!IsValid()) {
            return;
        }
    
        if (textureBar != _textureBar) { // texture bar dirty?
            textureBarVisibleBoundsOrig = FindBounds(textureBar);
            if (textureBarVisibleBoundsOrig.width == 0) {  // not readable (yet)
                return;
            }
            
            textureBarVisibleBounds = textureBarVisibleBoundsOrig;
            _textureBar = textureBar;
        }
        
//        FixRatio();
        
        // update blink effect
        if (effectBlink) {
            effectBlinkVisible = EnergyBarCommons.Blink(
                energyBar.ValueF, effectBlinkValue, effectBlinkRatePerSecond, ref effectBlinkAccum);
        }
    }
    
    private bool IsValid() {
        return textureBar != null;
    }
#endregion
    
#region OnGUI
    new public void OnGUI() {
        base.OnGUI();
        
        if (!RepaintPhase()) {
            return;
        }
        
        if (!IsVisible()) {
            return;
        }
    
        if (!IsValid()) {
            return;
        }
        
        UpdateSize();
        
        GUIDrawBackground();
    
        if (effectBurn && ValueFBurn != 0) {
            var texture = effectBurnTextureBar != null ? effectBurnTextureBar : textureBar;
            DrawBar(ValueFBurn, GetTextureBurnColor(), texture);
        }

        if (ValueF2 != 0) {
            DrawBar(ValueF2, GetTextureBarColor(), textureBar);
        }
        
        GUIDrawForeground();
        
        GUIDrawEdgeEffect();
        
        GUIDrawLabel();
    }
    
    private Color GetTextureBarColor() {
        // if effect blink enabled, return blink color if blink is currently visible
        // or return regular color if blink is not visible
        if (effectBlink && effectBlinkVisible) {
            return effectBlinkColor;
        }
        
        if (growDirection == GrowDirection.ColorChange) {
            return textureBarGradient.Evaluate(energyBar.ValueF);
        }
    
        switch (textureBarColorType) {
            case ColorType.Solid:
                return textureBarColor;
            case ColorType.Gradient:
                return textureBarGradient.Evaluate(energyBar.ValueF);
            default:
                Debug.LogError("Unknown texture bar type! This is a bug! Please report this.");
                return Color.white;
        }
    }
    
    private Color GetTextureBurnColor() {
        // when blinking bisible do not display burn bar at all
        if (effectBlink && effectBlinkVisible) {
            return Transparent;
        }
        
        return effectBurnTextureBarColor;
    }
    
    private void DrawBar(float value, Color color, Texture2D texture) {
        var rect = new Rect(screenPositionReal.x, screenPositionReal.y, sizeReal.x, sizeReal.y);
        var visibleRect = textureBarVisibleBounds;
        
        switch (growDirection) {
            case GrowDirection.LeftToRight:
                DrawTextureHorizFill(rect, texture, visibleRect, color, false, value);
                break;
            case GrowDirection.RightToLeft:
                DrawTextureHorizFill(rect, texture, visibleRect, color, true, value);
                break;
            case GrowDirection.TopToBottom:
                DrawTextureVertFill(rect, texture, visibleRect, color, false, value);
                break;
            case GrowDirection.BottomToTop:
                DrawTextureVertFill(rect, texture, visibleRect, color, true, value);
                break;
            case GrowDirection.RadialCW:
                DrawTextureRadialFill(rect, texture, color, false, value, radialOffset, radialLength);
                break;
            case GrowDirection.RadialCCW:
                DrawTextureRadialFill(rect, texture, color, true, value, radialOffset, radialLength);
                break;
            case GrowDirection.ExpandHorizontal:
                DrawTextureExpandFill(rect, texture, visibleRect, color, false, value);
                break;
            case GrowDirection.ExpandVertical:
                DrawTextureExpandFill(rect, texture, visibleRect, color, true, value);
                break;
            case GrowDirection.ColorChange:
                DrawTexture(rect, texture, color);
                break;
            default:
                Debug.LogError("Unknown grow direction: " + growDirection);
                break;
        }
        
    }
    
    private Rect TexCoordsForEnergyBar(Rect energyBounds) {
        var pos = screenPositionReal;
        var s = sizeReal;
    
        switch (growDirection) {
            case GrowDirection.LeftToRight: {
                return new Rect(0, 0, (energyBounds.xMax - pos.x) / s.x, 1);
            }
            case GrowDirection.RightToLeft: {
                float x = (energyBounds.xMin - pos.x) / s.x;
                float w = 1 - x;
                return new Rect(x, 0, w, 1);
            }
            case GrowDirection.BottomToTop: {
                float y = (energyBounds.yMin - pos.y) / (s.y);
                float h = 1 - y;
                return new Rect(0, 0, 1, h);
            }
            case GrowDirection.TopToBottom: {
                float y = (energyBounds.yMax - pos.y) / s.y;
                return new Rect(0, 1 - y, 1, y);
            }
                
            default:
                MadDebug.Assert(false, "Unknown grow direction " + growDirection);
                return new Rect(); // won't get here
        }
    }
    
    private Rect toAbsolute(Rect rect) {
        return new Rect(rect.x + screenPositionReal.x, rect.y + screenPositionReal.y, rect.width, rect.height);
    }
    
    
    void GUIDrawEdgeEffect() {
        if (!effectEdge || effectEdgeTexture == null) {
            return;
        }
        
        var edgePosition = EdgePosition();
        
        edgeEffectFunction(edgePosition);
    }
    
    // this is the default function
    void GUIDrawEdgeEffectFun(Vector2 position) {
        float w2 = effectEdgeTexture.width / 2;
        float h2 = effectEdgeTexture.height / 2;
        var rect = new Rect(position.x - w2, position.y - h2, w2 * 2, h2 * 2);

        float visibility;
        if (ValueF2 < effectEdgeIn) {
            visibility = ValueF2 / effectEdgeIn;
        } else if (1 - ValueF2 < effectEdgeOut) {
            visibility = (1 - ValueF2) / effectEdgeOut;
        } else {
            visibility = 1;
        }
        
        var currentMatrix = GUI.matrix;
        GUI.matrix =
            Matrix4x4.TRS(new Vector3(-position.x, -position.y, 0), Quaternion.identity, Vector3.one) * GUI.matrix;
        GUI.matrix =
            Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(ValueF2 * effectEdgeRotateAngle, Vector3.forward), Vector3.one) * GUI.matrix;
        GUI.matrix =
            Matrix4x4.TRS(new Vector3(position.x, position.y, 0), Quaternion.identity, Vector3.one) * GUI.matrix;
        
        Color color = new Color(1, 1, 1, visibility);
        DrawTexture(rect, effectEdgeTexture, color);
        
        GUI.matrix = currentMatrix;
    }
#endregion

    Vector2 EdgePosition() {
        var vb = textureBarVisibleBounds;
    
        switch (growDirection) {
            case GrowDirection.LeftToRight:
                return screenPositionReal + new Vector2((vb.xMin + vb.width * ValueF2) * sizeReal.x, (1 - (vb.yMin + vb.height / 2)) * sizeReal.y);
            default:
                return Vector2.zero; 
        }
    }

    // ===========================================================
    // Static Methods
    // ===========================================================

    // ===========================================================
    // Inner and Anonymous Classes
    // ===========================================================
 
    public enum RotatePoint {
        VisibleAreaCenter,
        TextureCenter,
        CustomPoint
    }
    
    public class BadTextureException : System.Exception {
       public BadTextureException() {
       }
    
       public BadTextureException(string message): base(message) {
       }
    }
    
    public delegate void EdgeEffectFunction(Vector2 edgePosition);
}

} // namespace
