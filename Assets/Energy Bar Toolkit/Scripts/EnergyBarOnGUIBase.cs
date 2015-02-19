/*
* Energy Bar Toolkit by Mad Pixel Machine
* http://www.madpixelmachine.com
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using EnergyBarToolkit;

public abstract class EnergyBarOnGUIBase : EnergyBarBase {

    // ===========================================================
    // Constants
    // ===========================================================
    
    protected const string ShaderStandardTransparentName = "Custom/Energy Bar Toolkit/Unlit";
    protected const string ShaderStandardTransparentPreName = "Custom/Energy Bar Toolkit/Unlit Pre";
    protected const string ShaderHorizontalFillName = "Custom/Energy Bar Toolkit/Horizontal Fill";
    protected const string ShaderHorizontalFillPreName = "Custom/Energy Bar Toolkit/Horizontal Fill Pre";
    protected const string ShaderVerticalFillName = "Custom/Energy Bar Toolkit/Vertical Fill";
    protected const string ShaderVerticalFillPreName = "Custom/Energy Bar Toolkit/Vertical Fill Pre";
    protected const string ShaderExpandFillName = "Custom/Energy Bar Toolkit/Expand Fill";
    protected const string ShaderExpandFillPreName = "Custom/Energy Bar Toolkit/Expand Fill Pre";
    protected const string ShaderRadialFillName = "Custom/Energy Bar Toolkit/Radial Fill";
    protected const string ShaderRadialFillPreName = "Custom/Energy Bar Toolkit/Radial Fill Pre";
    
    protected const string ShaderParamProgress = "_Progress";
    protected const string ShaderParamColor = "_Color";
    protected const string ShaderParamInvert = "_Invert";
    protected const string ShaderParamVisibleRect = "_Rect";

    // ===========================================================
    // Fields
    // ===========================================================
    
    public ResizeMode resizeMode = default(ResizeMode);
    
    public Pivot pivot = Pivot.TopLeft;
    
    public bool positionSizeFromTransform = false;
    public bool positionSizeFromTransformNormalized = false;
    
    // label
    public bool labelOutlineEnabled = true;
    public Color labelOutlineColor = Color.black;
    public Pivot labelPivot = Pivot.TopLeft;
    
    // materials for shaders
    private Material[] materials;
    
    // ===========================================================
    // Properties
    // ===========================================================
    
    /// <summary>
    /// Gets or sets on-screen size in pixels of whole bar (or one part if it repeats)
    /// </summary>
    /// <value>The size pixels.</value>
    public abstract Vector2 SizePixels { get; set; }
    
    /// <summary>
    /// Gets the texture image size in pixels. This is independed of how texture is displayed.
    /// </summary>
    /// <value>The texture size pixels.</value>
    public abstract Vector2 TextureSizePixels { get; }

    // ===========================================================
    // Methods
    // ===========================================================
    
    #region UnityMethods
    
    protected override void OnEnable() {
        base.OnEnable();
        CreateMaterial();
        
        // I want all bars to go through layout phase because I need to set GUI.depth
        // on this phase. If I won't, the ordering will be broken.
        // Also I must remember to put this on the beginning of each OnGUI() method
        // for all derived classes:
        // if (!RepaintPhase()) {
        //     return;
        // }
        useGUILayout = true;
    }
    
    protected override void OnDisable() {
        base.OnDisable();
    }

    protected void OnGUI() {
        GUI.depth = guiDepth;
    }
    
    #endregion
    
    #region Materials
    
    protected Material MaterialStandard() {
        if (premultipliedAlpha) {
            return materials[(int) MaterialType.StandardTransparentPre];
        } else {
            return materials[(int) MaterialType.StandardTransparent];
        }
    }
    
    protected Material MaterialHorizFill() {
        if (premultipliedAlpha) {
            return materials[(int) MaterialType.HorizontalFillPre];
        } else {
            return materials[(int) MaterialType.HorizontalFill];
        }
    }
    
    protected Material MaterialVertFill() {
        if (premultipliedAlpha) {
            return materials[(int) MaterialType.VerticalFillPre];
        } else {
            return materials[(int) MaterialType.VerticalFill];
        }
    }
    
    protected Material MaterialExpandFill() {
        if (premultipliedAlpha) {
            return materials[(int) MaterialType.ExpandFillPre];
        } else {
            return materials[(int) MaterialType.ExpandFill];
        }
    }
    
    protected Material MaterialRadialFill() {
        if (premultipliedAlpha) {
            return materials[(int) MaterialType.RadialFillPre];
        } else {
            return materials[(int) MaterialType.RadialFill];
        }
    }
    
    private void CreateMaterial() {
        int materialsCount = Enum.GetNames(typeof(MaterialType)).Length;
        materials = new Material[materialsCount];
        
        materials[(int) MaterialType.StandardTransparent] = new Material(Shader.Find(ShaderStandardTransparentName));
        materials[(int) MaterialType.StandardTransparentPre] = new Material(Shader.Find(ShaderStandardTransparentPreName));
        materials[(int) MaterialType.HorizontalFill] = new Material(Shader.Find(ShaderHorizontalFillName));
        materials[(int) MaterialType.HorizontalFillPre] = new Material(Shader.Find(ShaderHorizontalFillPreName));
        materials[(int) MaterialType.VerticalFill] = new Material(Shader.Find(ShaderVerticalFillName));
        materials[(int) MaterialType.VerticalFillPre] = new Material(Shader.Find(ShaderVerticalFillPreName));
        materials[(int) MaterialType.ExpandFill] = new Material(Shader.Find(ShaderExpandFillName));
        materials[(int) MaterialType.ExpandFillPre] = new Material(Shader.Find(ShaderExpandFillPreName));
        materials[(int) MaterialType.RadialFill] = new Material(Shader.Find(ShaderRadialFillName));
        materials[(int) MaterialType.RadialFillPre] = new Material(Shader.Find(ShaderRadialFillPreName));
        
        foreach (var mat in materials) {
            mat.hideFlags = HideFlags.DontSave;
        }
    }
    
    private void DestroyMaterial() {
        foreach (var mat in materials) {
            if (Application.isPlaying) {
                Destroy(mat);
            } else {
                DestroyImmediate(mat);
            }
        }
        
        materials = null;
    }
    
    #endregion
    
    #region Shaders
    protected void ShaderSetProgress(float progress, Material mat) {
        mat.SetFloat(ShaderParamProgress, progress);
    }
    
    protected void ShaderSetInvert(bool invert, Material mat) {
        mat.SetFloat(ShaderParamInvert, invert ? 1 : 0);
    }
    
    protected void ShaderSetColor(Color color, Material mat) {
        mat.SetColor(ShaderParamColor, color);
    }
    
    protected void ShaderSetVisibleRect(Rect visibleRect, Material mat) {
        mat.SetVector(ShaderParamVisibleRect, ToVector4(visibleRect));
    }
    #endregion
    
    #region DrawTexture
    
    protected void GUIDrawBackground() {
        if (texturesBackground != null) {
            var rect = DrawAreaRect;
            
            foreach (var bg in texturesBackground) {
                var t = bg.texture;
                if (t != null) {
                    DrawTexture(rect, t, bg.color);
                }
            }
        }
    }
    
    protected void GUIDrawForeground() {
        if (texturesForeground != null) {
            var rect = DrawAreaRect;
            
            foreach (var fg in texturesForeground) {
                var t = fg.texture;
                if (t != null) {
                    DrawTexture(rect, t, fg.color);
                }
            }
        }
    }
    
    protected void DrawTexture(Rect rect, Texture2D texture, Color tint) {
        var mat = MaterialStandard();
        mat.SetColor("_Color", ComputeColor(tint));
        Graphics.DrawTexture(rect, texture, mat);
    }
    
    protected void DrawTexture(Rect rect, Texture2D texture, Rect coords, Color tint) {
        var mat = MaterialStandard();
        mat.SetColor("_Color", ComputeColor(tint));
        Graphics.DrawTexture(rect, texture, coords, 0, 0, 0, 0, mat);
    }
    
    protected void DrawTextureHorizFill(
        Rect rect, Texture2D texture, Rect visibleRect, Color color, bool invert, float progress) {
        
        var mat = MaterialHorizFill();
        mat.SetColor("_Color", ComputeColor(color));
        mat.SetFloat("_Invert", invert ? 1 : 0);
        mat.SetFloat("_Progress", progress);
        mat.SetVector("_Rect", ToVector4(visibleRect));
        
        Graphics.DrawTexture(rect, texture, mat);
    }
    
    protected void DrawTextureVertFill(
        Rect rect, Texture2D texture, Rect visibleRect, Color color, bool invert, float progress) {
        
        var mat = MaterialVertFill();
        mat.SetColor("_Color", ComputeColor(color));
        mat.SetFloat("_Invert", invert ? 1 : 0);
        mat.SetFloat("_Progress", progress);
        mat.SetVector("_Rect", ToVector4(visibleRect));
        
        Graphics.DrawTexture(rect, texture, mat);
    }
    
    protected void DrawTextureExpandFill(
        Rect rect, Texture2D texture, Rect visibleRect, Color color, bool invert, float progress) {
        
        var mat = MaterialExpandFill();
        mat.SetColor("_Color", ComputeColor(color));
        mat.SetFloat("_Invert", invert ? 1 : 0);
        mat.SetFloat("_Progress", progress);
        mat.SetVector("_Rect", ToVector4(visibleRect));
        
        Graphics.DrawTexture(rect, texture, mat);
    }
    
    protected void DrawTextureRadialFill(
        Rect rect, Texture2D texture, Color color, bool invert, float progress, float offset, float length) {
        
        var mat = MaterialRadialFill();
        mat.SetColor("_Color", ComputeColor(color));
        mat.SetFloat("_Invert", invert ? 1 : 0);
        mat.SetFloat("_Progress", progress);
        mat.SetFloat("_Offset", offset);
        mat.SetFloat("_Length", length);
        
        Graphics.DrawTexture(rect, texture, mat);
    }
    
    protected void GUIDrawLabel() {
        if (!labelEnabled) {
            return;
        }
        
        var skin = labelSkin;
        if (skin == null) {
            skin = GUI.skin;
        }
        
        float outlineSize = 1; // only size "1" looks good
        
        var text = LabelFormatResolve(labelFormat);
        var size = skin.label.CalcSize(new GUIContent(text));
        if (labelOutlineEnabled) {
            float outlineSize2 = outlineSize * 2;
            size.x += outlineSize2;
            size.y += outlineSize2;
        }
        
        var pos = LabelPositionPixels;
        Rect rect = new Rect(pos.x, pos.y, size.x, size.y);
        rect = ApplyLabelPivot(rect);
        
        GUI.color = labelColor;
        if (labelOutlineEnabled) {
            LabelWithOutline(rect, text, skin.label, labelOutlineColor, outlineSize);
        } else {
            GUI.Label(rect, text, skin.label);
        }
        GUI.color = Color.white;
    }
    
    Rect ApplyLabelPivot(Rect r) {
        switch (labelPivot) {
            case Pivot.TopLeft:
                return r; // no change
            case Pivot.Top:
                return new Rect(r.x - r.width / 2, r.y, r.width, r.height);
            case Pivot.TopRight:
                return new Rect(r.x - r.width, r.y, r.width, r.height);
            case Pivot.Right:
                return new Rect(r.x - r.width, r.y - r.height / 2, r.width, r.height);
            case Pivot.BottomRight:
                return new Rect(r.x - r.width, r.y - r.height, r.width, r.height);
            case Pivot.Bottom:
                return new Rect(r.x - r.width / 2, r.y - r.height, r.width, r.height);
            case Pivot.BottomLeft:
                return new Rect(r.x, r.y - r.height, r.width, r.height);
            case Pivot.Left:
                return new Rect(r.x, r.y - r.height / 2, r.width, r.height);
            case Pivot.Center:
                return new Rect(r.x - r.width / 2, r.y - r.height / 2, r.width, r.height);
            default:
                Debug.LogError("Unknown pivot: " + labelPivot);
                return r;
        }
    }
    
    void LabelWithOutline(Rect rect, string text, GUIStyle style, Color color, float size) {
        Color prevColor = GUI.color;
        GUI.color = color;
        
        foreach (Vector2 trans in new Vector2[] { new Vector2(1, 0), new Vector2(-1, 0), new Vector2(0, 1), new Vector2(0, -1)} ) {
            var nRect = new Rect(rect.x + trans.x * size, rect.y + trans.y * size, rect.width, rect.height);
            GUI.Label(nRect, text, style);
        }
        
        GUI.color = prevColor;
        GUI.Label(rect, text, style);
    }
    
    #endregion
    
    #region Transforms
    protected Vector2 TransformScale() {
        if (positionSizeFromTransform) {
            var s = transform.lossyScale;
            return new Vector2(s.x, s.y);
        } else {
            return Vector2.one;
        }
    }
    
    protected Vector2 RealPosition(Vector2 pos, Vector2 bounds) {
        Vector2 transformAnchor = Vector2.zero;
        if (positionSizeFromTransform) {
            if (positionSizeFromTransformNormalized) {
                transformAnchor = new Vector2(
                    transform.position.x * Screen2.width, -transform.position.y * Screen2.height);
            } else {
                transformAnchor = new Vector2(transform.position.x, -transform.position.y);
            }
            
            pos.Scale(transform.lossyScale);
        }
        
        Vector2 screenAnchor = Vector2.zero;
        Vector2 boundsAnchor = Vector2.zero;
        
        switch (pivot) {
        case Pivot.TopLeft:
            screenAnchor = Vector2.zero;
            boundsAnchor = Vector2.zero;
            break;
        case Pivot.Top:
            screenAnchor = new Vector2(Screen2.width / 2, 0);
            boundsAnchor = new Vector2(bounds.x / 2, 0);
            break;
        case Pivot.TopRight:
            screenAnchor = new Vector2(Screen2.width, 0);
            boundsAnchor = new Vector2(bounds.x, 0);
            break;
        case Pivot.Right:
            screenAnchor = new Vector2(Screen2.width, Screen2.height / 2);
            boundsAnchor = new Vector2(bounds.x, bounds.y / 2);
            break;
        case Pivot.BottomRight:
            screenAnchor = new Vector2(Screen2.width, Screen2.height);
            boundsAnchor = new Vector2(bounds.x, bounds.y);
            break;
        case Pivot.Bottom:
            screenAnchor = new Vector2(Screen2.width / 2, Screen2.height);
            boundsAnchor = new Vector2(bounds.x / 2, bounds.y);
            break;
        case Pivot.BottomLeft:
            screenAnchor = new Vector2(0, Screen2.height);
            boundsAnchor = new Vector2(0, bounds.y);
            break;
        case Pivot.Left:
            screenAnchor = new Vector2(0, Screen2.height / 2);
            boundsAnchor = new Vector2(0, bounds.y / 2);
            break;
        case Pivot.Center:
            screenAnchor = new Vector2(Screen2.width / 2, Screen2.height / 2);
            boundsAnchor = new Vector2(bounds.x / 2, bounds.y / 2);
            break;
        }
        
        if (anchorObject != null) {
            Camera cam;
            if (anchorCamera != null) {
                cam = anchorCamera;
            } else {
                cam = Camera.main;
            }
            
            screenAnchor = cam.WorldToScreenPoint(anchorObject.transform.position);
            screenAnchor.y = Screen2.height - screenAnchor.y;
        }
        
        var o = transformAnchor + screenAnchor - boundsAnchor + pos;
        return o;
    }
    #endregion
    
    #region Helpers
    
    override protected Color ComputeColor(Color localColor) {
        var outColor = base.ComputeColor(localColor);
        
        if (premultipliedAlpha) {
            outColor = PremultiplyAlpha(outColor);
        }
        
        return outColor;
    }
    
    protected Vector2 GetSizePixels(Vector2 manualSize) {
        Vector2 o;
        switch (resizeMode) {
            case ResizeMode.None:
                o = TextureSizePixels;
                break;
            case ResizeMode.Fixed:
                o = manualSize;
                break;
            case ResizeMode.Stretch:
                o = new Vector2(manualSize.x * Screen2.width, manualSize.y * Screen2.height);
                break;
            case ResizeMode.KeepRatio:
                var texSize = TextureSizePixels;
                
                float height = manualSize.y * Screen2.height;
                float width = (texSize.x / texSize.y) * height;
                o = new Vector2(width, height);
                break;
            default:
                Debug.LogError("Unknown resize mode: " + resizeMode);
                o = manualSize;
                break;
        }
        
        o.Scale(TransformScale());
        return o;
    }
    
    protected void SetSizePixels(ref Vector2 manualSize, Vector2 value) {
        switch (resizeMode) {
            case ResizeMode.None:
                // do nothing
                break;
            case ResizeMode.Fixed:
                manualSize = value;
                break;
            case ResizeMode.Stretch:
                manualSize = new Vector2(value.x / Screen2.width, value.y / Screen2.height);
                break;
            case ResizeMode.KeepRatio:
                var s = TextureSizePixels;
                float height = value.y / Screen2.height;
                float width = (s.x / s.y) * height;
                manualSize = new Vector2(width, height);
                break;
            default:
                Debug.LogError("Unknown resize mode: " + resizeMode);
                manualSize = value;
                break;
        }
    }
    
    public void ResetSize() {
        SizePixels = TextureSizePixels;
    }
    
    #endregion
    

    // ===========================================================
    // Static Methods
    // ===========================================================

    // ===========================================================
    // Inner and Anonymous Classes
    // ===========================================================
    
    protected enum MaterialType {
        StandardTransparent     = 0,
        StandardTransparentPre  = 1,
        HorizontalFill          = 2,
        HorizontalFillPre       = 3,
        VerticalFill            = 4,
        VerticalFillPre         = 5,
        ExpandFill              = 6,
        ExpandFillPre           = 7,
        RadialFill              = 8,
        RadialFillPre           = 9,
    }
    
    public enum ResizeMode {
        None,
        Fixed,
        Stretch,
        KeepRatio,
    }

}