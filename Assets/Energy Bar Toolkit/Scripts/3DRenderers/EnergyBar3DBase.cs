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

public abstract class EnergyBar3DBase : EnergyBarBase {

#region Contants
    // how much depth values will be reserved for single energy bar
    public const int DepthSpace = 32;
#endregion

    #region Fields public

    public MadPanel panel;

    public TextureMode textureMode = TextureMode.Textures;
    
    // used only when texture mode is atlas
    public MadAtlas atlas;
    public AtlasTex[] atlasTexturesBackground = new AtlasTex[0];
    public AtlasTex[] atlasTexturesForeground = new AtlasTex[0];

    // transforms
    public LookAtMode lookAtMode = LookAtMode.Disabled;
    public GameObject lookAtObject;
    
    // label
    public MadFont labelFont;
    public float labelScale = 32;
    public Pivot labelPivot = Pivot.Center;
    
    // determines if this bar is selectable in the editor
    public bool editorSelectable = true;
    
    #endregion
    
    #region Fields private

    // tells if this instance (or prefab) is under the panel
    // on change when panel is null scale should be changed too
    [SerializeField]
    private bool underThePanel = true;

    // created label sprite object
    private MadText labelSprite;
    
    // created background sprite objects
    private List<MadSprite> spriteObjectsBg = new List<MadSprite>();
    
    // created foreground sprite objects
    private List<MadSprite> spriteObjectsFg = new List<MadSprite>();

    private List<GameObject> hiddenObjects = new List<GameObject>();
    
    #endregion

    #region Properties
    
    protected bool useAtlas {
        get {
            return textureMode == TextureMode.TextureAtlas;
        }
    }
    
    public virtual Pivot pivot {
        get {
            return _pivot;
        }
        
        set {
            _pivot = value;
        }
    }
    [SerializeField]
    private Pivot _pivot = Pivot.Center;
    #endregion 
    
    #region Unity Methods

#if UNITY_EDITOR
    void OnDrawGizmos() {

        // Draw the gizmo
        Gizmos.matrix = transform.localToWorldMatrix;

        Gizmos.color = (UnityEditor.Selection.activeGameObject == gameObject)
            ? Color.green : new Color(1, 1, 1, 0.2f);

        Bounds totalBounds = new Bounds(Vector3.zero, Vector3.zero);
        bool totalBoundsSet = false;

        foreach (var hiddenObject in hiddenObjects) {
            var sprite = hiddenObject.GetComponent<MadSprite>();

            if (sprite == null || !sprite.CanDraw()) {
                return;
            }

            Rect boundsRect = sprite.GetTransformedBounds();
            Bounds bounds = new Bounds(boundsRect.center, new Vector2(boundsRect.width, boundsRect.height));

            if (!totalBoundsSet) {
                totalBounds = bounds;
                totalBoundsSet = true;
            } else {
                totalBounds.Encapsulate(bounds);
            }
        }


        Gizmos.DrawWireCube(totalBounds.center, totalBounds.size);

        if (editorSelectable) {
            // Make the widget selectable
            Gizmos.color = Color.clear;
            Gizmos.DrawCube(totalBounds.center,
                            new Vector3(totalBounds.size.x, totalBounds.size.y, 0.01f * (guiDepth + 1)));
        }
    }
#endif

    protected override void OnEnable() {
        base.OnEnable();
        EnableAllHidden();
    }

    protected override void Start() {
        base.Start();
        ReconnectPanelIfNeeded(true);
    }

    protected override void OnDisable() {
        base.OnDisable();
        DisableAllHidden();
    }

    protected virtual void OnDestroy() {
        DestroyAllHidden(true);
    }

    #endregion

    #region Unity Methods Update
    
    protected override void Update() {
        base.Update();

        ReconnectPanelIfNeeded(false);
        UpdatePanelInfo();

        UpdateContainer();

        UpdateLabel();
        UpdateAnchor();
        UpdateColors();
        UpdatePivot();
        UpdateLookAt();
    }

    protected void UpdateContainer() {
        if (container != null) {
            ApplyTransform(container);
        }
    }

    void UpdateLabel() {
        if (labelSprite == null) {
            return;
        }

        labelSprite.scale = labelScale;

        labelSprite.pivotPoint = Translate(labelPivot);
        labelSprite.transform.localPosition = LabelPositionPixels;
        
        labelSprite.text = LabelFormatResolve(labelFormat);
        labelSprite.tint = ComputeColor(labelColor);
    }
    
    void UpdateAnchor() {
        // rewriting anchor objects to make them easy accessible
        var anchor = MadTransform.FindParent<MadAnchor>(transform, 1);
        if (anchor != null) {
            anchorObject = anchor.anchorObject;
            anchorCamera = anchor.anchorCamera;
        }
    }
    
