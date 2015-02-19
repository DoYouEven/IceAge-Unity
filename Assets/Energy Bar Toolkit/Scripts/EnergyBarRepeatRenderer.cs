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
public class EnergyBarRepeatRenderer : EnergyBarOnGUIBase {

    // ===========================================================
    // Constants
    // ===========================================================
    
    readonly static Vector2 DefaultPositionDelta = new Vector2(1.2f, 0);

    // ===========================================================
    // Fields
    // ===========================================================
    
    public Texture2D icon;
    public Color iconColor = Color.white;
    public Texture2D iconSlot;
    public Color iconSlotColor = Color.white;
    
    public Vector2 iconSize;
    public Vector2 startPosition = new Vector2(10, 10);   // position of first icon
    public bool startPositionNormalized;
    public int repeatCount = 5;         // how many times icon will be repeated
    public Vector2 positionDelta = DefaultPositionDelta;   // distance between each next icon
    public Effect effect = Effect.GrowIn;
    public CutDirection cutDirection = CutDirection.BottomToTop;
    
    // deprecated
    [SerializeField] private bool iconSizeNormalized;
    [SerializeField] private bool positionDeltaNormalized;
    [SerializeField] private bool iconSizeCalculate = true;

    // ===========================================================
    // Constructors (Including Static Constructors)
    // ===========================================================

    // ===========================================================
    // Getters / Setters
    // ===========================================================
    
    public override Vector2 SizePixels {
        get {
            return GetSizePixels(iconSize);
        }
        
        set {
            SetSizePixels(ref iconSize, value);
        }
    }
    
    private Vector2 StartPositionPixels {
        get {
            if (startPositionNormalized) {
                return new Vector2(startPosition.x * Screen.width, startPosition.y * Screen.height);
            } else {
                return startPosition;
            }
        }
    }
    
    private Vector2 PositionDeltaPixels {
        get {
            var iconSize = SizePixels;
            return new Vector2(positionDelta.x * iconSize.x, positionDelta.y * iconSize.y);
        }
    }
    
    // ===========================================================
    // Methods for/from SuperClass/Interfaces
    // ===========================================================
    
    public override Vector2 TextureSizePixels {
        get {
            if (icon == null) {
                return Vector2.one;
            }
            
            return new Vector2(icon.width, icon.height);
        }
    }
    
    public override Rect DrawAreaRect {
        get {
            var startPosition = StartPositionPixels;
            var positionDelta = PositionDeltaPixels;
            return new Rect(
                startPosition.x,
                startPosition.y,
                positionDelta.x * repeatCount + iconSize.x,
                positionDelta.y * repeatCount + iconSize.y
            );
        }
    }

    // ===========================================================
    // Methods
    // ===========================================================
    
    new void Start() {
        base.Start();
        
        if (version == 169) {
            Upgrade_169_170();
        }
    }

#region UpgradeProcedures
    void Upgrade_169_170() {
        // I cannot be sure if this component was created on 169 or 170
        // What I am doing is safe-upgrade
        if (iconSizeNormalized) {
            resizeMode = EnergyBarOnGUIBase.ResizeMode.Stretch;
        } else if (!iconSizeCalculate) {
            resizeMode = EnergyBarOnGUIBase.ResizeMode.Fixed;
        }
        
        if (positionDelta != DefaultPositionDelta) {
            if (positionDeltaNormalized) {
                positionDelta = new Vector2(positionDelta.x * Screen.width, positionDelta.y * Screen.height);
            }
            positionDelta = new Vector2(positionDelta.x / icon.width, positionDelta.y / icon.height);
        }
        
        version = 170;
    }
#endregion
    
    protected override void Update() {
        base.Update();
        FixValues();
    }
    
    private void FixValues() {
        repeatCount = Mathf.Clamp(repeatCount, 1, 1000);
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
        
        var pos = RealPosition(Round(StartPositionPixels), SizePixels);
        var iSize = SizePixels;
        var delta = PositionDeltaPixels;
        
        float iconCount = ValueF2 * repeatCount;
        int baseIconCount = (int) Mathf.Floor(iconCount);     // icons painted at full visibility
        float lastIconValue = iconCount - baseIconCount;
        
        for (int i = 0; i < repeatCount; ++i) {
            // draw slots if set
            if (iconSlot != null) {
                DrawTexture(new Rect(pos.x, pos.y, iSize.x, iSize.y), iconSlot, iconSlotColor);
            }
            
            if (i < baseIconCount) { // draw solid icons
                DrawTexture(new Rect(pos.x, pos.y, iSize.x, iSize.y), icon, iconColor);
            } else if (i == baseIconCount) { // draw growing icon
                var iconRect =  new Rect(pos.x, pos.y, iSize.x, iSize.y);
        
                switch (effect) {
                    case Effect.FadeOut:
                        var tint = new Color(iconColor.r, iconColor.g, iconColor.b, iconColor.a * lastIconValue);
                        DrawTexture(new Rect(pos.x, pos.y, iSize.x, iSize.y), icon, tint);
                        break;
                        
                    case Effect.GrowIn:
                        var rect = Resize(iconRect, lastIconValue);
                        DrawTexture(rect, icon, iconColor);
                        break;
                        
                    case Effect.Cut:
                        switch (cutDirection) {
                            case CutDirection.BottomToTop:
                                DrawTextureVertFill(iconRect, icon, new Rect(0, 0, 1, 1), iconColor, false, lastIconValue);
                                break;
                            case CutDirection.TopToBottom:
                                DrawTextureVertFill(iconRect, icon, new Rect(0, 0, 1, 1), iconColor, true, lastIconValue);
                                break;
                            case CutDirection.LeftToRight:
                                DrawTextureHorizFill(iconRect, icon, new Rect(0, 0, 1, 1), iconColor, false, lastIconValue);
                                break;
                            case CutDirection.RightToLeft:
                                DrawTextureHorizFill(iconRect, icon, new Rect(0, 0, 1, 1), iconColor, true, lastIconValue);
                                break;
                            default:
                                Debug.LogError("Unknown cut direction: " + cutDirection);
                                break;
                        }
        
                        break;
                        
                }
            }
            
            pos += Round(delta);
        }
        
        GUIDrawLabel();
    }
    
    private bool IsValid() {
        return icon != null;
    }
    
    private Rect Resize(Rect rect, float factor) {
        var w = rect.width;
        var h = rect.height;
        var c = rect.center;
        
        return new Rect(c.x - w / 2 * factor, c.y - h / 2 * factor, w * factor, h * factor);
    }

    // ===========================================================
    // Static Methods
    // ===========================================================

    // ===========================================================
    // Inner and Anonymous Classes
    // ===========================================================
    
    public enum Effect {
        None,
        GrowIn,
        FadeOut,
        Cut
    }
    
    public enum CutDirection {
        LeftToRight,
        TopToBottom,
        RightToLeft,
        BottomToTop
    }

}

} // namespace
