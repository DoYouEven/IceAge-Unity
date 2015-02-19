/*
* Copyright (c) Mad Pixel Machine
* http://www.madpixelmachine.com/
*/

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace EnergyBarToolkit {

[ExecuteInEditMode]
[RequireComponent(typeof(EnergyBar))]
public class TransformRenderer3D : EnergyBar3DBase {

    #region Public Fields

    public Texture2D objectTexture; // non-atlas
    public string objectAtlasTextureGUID;
    public Color objectTint = Color.white;

    public Vector2 objectAnchor = new Vector2(0.5f, 0.5f);

    public bool transformTranslate;
    public bool transformRotate;
    public bool transformScale;

    public TranslateFunction translateFunction = new TranslateFunction();
    public RotateFunction rotateFunction = new RotateFunction();
    public ScaleFunction scaleFunction = new ScaleFunction();

    #endregion

    #region Private Fields

    private int lastRebuildHash;

    private bool dirty = true;

    private MadSprite objectSprite;

    #endregion

    #region Public Properties

    public override Rect DrawAreaRect {
        get {

            Rect rect = AnyBackgroundOrForegroundSpriteSize();
            if (rect.width == 0) {
                if (objectSprite != null && objectSprite.CanDraw()) {
                    rect = objectSprite.GetTransformedBounds();
                }
            }

            return rect;
        }
    }

    #endregion

    #region Slots

    protected override void Start() {
        base.Start();

        if (RebuildNeeded()) {
            Rebuild();
        }
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
        UpdateTransform();
    }

    #endregion

    #region Private Methods

    private void UpdateColor() {
        if (objectSprite != null) {
            objectSprite.tint = ComputeColor(objectTint);
        }
    }

    private void UpdateTransform() {
        if (transformTranslate) {
            Vector2 translate = translateFunction.Value(ValueF2);

            var bounds = objectSprite.GetBounds();
            objectSprite.transform.localPosition = new Vector2(
                translate.x * bounds.width,
                translate.y * bounds.height);
        }

        if (transformRotate) {
            Quaternion rotate = rotateFunction.Value(ValueF2);
            objectSprite.transform.localRotation = Quaternion.identity * Quaternion.Inverse(rotate);
        }

        if (transformScale) {
            Vector3 scale = scaleFunction.Value(ValueF2);
            objectSprite.transform.localScale = scale;
        }
    }

    private bool RebuildNeeded() {
        if (panel == null) {
            return false;
        }

        var hash = new MadHashCode();

        hash.Add(textureMode);
        hash.Add(atlas);

        hash.Add(objectTexture);
        hash.Add(objectAtlasTextureGUID);

        hash.Add(objectAnchor);

        hash.AddEnumerable(texturesBackground);
        hash.AddEnumerable(atlasTexturesBackground);
        hash.AddEnumerable(texturesForeground);
        hash.AddEnumerable(atlasTexturesForeground);

        hash.Add(transformTranslate);
        hash.Add(transformRotate);
        hash.Add(transformScale);

        hash.Add(translateFunction);
        hash.Add(rotateFunction);
        hash.Add(scaleFunction);

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
        if (objectSprite != null) {
            MadGameObject.SafeDestroy(objectSprite.gameObject);
        }

        int nextDepth = guiDepth * DepthSpace;

        nextDepth = BuildBackgroundTextures(nextDepth);
        nextDepth = BuildObject(nextDepth);
        nextDepth = BuildForegroundTextures(nextDepth);
        nextDepth = RebuildLabel(nextDepth);

        UpdateContainer();
    }

    private int BuildObject(int nextDepth) {
        objectSprite = CreateHidden<MadSprite>("object");

        objectSprite.guiDepth = nextDepth++;

        SetTexture(objectSprite, objectTexture, objectAtlasTextureGUID);

        objectSprite.pivotPoint = MadSprite.PivotPoint.Custom;
        objectSprite.customPivotPoint = new Vector2(objectAnchor.x, 1 - objectAnchor.y);

        return nextDepth;
    }

    #endregion
}

} // namespace