    void UpdateColors() {
        if (textureMode == TextureMode.Textures) {
            UpdateTextureColors(spriteObjectsBg, texturesBackground);
            UpdateTextureColors(spriteObjectsFg, texturesForeground);
        } else {
            UpdateTextureColors(spriteObjectsBg, atlasTexturesBackground);
            UpdateTextureColors(spriteObjectsFg, atlasTexturesForeground);
        }
    }
    
    void UpdateTextureColors(List<MadSprite> sprites, AbstractTex[] textures) {
        if (sprites.Count != textures.Length) {
            return; // textures not yet created
        }

        for (int i = 0; i < sprites.Count; i++) {
            var sprite = sprites[i];
            var texture = textures[i];
            
            if (sprite == null) {
                continue;
            }
            
            sprite.visible = IsVisible();
            sprite.tint = ComputeColor(texture.color);
        }
    }
    
    void UpdatePivot() {
        var pivot = Translate(this.pivot);
        for (int i = 0; i < spriteObjectsBg.Count; ++i) {
            var sprite = spriteObjectsBg[i];
            if (sprite != null) {
                sprite.pivotPoint = pivot;
            }
        }

        for (int i = 0; i < spriteObjectsFg.Count; ++i) {
            var sprite = spriteObjectsFg[i];
            if (sprite != null) {
                sprite.pivotPoint = pivot;
            }
        }
    }

    private void UpdateLookAt() {
        Transform t;

        switch (lookAtMode) {
            case LookAtMode.Disabled:
                // do nothing
                return;

            case LookAtMode.CustomObject:
                if (lookAtObject != null) {
                    t = lookAtObject.transform;
                } else {
                    return;
                }
                break;

            case LookAtMode.MainCamera:
                var camera = Camera.main;
                if (camera != null) {
                    t = camera.transform;
                } else {
                    Debug.LogError("Cannot find MainCamera.");
                    return;
                }
                break;

            default:
                Debug.LogError("Unknown option: " + lookAtMode);
                return;
        }

        transform.LookAt(transform.position + (transform.position - t.position));
    }

    private void ReconnectPanelIfNeeded(bool firstTime) {
        if (panel == null) {
            panel = MadPanel.FirstOrNull(transform);
            if (panel == null && firstTime) {
                Debug.LogError("You have to initialize scene first! Please execute Tools -> Energy Bar Toolkit -> Initialize");
            } else {
                // check if now I am under the panel
                var topPanel = MadTransform.FindParent<MadPanel>(transform);
                bool nowUnderThePanel = topPanel == panel;

                // new scale should be applied if location of bar relative to panel has changed
                if (nowUnderThePanel && !underThePanel) {
                    // now is under the panel, but wasn't before
                    transform.localScale /= panel.transform.lossyScale.x;
                } else if (!nowUnderThePanel && underThePanel) {
                    // was under the panel before, now it isn't
                    transform.localScale *= panel.transform.lossyScale.x;
                }

                underThePanel = nowUnderThePanel;
            }
        }
    }

    // updates information about this bar position
    // this method is quite heavy, but it will only execute in editor and when application is not playing
    private void UpdatePanelInfo() {
        if (panel != null && Application.isEditor && !Application.isPlaying) {
            var topPanel = MadTransform.FindParent<MadPanel>(transform);
            bool nowUnderThePanel = topPanel == panel;

            underThePanel = nowUnderThePanel;
        }
    }

#endregion

    #region Methods rebuild

    private void EnableAllHidden() {
        for (int i = 0; i < hiddenObjects.Count; ++i) {
            hiddenObjects[i].SetActive(true);
        }

        if (container != null) {
            container.gameObject.SetActive(true);
        }
    }

    private void DisableAllHidden() {
        for (int i = 0; i < hiddenObjects.Count; ++i) {
            hiddenObjects[i].SetActive(false);
        }

        if (container != null) {
            container.gameObject.SetActive(false);
        }
    }

    private void DestroyAllHidden(bool forceImmediate = false) {
        for (int i = 0; i < hiddenObjects.Count; ++i) {
            DestroyHidden(hiddenObjects[i], forceImmediate);
        }

        hiddenObjects.Clear();

        if (container != null) {
            DestroyHidden(container.gameObject, forceImmediate);
        }
    }
    
    protected virtual void Rebuild() {
#if MAD_DEBUG
        Debug.Log("rebuilding " + this, this);
#endif

        spriteObjectsBg.Clear();
        spriteObjectsFg.Clear();

        DestroyAllHidden();

        // as a precaution I will search for hidden sprites at level 0
        var hidden = MadTransform.FindChildren<MadSprite>(transform, (s) => (s.hideFlags | HideFlags.HideInHierarchy) != 0, 0);
        if (hidden.Count > 0) {
            Debug.Log("There were " + hidden.Count + " hidden unmanaged sprites under this bar. I will remove them.");
            for (int i = 0; i < hidden.Count; ++i) {
                MadGameObject.SafeDestroy(hidden[i].gameObject);
            }
        }
    }
    
