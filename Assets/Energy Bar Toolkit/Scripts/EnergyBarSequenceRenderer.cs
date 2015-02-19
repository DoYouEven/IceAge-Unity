/*
* Energy Bar Toolkit by Mad Pixel Machine
* http://www.madpixelmachine.com
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace EnergyBarToolkit {

[ExecuteInEditMode]
[RequireComponent(typeof(EnergyBar))]
public class EnergyBarSequenceRenderer : EnergyBarOnGUIBase {

    // ===========================================================
    // Constants
    // ===========================================================

    // ===========================================================
    // Fields
    // ===========================================================
    
    public enum Method {
        Grid,
        Sequence
    }
    
    public Vector2 position = new Vector2();
    public bool positionNormalized;
    
    public Vector2 size;
    
    public Color color = Color.white;
    
    public Method method = Method.Grid;
    
    //
    // Grid method fields
    //
    public Texture2D gridTexture;
    public int gridWidth = 2;
    public int gridHeight = 2;
    
    public bool frameCountManual = false;
    public int frameCount = 4;
    
    //
    // Sequence method fields
    //
    public Texture2D[] sequence;
    
    
    // deprecated
    [SerializeField] private bool sizeNormalized;
    [SerializeField] private bool sizeCalculate = true;
    
    // ===========================================================
    // Constructors (Including Static Constructors)
    // ===========================================================

    // ===========================================================
    // Getters / Setters
    // ===========================================================
    
    private Vector2 PositionPixels {
        get {
            if (positionNormalized) {
                return new Vector2(position.x * Screen.width, position.y * Screen.height);
            } else {
                return position;
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
            switch (method) {
            case Method.Grid:
                if (gridTexture == null) {
                    return Vector2.one;
                }
                
                return new Vector2(gridTexture.width / gridWidth, gridTexture.height / gridHeight);
            case Method.Sequence:
                if (sequence == null || sequence.Length == 0 || sequence[0] == null) {
                    return Vector2.one;
                }
                
                var t = sequence[0];
                return new Vector2(t.width, t.height);
                
            default:
                MadDebug.Assert(false, "unknown method: " + method);
                return Vector2.one;
            }
        }
    }
    

    // ===========================================================
    // Methods for/from SuperClass/Interfaces
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
        } else if (!sizeCalculate) {
            resizeMode = EnergyBarOnGUIBase.ResizeMode.Fixed;
        }
        
        version = 171;
    }
    #endregion

    protected override void Update() {
        base.Update();
        FixValues();
        
        if (!frameCountManual) {
            frameCount = gridWidth * gridHeight;
        }
    }
    
    new void OnGUI() {
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
        
        Rect texCoords;
        var texture = GetTexture(out texCoords);
        
        if (texture != null) {
            GUIDrawBackground();
        
            var s = Round(SizePixels);
            var pos = RealPosition(Round(PositionPixels), s);
            DrawTexture(new Rect(pos.x, pos.y, s.x, s.y), texture, texCoords, color);
            
            GUIDrawForeground();
        }
        
        GUIDrawLabel();
    }
    
    public override Rect DrawAreaRect {
        get {
            var sizeReal = Round(SizePixels);
            var screenPositionReal = RealPosition(Round(PositionPixels), SizePixels);
            
            var rect = new Rect(screenPositionReal.x, screenPositionReal.y, sizeReal.x, sizeReal.y);
            return rect;
        }
    }

    // ===========================================================
    // Methods
    // ===========================================================
    
    private void FixValues() {
        gridWidth = Mathf.Max(1, gridWidth);
        gridHeight = Mathf.Max(1, gridHeight);
        frameCount = Mathf.Max(1, frameCount);
    }
    
    public bool IsValid() {
        if (energyBar == null) {
            return false;
        }
    
        switch (method) {
            case Method.Grid:
                return gridWidth > 0 && gridHeight > 0 && gridTexture != null;
            case Method.Sequence:
                return sequence.Length > 0 && sequence[0] != null;
            default:
                MadDebug.Assert(false, "unknown method: " + method);
                return false; // won't get here
        }
    }
    
    public Texture2D GetTexture(out Rect texCoords) {
        switch (method) {
            case Method.Grid:
                return GetTextureGrid(out texCoords);
            case Method.Sequence:
                return GetTextureSequence(out texCoords);
            default:
                MadDebug.Assert(false, "unknown method: " + method);
                texCoords = new Rect();
                return null; // won't get here
        }
    }
    
    private Texture2D GetTextureGrid(out Rect texCoords) {
        int size = frameCount;
        int index = Index(size);
        float y = (gridHeight - 1 - index / gridWidth) / (float) gridHeight;
        float x = index % gridWidth / (float) gridWidth;
        float w = 1.0f / gridWidth;
        float h = 1.0f / gridHeight;
        
        texCoords = new Rect(x, y, w, h);
        return gridTexture;
    }
    
    private Texture2D GetTextureSequence(out Rect texCoords) {
        int size = sequence.Length;
        int index = Index(size);
        
        texCoords = new Rect(0, 0, 1, 1);
        return sequence[index];
    }
    
    private int Index(int size) {
        var valueF = ValueF2;
        int index = (int) Mathf.Min(Mathf.Floor(valueF * size), size - 1);
        return index;
    }

    // ===========================================================
    // Static Methods
    // ===========================================================

    // ===========================================================
    // Inner and Anonymous Classes
    // ===========================================================

}

} // namespace
