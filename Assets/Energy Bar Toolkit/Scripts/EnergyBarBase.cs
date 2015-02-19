/*
* Energy Bar Toolkit by Mad Pixel Machine
* http://www.madpixelmachine.com
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using EnergyBarToolkit;

#if UNITY_EDITOR
using UnityEditor;
#endif

public abstract class EnergyBarBase : MonoBehaviour {

    // ===========================================================
    // Constants
    // ===========================================================
    
    // ===========================================================
    // Fields
    // ===========================================================
    
    [SerializeField]
    protected int version = 169;  // EBT version number to help updating properties
    
    public Tex[] texturesBackground = new Tex[0];
    public Tex[] texturesForeground = new Tex[0];

    // tells if textures has premultiplied alpha
    public bool premultipliedAlpha = false;
    
    public int guiDepth = 1;
    
    public GameObject anchorObject;
    public Camera anchorCamera; // camera on which anchor should be visible. if null then Camera.main
    
    // Label
    public bool labelEnabled;
    public GUISkin labelSkin;
    public Vector2 labelPosition;
    public bool labelPositionNormalized = true;
    
    public string labelFormat = "{cur}/{max}";
    public Color labelColor = Color.white;
    
    // smooth effect
    public bool effectSmoothChange = false;          // smooth change value display over time
    public float effectSmoothChangeSpeed = 0.5f;    // value bar width percentage per second

    // burn effect
    public bool effectBurn = false;                 // bar draining will display 'burn' effect
    public Texture2D effectBurnTextureBar;
    public string atlasEffectBurnTextureBarGUID = "";
    public Color effectBurnTextureBarColor = Color.red;

    // reference to actual bar component    
    protected EnergyBar energyBar;

    protected float ValueFBurn;
    protected float ValueF2;
    
    // ===========================================================
    // Getters / Setters
    // ===========================================================

    public abstract Rect DrawAreaRect { get; }
    
    protected float ValueF {
        get {
            return energyBar.ValueF;
        }
    }
    
    protected Vector2 LabelPositionPixels {
        get {
            var rect = DrawAreaRect;
            Vector2 v;
            if (labelPositionNormalized) {
                v = new Vector2(rect.x + labelPosition.x * rect.width, rect.y + labelPosition.y * rect.height);
            } else {
                v = new Vector2(rect.x + labelPosition.x, rect.y + labelPosition.y);
            }
            
            return v;
        }
    }

    /// <summary>
    /// Currently displayed value by the renderer (after applying effects)
    /// </summary>
    public float displayValue {
        get {
            return ValueF2;
        }
    }
    
    /// <summary>
    /// Global opacity value.
    /// </summary>
    public float opacity {
        get {
            return _tint.a;
        }
        set {
            _tint.a = Mathf.Clamp01(value);
        }
    }
    
    /// <summary>
    /// Global tint value
    /// </summary>
    public Color tint {
        get {
            return _tint;
        }
        set {
            _tint = value;
        }
    }
    [SerializeField]
    Color _tint = Color.white;

    // ===========================================================
    // Methods for/from SuperClass/Interfaces
    // ===========================================================
    
    // ===========================================================
    // Methods
    // ===========================================================
    
    protected virtual void OnEnable() {
        energyBar = GetComponent<EnergyBar>();
        MadDebug.Assert(energyBar != null, "Cannot access energy bar?!");
        ValueF2 = ValueF;
    }
    
    protected virtual void OnDisable() {
        // do nothing
    }
    
    protected virtual void Start() {
        // do nothing
    }

    protected virtual void Update() {
        UpdateAnimations();
    }

    void UpdateAnimations() {
        UpdateBarValue();
        UpdateBurnValue();
    }

    void UpdateBurnValue() {
        EnergyBarCommons.SmoothDisplayValue(
                       ref ValueFBurn, ValueF2, effectSmoothChangeSpeed);
        ValueFBurn = Mathf.Max(ValueFBurn, ValueF2);
    }

    void UpdateBarValue() {
        if (effectBurn) {
            if (effectSmoothChange) {
                // in burn mode smooth primary bar only when it's increasing
                if (ValueF > ValueF2) {
                    EnergyBarCommons.SmoothDisplayValue(ref ValueF2, ValueF, effectSmoothChangeSpeed);
                } else {
                    ValueF2 = energyBar.ValueF;
                }
            } else {
                ValueF2 = energyBar.ValueF;
            }

        } else {
            if (effectSmoothChange) {
                EnergyBarCommons.SmoothDisplayValue(ref ValueF2, ValueF, effectSmoothChangeSpeed);
            } else {
                ValueF2 = energyBar.ValueF;
            }
        }
    }
    
    protected bool RepaintPhase() {
        return Event.current.type == EventType.Repaint;
    }
    
    
    protected string LabelFormatResolve(string format) {
        format = format.Replace("{cur}", "" + energyBar.valueCurrent);
        format = format.Replace("{min}", "" + energyBar.valueMin);
        format = format.Replace("{max}", "" + energyBar.valueMax);
        format = format.Replace("{cur%}", string.Format("{0:00}", energyBar.ValueF * 100));
        format = format.Replace("{cur2%}", string.Format("{0:00.0}", energyBar.ValueF * 100));
        
        return format;
    }
    
    protected Vector4 ToVector4(Rect r) {
        return new Vector4(r.xMin, r.yMax, r.xMax, r.yMin);
    }
    
    protected Vector2 Round(Vector2 v) {
        return new Vector2(Mathf.Round(v.x), Mathf.Round (v.y));
    }
    
    protected bool IsVisible() {
        if (anchorObject != null) {
            Camera cam;
            if (anchorCamera != null) {
                cam = anchorCamera;
            } else {
                cam = Camera.main;
            }
            
            Vector3 heading = anchorObject.transform.position - cam.transform.position;
            float dot = Vector3.Dot(heading, cam.transform.forward);
            
            return dot >= 0;
        }
        
        if (opacity == 0) {
            return false;
        }
        
        return true;
    }
    
    protected Color PremultiplyAlpha(Color c) {
        return new Color(c.r * c.a, c.g * c.a, c.b * c.a, c.a);
    }
    
    protected virtual Color ComputeColor(Color localColor) {
        Color outColor =
            new Color(
                localColor.r * tint.r,
                localColor.g * tint.g,
                localColor.b * tint.b,
                localColor.a * tint.a);
    
        return outColor;
    }

    // ===========================================================
    // Static Methods
    // ===========================================================

    protected static int HashAdd(int current, bool obj) {
        return MadHashCode.Add(current, obj);
    }

    protected static int HashAdd(int current, int obj) {
        return MadHashCode.Add(current, obj);
    }

    protected static int HashAdd(int current, float obj) {
        return MadHashCode.Add(current, obj);
    }

    protected static int HashAdd(int current, UnityEngine.Object obj) {
        if (obj != null) {
            return MadHashCode.Add(current, obj.GetInstanceID());
        } else {
            return MadHashCode.Add(current, null);
        }
    }

    protected static int HashAdd(int current, object obj) {
        return MadHashCode.Add(current, obj);
    }

    protected static int HashAddTexture(int current, Texture texture) {
#if UNITY_EDITOR
        string path = AssetDatabase.GetAssetPath(texture);
        string guid = AssetDatabase.AssetPathToGUID(path);
        return MadHashCode.Add(current, guid);
#else
        return MadHashCode.Add(current, texture);
#endif
    }

    protected static int HashAddArray(int current, object[] arr) {
        return MadHashCode.AddArray(current, arr);
    }

    protected static int HashAddTextureArray(int current, Texture[] arr, string name = "") {
#if UNITY_EDITOR

        for (int i = 0; i < arr.Length; ++i) {
            string path = AssetDatabase.GetAssetPath(arr[i]);
            string guid = AssetDatabase.AssetPathToGUID(path);
            current =  MadHashCode.Add(current, guid);
        }

        return current;
#else
        return MadHashCode.AddArray(current, arr);
#endif
    }

    protected Rect FindBounds(Texture2D texture) {
        
        int left = -1, top = -1, right = -1, bottom = -1;
        bool expanded = false;
        Color32[] pixels;
        try {
            pixels = texture.GetPixels32();
        } catch (UnityException) { // catch not readable
            return new Rect();
        }
            
        int w = texture.width;
        int h = texture.height;
        int x = 0, y = h - 1;
        for (int i = 0; i < pixels.Length; ++i) {
            var c = pixels[i];
            if (c.a != 0) {
                Expand(x, y, ref left, ref top, ref right, ref bottom);
                expanded = true;
            }
            
            if (++x == w) {
                y--;
                x = 0;
            }
        }
        
        
        MadDebug.Assert(expanded, "bar texture has no visible pixels");
        
        var pixelsRect = new Rect(left, top, right - left + 1, bottom - top + 1);
        var normalizedRect = new Rect(
            pixelsRect.xMin / texture.width,
            1 - pixelsRect.yMax / texture.height,
            pixelsRect.xMax / texture.width - pixelsRect.xMin / texture.width,
            1 - pixelsRect.yMin / texture.height - (1 - pixelsRect.yMax / texture.height));
            
        return normalizedRect;
    }
    
    protected void Expand(int x, int y, ref int left, ref int top, ref int right, ref int bottom) {
        if (left == -1) {
            left = right = x;
            top = bottom = y;
        } else {
            if (left > x) {
                left = x;
            } else if (right < x) {
                right = x;
            }
            
            if (top > y) {
                top = y;
            } else if (bottom == -1 || bottom < y) {
                bottom = y;
            }    
        }
    }
    
    // ===========================================================
    // Static Methods
    // ===========================================================
    
    // ===========================================================
    // Inner and Anonymous Classes
    // ===========================================================
    
    public enum Pivot {
        TopLeft,
        Top,
        TopRight,
        Right,
        BottomRight,
        Bottom,
        BottomLeft,
        Left,
        Center,
    }
    
    [System.Serializable]
    public class Tex : AbstractTex {
        public virtual int width { get { return texture.width; } }
        public virtual int height { get { return texture.height; } }
        
        public virtual bool Valid {
            get {
                return texture != null;
            }
        }
    
        public Texture2D texture;
        
        public override int GetHashCode() {
            int hash = MadHashCode.FirstPrime;
            hash = HashAddTexture(hash, texture);
            //hash = HashAdd(hash, color);
            
            return hash;
        }
    }
    
    public class AbstractTex {
        public Color color = Color.black;
    }
    
    public enum GrowDirection {
        LeftToRight,
        RightToLeft,
        BottomToTop,
        TopToBottom,
        RadialCW,
        RadialCCW,
        ExpandHorizontal,
        ExpandVertical,
        ColorChange,
    }
          
    public enum ColorType {
        Solid,
        Gradient,
    }

    public abstract class TransformFunction {
    }

    [System.Serializable]
    public class TranslateFunction : TransformFunction {
        public Vector2 startPosition;
        public Vector2 endPosition;

        public Vector2 Value(float progress) {
            progress = Mathf.Clamp01(progress);

            var result = Vector2.Lerp(startPosition, endPosition, progress);
            return result;
        }

        public override bool Equals(object obj) {
            if (obj == null) {
                return false;
            }
            
            if (!(obj is TranslateFunction)) {
                return false;
            }

            return GetHashCode() == obj.GetHashCode();
        }

        public override int GetHashCode() {
            int hash = 17;
            hash = hash * 23 + startPosition.GetHashCode();
            hash = hash * 23 + endPosition.GetHashCode();
            return hash;
        }

    }

    [System.Serializable]
    public class ScaleFunction : TransformFunction {
        public Vector2 startScale = Vector3.one;
        public Vector2 endScale = Vector3.one;

        public Vector3 Value(float progress) {
            progress = Mathf.Clamp01(progress);

            var result = Vector2.Lerp(startScale, endScale, progress);
            return new Vector3(result.x, result.y, 1);
        }

        public override bool Equals(object obj) {
            if (obj == null) {
                return false;
            }

            if (!(obj is ScaleFunction)) {
                return false;
            }

            return GetHashCode() == obj.GetHashCode();
        }

        public override int GetHashCode() {
            int hash = 17;
            hash = hash * 23 + startScale.GetHashCode();
            hash = hash * 23 + endScale.GetHashCode();
            return hash;
        }
    }

    [System.Serializable]
    public class RotateFunction : TransformFunction {
        public float startAngle;
        public float endAngle;

        public Quaternion Value(float progress) {
            progress = Mathf.Clamp01(progress);

            float angle = Mathf.Lerp(startAngle, endAngle, progress);

            var result = Quaternion.Euler(0, 0, angle);
            return result;
        }

        public override bool Equals(object obj) {
            if (obj == null) {
                return false;
            }

            if (!(obj is RotateFunction)) {
                return false;
            }

            return GetHashCode() == obj.GetHashCode();
        }

        public override int GetHashCode() {
            int hash = 17;
            hash = hash * 23 + startAngle.GetHashCode();
            hash = hash * 23 + endAngle.GetHashCode();
            return hash;
        }
    }
    
}