    protected int BuildBackgroundTextures(int depth) {
        if (useAtlas) {
            return BuildTextures(atlasTexturesBackground, "bg_", depth, ref spriteObjectsBg);
        } else {
            return BuildTextures(texturesBackground, "bg_", depth, ref spriteObjectsBg);
        }
    }
    
    protected int BuildForegroundTextures(int depth) {
        if (useAtlas) {
            return BuildTextures(atlasTexturesForeground, "fg_", depth, ref spriteObjectsFg);
        } else {
            return BuildTextures(texturesForeground, "fg_", depth, ref spriteObjectsFg);
        }
    }
    
    int BuildTextures<T>(T[] textures, string prefix, int startDepth, ref List<MadSprite> sprites) {
        
        int counter = 0;
        foreach (var gTexture in textures) {
            Tex tex = gTexture as Tex;
            AtlasTex atlasTex = gTexture as AtlasTex;
            
            if ((tex != null && !tex.Valid) || (atlasTex != null && !atlasTex.Valid)) {
                continue;
            }
            
            string name = string.Format("_{0}{1:D2}", prefix, counter + 1);


            var sprite = CreateHidden<MadSprite>(name);
            
            sprite.guiDepth = startDepth + counter;
            
            if (tex != null) {
                sprite.texture = tex.texture;
            } else {
                sprite.inputType = MadSprite.InputType.TextureAtlas;
                sprite.textureAtlas = atlas;
                sprite.textureAtlasSpriteGUID = atlasTex.spriteGUID;
            }
            
            sprite.tint = tex != null ? tex.color : atlasTex.color;

            // binds color to not trigger rebuild when color is changed
            var colorBind = sprite.gameObject.AddComponent<ColorBind>();
            colorBind.tex = tex;
            
            sprites.Add(sprite);
            
            counter++;
        }
        
        return startDepth + counter;
    }
    
    protected int RebuildLabel(int depth) {
        if (labelEnabled && labelFont != null) {
            labelSprite = CreateHidden<MadText>("_label");
            labelSprite.font = labelFont;
            labelSprite.guiDepth = depth++;
        }
        
        // after build we must update label at least once to make it visible
        UpdateLabel();
        
        return depth;
    }

    #endregion

    #region Protected Helper Methods

    public Rect AnyBackgroundOrForegroundSpriteSize() {
        MadSprite sprite = null;

        if (spriteObjectsBg.Count > 0) {
            sprite = spriteObjectsBg[0];
        }

        if (sprite == null && spriteObjectsFg.Count > 0) {
            sprite = spriteObjectsFg[0];
        }

        if (sprite != null) {
            return sprite.GetTransformedBounds();
        }

        // no sprites, looking for textures
        Texture2D texture = null;
        if (texturesBackground.Length > 0) {
            texture = texturesBackground[0].texture;
        }

        if (texture == null && texturesForeground.Length > 0) {
            texture = texturesForeground[0].texture;
        }

        if (texture != null) {
            Vector2 offset = PivotOffset(pivot);
            float w = texture.width;
            float h = texture.height;
            return new Rect(offset.x * w, (1 - offset.y) * h, w, h);
        }

        return new Rect();
    }

    protected void ApplyTransform(Component c) {
        if (c != null) {
            ApplyTransform(c.gameObject);
        }
    }

    protected void ApplyTransform(GameObject go) {
        if (go != null) {
            go.transform.position =  transform.position;
            go.transform.localScale = transform.lossyScale;
            go.transform.rotation = transform.rotation;
        }
    }

    protected void DestroyHidden(GameObject go, bool forceImmediate) {
        if (forceImmediate) {
            GameObject.DestroyImmediate(go);
        } else {
            MadGameObject.SafeDestroy(go);
        }
    }

    private Transform container;

