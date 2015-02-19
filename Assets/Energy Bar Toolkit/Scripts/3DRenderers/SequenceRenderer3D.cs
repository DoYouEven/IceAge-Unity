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
public class SequenceRenderer3D : EnergyBar3DBase {

    #region Fields public
    
    // rendering method
    public Method renderingMethod = Method.Grid;
    
    // grid
    public Texture2D gridTexture; // non-atlas
    public string gridAtlasTextureGUID;
    public int gridWidth = 2;
    public int gridHeight = 2;
    public bool gridFrameCountManual = false;
    public int gridFrameCount = 4;
    public Color gridTint = Color.white;
    
    // sequence
    public Texture2D[] sequenceTextures = new Texture2D[0]; // non-atlas
    public string[] sequenceAtlasTexturesGUID = new string[0];
    
    
    #endregion

    #region Fields private

    private MadSprite spriteBar;

    private int lastRebuildHash;
    
    private bool dirty = true;
    
    #endregion

    #region Properties
        public override Rect DrawAreaRect {
            get {
                if (spriteBar != null && spriteBar.CanDraw()) {
                    return spriteBar.GetTransformedBounds();
                } else if (gridTexture != null) {
                    // if there's no sprite set, but texture bar is set then this means that rebuild
                    // is not done yet. Trying to calculate bounds manually.
                    Vector2 offset = PivotOffset(pivot);
                    float w = gridTexture.width;
                    float h = gridTexture.height;

                    if (renderingMethod == Method.Grid) {
                        w /= gridWidth;
                        h /= gridHeight;
                    }

                    return new Rect(offset.x * w, (1 - offset.y) * h, w, h);
                } else {
                    return new Rect();
                }
            }
        }

    #endregion

    #region Methods slots

    protected override void OnEnable() {
        base.OnEnable();
    }
    
    protected override void Update() {
        base.Update();

        if (RebuildNeeded()) {
            Rebuild();
        }

        if (panel == null) {
            return;
        }

        UpdateColor();
        UpdateProgress();

        if (renderingMethod == Method.Grid && spriteBar.CanDraw()) {
            spriteBar.size = new Vector2(spriteBar.initialSize.x / gridWidth, spriteBar.initialSize.y / gridHeight);
        }

        spriteBar.pivotPoint = Translate(pivot);
    }

    void UpdateColor() {
        if (spriteBar == null) {
            return;
        }

        spriteBar.tint = ComputeColor(gridTint);
    }

    void UpdateProgress() {
        switch (renderingMethod) {
            case Method.Grid:
                UpdateGridProgress();
                break;
            case Method.Sequence:
                UpdateSequenceProgress();
                break;
            default:
                Debug.Log("Unknown rendering method: " + renderingMethod);
                break;
        }
    }
    
    void UpdateGridProgress() {
        if (useAtlas) {
            spriteBar.inputType = MadSprite.InputType.TextureAtlas;
            spriteBar.textureAtlas = atlas;
            spriteBar.textureAtlasSpriteGUID = gridAtlasTextureGUID;
        } else {
            spriteBar.texture = gridTexture;
        }
        
        int size;
        if (gridFrameCountManual) {
            size = gridFrameCount;
        } else {
            size = gridWidth * gridHeight;
        }
        
        int index = Index(size);
        float y = (gridHeight - 1 - index / gridWidth) / (float) gridHeight;
        float x = index % gridWidth / (float) gridWidth;
        
        spriteBar.textureRepeat = new Vector2(1f / gridWidth, 1f / gridHeight);
        spriteBar.textureOffset = new Vector2(x, y);
    }
    
    void UpdateSequenceProgress() {
        if (useAtlas) {
            if (sequenceAtlasTexturesGUID.Length > 0) {
                spriteBar.textureAtlas = atlas;
                spriteBar.textureAtlasSpriteGUID = sequenceAtlasTexturesGUID[Index(sequenceAtlasTexturesGUID.Length)];
            }
        } else {
            if (sequenceTextures.Length > 0) {
                spriteBar.texture = sequenceTextures[Index(sequenceTextures.Length)];
            }
        }
    }
    
    private int Index(int size) {
        int index = (int) Mathf.Min(Mathf.Floor(ValueF2 * size), size - 1);
        return index;
    }
    
    #endregion

#region Methods private

    bool RebuildNeeded() {
        if (panel == null) {
            return false;
        }

        var hash = new MadHashCode();
        hash.Add(textureMode);
        hash.Add(atlas);
            
        hash.AddEnumerable(texturesBackground);
        hash.AddEnumerable(atlasTexturesBackground);
        hash.AddEnumerable(texturesForeground);
        hash.AddEnumerable(atlasTexturesForeground);
        hash.Add(renderingMethod);
        hash.Add(gridTexture);
        hash.Add(gridAtlasTextureGUID);
        hash.AddEnumerable(sequenceTextures);
        hash.AddEnumerable(sequenceAtlasTexturesGUID);
        hash.Add(gridWidth);
        hash.Add(gridHeight);
        hash.Add(gridFrameCountManual);
        hash.Add(gridFrameCount);
        hash.Add(guiDepth);
        hash.Add(labelEnabled);
        hash.Add(labelFont);
        hash.Add(premultipliedAlpha);
        
        int hashNumber = hash.GetHashCode();
        
        if (hashNumber != lastRebuildHash || dirty) {
            lastRebuildHash = hashNumber;
            dirty = false;
            return true;
        } else {
            return false;
        }
    }
    
    protected override void Rebuild() {
        base.Rebuild();
        
        // remove used sprites
        if (spriteBar != null) {
            MadGameObject.SafeDestroy(spriteBar.gameObject);
        }
        
        int nextDepth = guiDepth * DepthSpace;
        
        nextDepth = BuildBackgroundTextures(nextDepth);
        nextDepth = BuildBar(nextDepth);
        nextDepth = BuildForegroundTextures(nextDepth);
        nextDepth = RebuildLabel(nextDepth);

        UpdateContainer();
    }
    
    int BuildBar(int nextDepth) {
        spriteBar = CreateHidden<MadSprite>("bar");

        spriteBar.guiDepth = nextDepth++;

        SetTexture(spriteBar, gridTexture, gridAtlasTextureGUID);
        
        return nextDepth;
    }

    #endregion

    #region Inner Classes

    public enum Method {
        Grid,
        Sequence
    }

    #endregion

}

} // namespace