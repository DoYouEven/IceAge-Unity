/*
* Energy Bar Toolkit by Mad Pixel Machine
* http://www.madpixelmachine.com
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace EnergyBarToolkit {
 
[ExecuteInEditMode]
[RequireComponent(typeof(EnergyBar))]
public class FilledRenderer3D : EnergyBar3DBase {

    // ===========================================================
    // Constants
    // ===========================================================
    
    // ===========================================================
    // Fields
    // ===========================================================
    
    
    //
    // textures
    //
    
    // non-atlas texture bar
    public Texture2D textureBar;
    
    // atlas texture bar
    public string atlasTextureBarGUID = "";
    
    //
    // appearance
    //
    public ColorType textureBarColorType;
    public Color textureBarColor = Color.white;
    public Gradient textureBarGradient;

    public GrowDirection growDirection = GrowDirection.LeftToRight;
    
    public float radialOffset;
    public float radialLength = 1;
    
    //
    // effect
    //
    
    // blink effect
    public bool effectBlink = false;
    public float effectBlinkValue = 0.2f;
    public float effectBlinkRatePerSecond = 1f;
    public Color effectBlinkColor = new Color(1, 1, 1, 0);
    
    // sprite follow effect
    public bool effectFollow = false;
    public UnityEngine.Object effectFollowObject; // MadSprite, Texture2D or 
    public AnimationCurve effectFollowScaleX;
    public AnimationCurve effectFollowScaleY;
    public AnimationCurve effectFollowScaleZ;
    public AnimationCurve effectFollowRotation;
    public Gradient effectFollowColor;
    
    [SerializeField] private bool effectFollowDefaultsSet = false;
    // when texture is set, the sprite is created
    private MadSprite effectFollowSprite;
    
    
    //
    // others
    //
    
#region Fields others
    private int lastRebuildHash;
    private bool dirty = true;
    
    // sprite references
    
    private MadSprite spriteBar;
    
    private MadSprite spriteBurnBar;
#endregion
    
    // ===========================================================
    // Properties
    // ===========================================================
    
    public override Pivot pivot {
        get {
            return base.pivot;
        }
        set {
            bool update = base.pivot != value;
            base.pivot = value;
            if (update) {
                UpdatePivot();
            }
        }
    }

    bool Blink {
        get; set;
    }
    
    // return current bar color based on color settings and effect
    float _effectBlinkAccum;
    Color BarColor {
        get {
            Color outColor = Color.white;
            
            if (growDirection == EnergyBarBase.GrowDirection.ColorChange) {
                outColor = textureBarGradient.Evaluate(energyBar.ValueF);
            } else {
                switch (textureBarColorType) {
                    case ColorType.Solid:
                        outColor = textureBarColor;
                        break;
                    case ColorType.Gradient:
                        outColor = textureBarGradient.Evaluate(energyBar.ValueF);
                        break;
                    default:
                        MadDebug.Assert(false, "Unkwnown option: " + textureBarColorType);
                        break;
                }
            }
            
            if (Blink) {
                outColor = effectBlinkColor;
            }
            
            return ComputeColor(outColor);
        }
    }
    
    Color BurnColor {
        get {
            Color outColor = effectBurnTextureBarColor;
            if (Blink) {
                outColor = new Color(0, 0, 0, 0);
            }
            
            return ComputeColor(outColor);
        }
    }
    
    // ===========================================================
    // Methods for/from SuperClass/Interfaces
    // ===========================================================
    
    public override Rect DrawAreaRect {
        get {
            if (spriteBar != null && spriteBar.CanDraw()) {
                return spriteBar.GetBounds();
            } else if (textureBar != null) {
                // if there's no sprite set, but texture bar is set then this means that rebuild
                // is not done yet. Trying to calculate bounds manually.
                Vector2 offset = PivotOffset(pivot);
                float w = textureBar.width;
                float h = textureBar.height;
                return new Rect(offset.x * w, (1 - offset.y) * h, w, h);
            } else {
                return new Rect();
            }
        }
    }

    // ===========================================================
    // Methods
    // ===========================================================
    
    protected override void OnEnable() {
        base.OnEnable();
        if (!effectFollowDefaultsSet) {
            effectFollowScaleX = AnimationCurve.Linear(0, 1, 1, 1);
            effectFollowScaleY = AnimationCurve.Linear(0, 1, 1, 1);
            effectFollowScaleZ = AnimationCurve.Linear(0, 1, 1, 1);
            effectFollowRotation = AnimationCurve.Linear(0, 0, 1, 0);
            effectFollowDefaultsSet = true;
        }
    }

    protected override void Update() {
        if (RebuildNeeded()) {
            Rebuild();
        }

        base.Update();

        if (effectBlink) {
            Blink = EnergyBarCommons.Blink(
                ValueF, effectBlinkValue, effectBlinkRatePerSecond, ref _effectBlinkAccum);
        } else {
            Blink = false;
        }

        UpdateBar();
        UpdatePivot();
        UpdateFollowEffect();
    }

    //protected override void UpdateTransforms() {
    //    base.UpdateTransforms();

    //    ApplyTransform(spriteBar);
    //    ApplyTransform(spriteBurnBar);
    //    ApplyTransform(effectFollowSprite);
    //}

    void UpdateBar() {
        bool visible = IsVisible();
    
        if (effectBurn && spriteBurnBar != null) {
            spriteBurnBar.visible = visible;
            spriteBurnBar.tint = BurnColor;
            spriteBurnBar.fillValue = ValueFBurn;
        }
        
        if (spriteBar != null) {
            spriteBar.visible = visible;
            spriteBar.tint = BarColor;
            spriteBar.fillValue = ValueF2;
        }
    }

    void UpdatePivot() {
        var pivot = Translate(this.pivot);
        if (spriteBar != null) {
            spriteBar.pivotPoint = pivot;
        }
        
        if (spriteBurnBar != null) {
            spriteBurnBar.pivotPoint = pivot;
        }
    }
    
    void UpdateFollowEffect() {
        if (!effectFollow) {
            return;
        }
        
        Color color = effectFollowColor.Evaluate(ValueF2);
        float scaleX = effectFollowScaleX.Evaluate(ValueF2);
        float scaleY = effectFollowScaleY.Evaluate(ValueF2);
        float scaleZ = effectFollowScaleZ.Evaluate(ValueF2);
        float rotation = effectFollowRotation.Evaluate(ValueF2) * 360;
        
        if (effectFollowSprite != null) {
            MadTransform.SetLocalPosition(effectFollowSprite.transform, EdgePosition());
            MadTransform.SetLocalScale(effectFollowSprite.transform, new Vector3(scaleX, scaleY, scaleZ));
            effectFollowSprite.tint = color;
            var newEulerAngles = new Vector3(0, 0, rotation);
            if (effectFollowSprite.transform.localEulerAngles != newEulerAngles) {
                effectFollowSprite.transform.localEulerAngles = newEulerAngles;
            }
        } else if (effectFollowObject != null && effectFollowObject is GameObject) {
            var worldPos = spriteBar.transform.TransformPoint(EdgePosition());
            GameObject obj = effectFollowObject as GameObject;
            MadTransform.SetPosition(obj.transform, worldPos);
            MadTransform.SetLocalScale(obj.transform, new Vector3(scaleX, scaleY, scaleZ));
            if (obj.renderer != null) {
                obj.renderer.sharedMaterial.color = color;
            }
            var newEulerAngles = new Vector3(0, 0, rotation);
            if (obj.transform.localEulerAngles != newEulerAngles) {
                obj.transform.localEulerAngles = newEulerAngles;
            }
            
        }
        
        
    }

    bool RebuildNeeded() {
        if (panel == null) {
            return false;
        }

        int ch = MadHashCode.FirstPrime;
        ch = MadHashCode.Add(ch, textureMode);

        ch = HashAdd(ch, panel);
        ch = HashAdd(ch, textureMode);
        ch = HashAddArray(ch, texturesBackground);
        ch = HashAddTexture(ch, textureBar);
        ch = HashAddArray(ch, texturesForeground);
        ch = HashAdd(ch, atlas);
        ch = HashAddArray(ch, atlasTexturesBackground);
        ch = HashAdd(ch, atlasTextureBarGUID);
        ch = HashAddArray(ch, atlasTexturesForeground);
        ch = HashAdd(ch, guiDepth);
        ch = HashAdd(ch, growDirection);
        ch = HashAdd(ch, effectBurn);
        ch = HashAddTexture(ch, effectBurnTextureBar);
        ch = HashAdd(ch, atlasEffectBurnTextureBarGUID);
        ch = HashAdd(ch, labelEnabled);
        ch = HashAdd(ch, labelFont);
        ch = HashAdd(ch, effectFollow);
        ch = HashAdd(ch, premultipliedAlpha);

        if (effectFollowObject != null && effectFollowObject is Texture) {
            ch = HashAddTexture(ch, effectFollowObject as Texture);
        } else {
            ch = HashAdd(ch, effectFollowObject);
        }
        
        ch = HashAdd(ch, radialOffset);
        ch = HashAdd(ch, radialLength);
        
        if (ch != lastRebuildHash || dirty) {
            lastRebuildHash = ch;
            dirty = false;
            return true;
        } else {
            return false;
        }
    }

    protected override void Rebuild() {
        base.Rebuild();

        // remove all hidden child sprites
        var hidden = MadTransform.FindChildren<MadSprite>(
            transform, (s) => s.gameObject.hideFlags == HideFlags.HideInHierarchy);
        hidden.ForEach((s) => MadGameObject.SafeDestroy(s));
    
        int nextDepth = guiDepth * DepthSpace;
        
        // build background
        nextDepth = BuildBackgroundTextures(nextDepth);

        // build the bar        
        if (textureBar != null) {
            if (effectBurn) {
                spriteBurnBar = CreateHidden<MadSprite>("bar_effect_burn");
                spriteBurnBar.guiDepth = nextDepth++;

                if (TextureValid(effectBurnTextureBar, atlasEffectBurnTextureBarGUID)) {
                    SetTexture(spriteBurnBar, effectBurnTextureBar, atlasEffectBurnTextureBarGUID);
                } else {
                    SetTexture(spriteBurnBar, textureBar, atlasTextureBarGUID);
                }
                
                spriteBurnBar.fillType = ToFillType(growDirection);
                spriteBurnBar.radialFillOffset = radialOffset;
                spriteBurnBar.radialFillLength = radialLength;
            }
        
            spriteBar = CreateHidden<MadSprite>("bar");
            spriteBar.guiDepth = nextDepth++;

            SetTexture(spriteBar, textureBar, atlasTextureBarGUID);
            
            spriteBar.fillType = ToFillType(growDirection);
            spriteBar.radialFillOffset = radialOffset;
            spriteBar.radialFillLength = radialLength;
        }
        
        // build foreground textures
        nextDepth = BuildForegroundTextures(nextDepth);
        
        // follow effect
        if (effectFollow) {
            if (effectFollowObject != null && effectFollowObject is Texture2D) {
                effectFollowSprite = CreateHidden<MadSprite>("bar_effect_follow");
                effectFollowSprite.texture = effectFollowObject as Texture2D;
                effectFollowSprite.guiDepth = nextDepth++;
            }
        }
        
        nextDepth = RebuildLabel(nextDepth);

        UpdateContainer();
    }
    
    MadSprite.FillType ToFillType(GrowDirection growDirection) {
        switch (growDirection) {
            case GrowDirection.LeftToRight:
                return MadSprite.FillType.LeftToRight;
            case GrowDirection.RightToLeft:
                return MadSprite.FillType.RightToLeft;
            case GrowDirection.TopToBottom:
                return MadSprite.FillType.TopToBottom;
            case GrowDirection.BottomToTop:
                return MadSprite.FillType.BottomToTop;
            case GrowDirection.ExpandHorizontal:
                return MadSprite.FillType.ExpandHorizontal;
            case GrowDirection.ExpandVertical:
                return MadSprite.FillType.ExpandVertical;
            case GrowDirection.RadialCW:
                return MadSprite.FillType.RadialCW;
            case GrowDirection.RadialCCW:
                return MadSprite.FillType.RadialCCW;
            case GrowDirection.ColorChange:
                return MadSprite.FillType.None;
            default:
                MadDebug.Assert(false, "Unkwnown grow direction: " + growDirection);
                return MadSprite.FillType.None;
        }
    }
    
    public bool GrowDirectionSupportedByFollowEffect(GrowDirection growDirection) {
        switch (growDirection) {
            case EnergyBarBase.GrowDirection.LeftToRight:
            case EnergyBarBase.GrowDirection.RightToLeft:
            case EnergyBarBase.GrowDirection.TopToBottom:
            case EnergyBarBase.GrowDirection.BottomToTop:
                return true;
            default:
                return false;
        }
    }
    
    Vector2 EdgePosition() {
        if (spriteBar == null) {
            return Vector2.zero;
        }
        
        float left = spriteBar.liveLeft * spriteBar.size.x;
        float right = spriteBar.liveRight * spriteBar.size.x;
        float top = spriteBar.liveTop * spriteBar.size.y;
        float bottom = spriteBar.liveBottom * spriteBar.size.y;
        float width = right - left;
        float height = top - bottom;
        float centerX = left + width / 2;
        float centerY = bottom + height / 2;
        
        Vector2 pos;
        
        switch (growDirection) {
        case GrowDirection.LeftToRight:
            pos = new Vector2(left + width * ValueF2, centerY);
            break;
        case GrowDirection.RightToLeft:
            pos = new Vector2(left + width * (1 - ValueF2), centerY);
            break;
        case GrowDirection.BottomToTop:
            pos = new Vector2(centerX, bottom + height * (1 - ValueF2));
            break;
        case GrowDirection.TopToBottom:
            pos = new Vector2(centerX, bottom + height * ValueF2);
            break;
        default:
            pos = Vector2.zero;
            break;
        }
        
        var offset = ComputeOffset();
        
        return new Vector2(pos.x - offset.x, -pos.y + offset.y);
    }
    
    Vector2 ComputeOffset() {
        var dar = DrawAreaRect;
        switch (pivot) {
            case Pivot.Left:
                return new Vector2(0, dar.height / 2);
            case Pivot.Top:
                return new Vector2(dar.width / 2, 0);
            case Pivot.Right:
                return new Vector2(dar.width, dar.height / 2);
            case Pivot.Bottom:
                return new Vector2(dar.width / 2, dar.height);
            case Pivot.TopLeft:
                return Vector2.zero;
            case Pivot.TopRight:
                return new Vector2(dar.width, 0);
            case Pivot.BottomRight:
                return new Vector2(dar.width, dar.height);
            case Pivot.BottomLeft:
                return new Vector2(0, dar.height);
            case Pivot.Center:
                return new Vector2(dar.width / 2, dar.height / 2);
            default:
                Debug.LogError("Unknown pivot: " + pivot);
                return Vector2.zero;
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