    protected T CreateHidden<T>(string name, Transform parent = null) where T : Component {
        if (container == null) {
            // create container
            container = new GameObject("_container").transform;

#if !MAD_DEBUG
            container.gameObject.hideFlags = HideFlags.HideAndDontSave;
#else
            container.gameObject.hideFlags = HideFlags.DontSave;
#endif
        }

        if (parent == null) {
            parent = container;
        }

        var obj = MadTransform.CreateChild<T>(parent, name, true);

        // if created object is mad sprite, then assign the panel
        if (obj is MadSprite) {
            var sprite = obj as MadSprite;
            sprite.panel = panel;

            if (obj.GetType() == typeof(MadSprite)) {
                sprite.hasPremultipliedAlpha = premultipliedAlpha;
            }
        }

        MadGameObject.SetActive(obj.gameObject, true);

        #if !MAD_DEBUG
        obj.gameObject.hideFlags = HideFlags.HideAndDontSave;
        #else
        obj.gameObject.hideFlags = HideFlags.DontSave;
        #endif

        hiddenObjects.Insert(0, obj.gameObject);

        return obj;
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Tells if one of given textures in valid, depending on if this bar is using atlases or not.
    /// </summary>
    protected bool TextureValid(Texture2D tex, string atlasTex) {
        if (useAtlas) {
            return AtlasTextureValid(atlasTex);
        } else {
            return tex != null;
        }
    }

    /// <summary>
    /// Assing regular texture or atlas texture based on current settings.
    /// </summary>
    protected void SetTexture(MadSprite sprite, Texture2D tex, string atlasTex) {
        if (useAtlas) {
            sprite.inputType = MadSprite.InputType.TextureAtlas;
            sprite.textureAtlas = atlas;
            sprite.textureAtlasSpriteGUID = atlasTex;
            sprite.texture = null;
        } else {
            sprite.inputType = MadSprite.InputType.SingleTexture;
            sprite.textureAtlas = null;
            sprite.lastTextureAtlasSpriteGUID = null;
            sprite.texture = tex;
        }
    }

    /// <summary>
    /// Tells if given texture guid is valid, and it is valid only if atlas is set, and it contains the given texture.
    /// </summary>
    protected bool AtlasTextureValid(string guid) {
        if (atlas == null) {
            return false;
        }

        var item = atlas.GetItem(guid);

        if (item != null) {
            return true;
        } else {
            return false;
        }
    }

    #endregion

    #region Static Methods
    
    protected static MadSprite.PivotPoint Translate(Pivot pivot) {
        switch (pivot) {
            case Pivot.Left:
                return MadSprite.PivotPoint.Left;
            case Pivot.Top:
                return MadSprite.PivotPoint.Top;
            case Pivot.Right:
                return MadSprite.PivotPoint.Right;
            case Pivot.Bottom:
                return MadSprite.PivotPoint.Bottom;
            case Pivot.TopLeft:
                return MadSprite.PivotPoint.TopLeft;
            case Pivot.TopRight:
                return MadSprite.PivotPoint.TopRight;
            case Pivot.BottomRight:
                return MadSprite.PivotPoint.BottomRight;
            case Pivot.BottomLeft:
                return MadSprite.PivotPoint.BottomLeft;
            case Pivot.Center:
                return MadSprite.PivotPoint.Center;
            default:
                Debug.Log("Unknown pivot point: " + pivot);
                return MadSprite.PivotPoint.Center;
        }
    }
    
    protected static Vector2 PivotOffset(Pivot pivot) {
        switch (pivot) {
            case Pivot.Left:
                return new Vector2(0f, -0.5f);
            case Pivot.Top:
                return new Vector2(-0.5f, -1f);
            case Pivot.Right:
                return new Vector2(-1f, -0.5f);
            case Pivot.Bottom:
                return new Vector2(-0.5f, 0f);
            case Pivot.TopLeft:
                return new Vector2(0f, -1f);
            case Pivot.TopRight:
                return new Vector2(-1f, -1f);
            case Pivot.BottomRight:
                return new Vector2(-1f, 0f);
            case Pivot.BottomLeft:
                return new Vector2(0f, 0f);
            case Pivot.Center:
                return new Vector2(-0.5f, -0.5f);
            default:
                Debug.Log("Unknown pivot point: " + pivot);
                return Vector2.zero;
        }
    }

    #endregion

    #region Inner Types
    
    // this intentionally shadows base declaration. The other Pivot order is just bad...
    public enum Pivot {
        Left,
        Top,
        Right,
        Bottom,
        TopLeft,
        TopRight,
        BottomRight,
        BottomLeft,
        Center,
    }
    
    public enum BarType {
        Filled,
        Repeated,
        Sequence,
        Transform,
    }
    
    public enum TextureMode {
        Textures,
        TextureAtlas,
    }

    public enum LookAtMode {
        Disabled,
        CustomObject,
        MainCamera,
    }
    
    [System.Serializable]
    public class AtlasTex : AbstractTex {
        public string spriteGUID = "";
        
        public bool Valid {
            get {
                return !string.IsNullOrEmpty(spriteGUID);
            }
        }
        
        public override int GetHashCode() {
            var hash = new MadHashCode();
            hash.Add(spriteGUID);
            hash.Add(color);
            
            return hash.GetHashCode();
        }
    }

    #endregion
}

} // namespace