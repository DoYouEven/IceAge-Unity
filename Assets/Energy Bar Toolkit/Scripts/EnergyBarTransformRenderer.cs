/*
* Energy Bar Toolkit by Mad Pixel Machine
* http://www.madpixelmachine.com
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace EnergyBarToolkit {

/// <summary>
/// Energy bar renderer that indicates progress change by transforming
/// (translating, resizing, rotating) its texture.
/// </summary>
[ExecuteInEditMode]
[RequireComponent(typeof(EnergyBar))]
public class EnergyBarTransformRenderer : EnergyBarOnGUIBase {

    // ===========================================================
    // Constants
    // ===========================================================

    // ===========================================================
    // Fields
    // ===========================================================
    
    public Vector2 screenPosition = new Vector2(10, 10);
    public bool screenPositionNormalized; // indicates that all coordinates are normalized to screen width and height
    
    public Vector2 size = new Vector2(100, 20);
    
    private Vector2 sizeReal;
    private Vector2 sizeOrig; // keeps the original size of this energy bar
    private Vector2 screenPositionReal;
    
    public Tex textureObject;
    
    public bool transformTranslate;
    public bool transformRotate;
    public bool transformScale;
    public Vector2 transformAnchor = new Vector2(0.5f, 0.5f);
    
    public TranslateFunction translateFunction;
    public RotateFunction rotateFunction;
    public ScaleFunction scaleFunction;
    
    // deprecated
    [SerializeField] private bool sizeNormalized;
    [SerializeField] private bool screenPositionCalculateSize = true;
    
    // ===========================================================
    // Fields
    // ===========================================================
    
    public override Rect DrawAreaRect {
        get {
            var rect = new Rect(screenPositionReal.x, screenPositionReal.y, sizeReal.x, sizeReal.y);
            return rect;
        }
    }
    
    private Vector2 ScreenPositionPixels {
        get {
            if (screenPositionNormalized) {
                return new Vector2(screenPosition.x * Screen.width, screenPosition.y * Screen.height);
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

    // ===========================================================
    // Methods for/from SuperClass/Interfaces
    // ===========================================================
    
    public override Vector2 TextureSizePixels {
        get {
            if (texturesBackground.Length > 0 && texturesBackground[0] != null && texturesBackground[0].Valid) {
                var tex = texturesBackground[0];
                return new Vector2(tex.width, tex.height);
            } else if (texturesForeground.Length > 0 && texturesForeground[0] != null && texturesForeground[0].Valid) {
                var tex = texturesForeground[0];
                return new Vector2(tex.width, tex.height);
            } else if (textureObject != null && textureObject.Valid) {
                return new Vector2(textureObject.width, textureObject.height);
            } else {
                return Vector2.one;
            }
        }
    }

    // ===========================================================
    // Methods
    // ===========================================================

    new void Start() {
        base.Start();
        
        if (version == 169) {
            Upgrade_169_171();
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

    protected override void Update() {
        base.Update();

        var anyTexture = AnyBackgroundOrForegroundTexture();
        if (anyTexture != null) {
            sizeOrig = new Vector2(anyTexture.width, anyTexture.height);
        } else if (textureObject.Valid) {
            sizeOrig = new Vector2(textureObject.texture.width, textureObject.texture.height);
        }
        
        UpdateSize();
    }
    
    // updates sizing of textures. Need to be called in Update and OnGUI methods
    // in other way it will lead to bad scaling effect in Editor preview
    void UpdateSize() {
        sizeReal = Round(SizePixels);
        screenPositionReal = RealPosition(Round(ScreenPositionPixels), SizePixels);
    }
    
    Texture2D AnyBackgroundOrForegroundTexture() {
        return AnyBackgroundTexture() ?? AnyForegroundTexture();
    }
    
    Texture2D AnyBackgroundTexture() {
        if (texturesBackground != null) {
            foreach (var tex in texturesBackground) {
                if (tex.texture != null) {
                    return tex.texture;
                }
            }
        }
        
        return null;
    }
    
    Texture2D AnyForegroundTexture() {
        if (texturesForeground != null) {
            foreach (var tex in texturesForeground) {
                if (tex.texture != null) {
                    return tex.texture;
                }
            }
        }
        
        return null;
    }
    
    new public void OnGUI() {
        base.OnGUI();
        
        if (!RepaintPhase()) {
            return;
        }
        
        if (!IsVisible()) {
            return;
        }
    
        UpdateSize();
        
        
        GUIDrawBackground();
        
        if (textureObject.Valid) {
            DrawObject();
        }
        
        GUIDrawForeground();
        GUIDrawLabel();
    }
    
    void DrawObject() {
        Vector3 translate = Vector3.zero;
        if (transformTranslate) {
            translate = translateFunction.Value(ValueF2);
            translate = new Vector3(translate.x * sizeReal.x, translate.y * sizeReal.y, 0);
        }
        
        Quaternion rotation = Quaternion.identity;
        if (transformRotate) {
            rotation = rotateFunction.Value(ValueF2);
        }
        
        Vector3 scale = Vector3.one;
        if (transformScale) {
            scale = scaleFunction.Value(ValueF2);
        }
        
        float tx = textureObject.width * transformAnchor.x;
        float ty = textureObject.height * transformAnchor.y;
        


        var matrix = Matrix4x4.identity;
        
        MadMatrix.Translate(ref matrix, -tx, -ty, 0);
        MadMatrix.Scale(ref matrix, scale);
        MadMatrix.Rotate(ref matrix, rotation);
        
        MadMatrix.Scale(ref matrix, sizeReal.x / sizeOrig.x, sizeReal.y / sizeOrig.y, 1);
//        if (positionSizeFromTransform) {
//            MadMatrix.Scale(ref matrix, transform.lossyScale);
//        }
        
        MadMatrix.Translate(ref matrix, screenPositionReal);
        MadMatrix.Translate(ref matrix, sizeReal.x / 2, sizeReal.y / 2, 0);
        MadMatrix.Translate(ref matrix, translate);
        
        GUI.matrix = matrix;
            
        DrawTexture(
            new Rect(
                0, 0,
                textureObject.width, textureObject.height),
            textureObject.texture,
            textureObject.color);
            
        GUI.matrix = Matrix4x4.identity;
    }
    
    // ===========================================================
    // Static Methods
    // ===========================================================

    // ===========================================================
    // Inner and Anonymous Classes
    // ===========================================================
    
    public enum TransformType {
        Translate,
        Scale,
        Rotate,
    }
    
}

} // namespace
