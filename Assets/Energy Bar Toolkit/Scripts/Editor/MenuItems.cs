/*
* Energy Bar Toolkit by Mad Pixel Machine
* http://www.madpixelmachine.com
*/

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace EnergyBarToolkit {

public class MenuItems : ScriptableObject {

    // ===========================================================
    // Constants
    // ===========================================================

    // ===========================================================
    // Fields
    // ===========================================================

    // ===========================================================
    // Methods for/from SuperClass/Interfaces
    // ===========================================================

    // ===========================================================
    // Methods
    // ===========================================================
    
    [MenuItem("Tools/Energy Bar Toolkit/Initialize", false, 100)]
    static void InitTool() {
        MadInitTool.ShowWindow();
    }
    
    [MenuItem ("Tools/Energy Bar Toolkit/Create Atlas", false, 120)]
    static void CreateAtlas() {
        MadAtlasBuilder.CreateAtlas();
    }
    
    [MenuItem ("Tools/Energy Bar Toolkit/Create Font", false, 130)]
    static void CreateFont() {
        MadFontBuilder.CreateFont();
    }
    
    [MenuItem ("Tools/Energy Bar Toolkit/Create UI/Sprite", false, 140)]
    static void CreateSprite() {
        var sprite = MadTransform.CreateChild<MadSprite>(ActiveParentOrPanel(), "sprite");
        Selection.activeGameObject = sprite.gameObject;
    }
    
    [MenuItem ("Tools/Energy Bar Toolkit/Create UI/Text", false, 141)]
    static void CreateText() {
        var text = MadTransform.CreateChild<MadText>(ActiveParentOrPanel(), "text");
        Selection.activeGameObject = text.gameObject;
    }
    
    [MenuItem ("Tools/Energy Bar Toolkit/Create UI/Anchor", false, 142)]
    static void CreateAnchor() {
        var anchor = MadTransform.CreateChild<MadAnchor>(ActiveParentOrPanel(), "Anchor");
        Selection.activeGameObject = anchor.gameObject;
    }
    
    static Transform ActiveParentOrPanel() {
        Transform parentTransform = null;
        
        var transforms = Selection.transforms;
        if (transforms.Length > 0) {
            var firstTransform = transforms[0];
            if (MadTransform.FindParent<MadPanel>(firstTransform) != null) {
                parentTransform = firstTransform;
            }
        }
        
        if (parentTransform == null) {
            var panel = MadPanel.UniqueOrNull();
            if (panel != null) {
                parentTransform = panel.transform;
            }
        }
        
        return parentTransform;
    }
    
    static T Create<T>(string name) where T : Component {
        var parent = Selection.activeTransform;
        var component = MadTransform.CreateChild<T>(parent, name);
        Selection.activeObject = component.gameObject;
        return component;
    }
    
    [MenuItem("Tools/Energy Bar Toolkit/Create Mesh Bar/Filled", false, 150)]
    [MenuItem("GameObject/Create Other/Energy Bar Toolkit/Mesh Filled Bar", false, 2010)]
    static void CreateMeshFillRenderer() {
        FilledRenderer3DBuilder.Create();
    }
    
    [MenuItem("Tools/Energy Bar Toolkit/Create Mesh Bar/Repeated", false, 151)]
    [MenuItem("GameObject/Create Other/Energy Bar Toolkit/Mesh Repeated Bar", false, 2020)]
    static void CreateMeshRepeatRenderer() {
        RepeatRenderer3DBuilder.Create();
    }

    [MenuItem("Tools/Energy Bar Toolkit/Create Mesh Bar/Sequence", false, 152)]
    [MenuItem("GameObject/Create Other/Energy Bar Toolkit/Mesh Sequence Bar", false, 2030)]
    static void CreateMeshSequenceRenderer() {
        SequenceRenderer3DBuilder.Create();
    }

    [MenuItem("Tools/Energy Bar Toolkit/Create Mesh Bar/Transform", false, 153)]
    [MenuItem("GameObject/Create Other/Energy Bar Toolkit/Mesh Transform Bar", false, 2040)]
    static void CreateMeshTransformRenderer() {
        TransformRenderer3DBuilder.Create();
    }

    [MenuItem("Tools/Energy Bar Toolkit/Create OnGUI Bar (Old)/Fill Renderer", false, 250)]
    static void CreateFillRendererOnGUI() {
        Create<EnergyBarRenderer>("fill renderer");
    }
    
    [MenuItem("Tools/Energy Bar Toolkit/Create OnGUI Bar (Old)/Repeat Renderer", false, 251)]
    static void CreateRepeatRendererOnGUI() {
        Create<EnergyBarRepeatRenderer>("repeat renderer");
    }
    
    [MenuItem("Tools/Energy Bar Toolkit/Create OnGUI Bar (Old)/Sequence Renderer", false, 252)]
    static void CreateSequenceRendererOnGUI() {
        Create<EnergyBarSequenceRenderer>("sequence renderer");
    }
    
    [MenuItem("Tools/Energy Bar Toolkit/Create OnGUI Bar (Old)/Transform Renderer", false, 253)]
    static void CreateTransformRendererOnGUI() {
        Create<EnergyBarTransformRenderer>("transform renderer");
    }
 
    [MenuItem("Tools/Energy Bar Toolkit/Online Resources/MadPixelMachine", false, 1000)]
    static void MadPixelMachine() {
        Application.OpenURL(
            "http://madpixelmachine.com");
    }
    
    [MenuItem("Tools/Energy Bar Toolkit/Online Resources/Support", false, 1100)]
    static void Support() {
        Application.OpenURL(
            "http://energybartoolkit.madpixelmachine.com/doc/latest/support.html");
    }
    
    [MenuItem("Tools/Energy Bar Toolkit/Online Resources/Online Manual", false, 1000)]
    static void OnlineManual() {
        Application.OpenURL(
            "http://energybartoolkit.madpixelmachine.com/documentation.html");
    }
    
    [MenuItem("Tools/Energy Bar Toolkit/Online Resources/Examples", false, 1000)]
    static void Examples() {
        Application.OpenURL(
            "http://energybartoolkit.madpixelmachine.com/demo.html");
    }
    
    [MenuItem("Tools/Energy Bar Toolkit/Online Resources/Change Log", false, 1000)]
    static void ReleaseNotes() {
        Application.OpenURL(
            "http://energybartoolkit.madpixelmachine.com/doc/latest/changelog.html");
    }

    // ===========================================================
    // Static Methods
    // ===========================================================

    // ===========================================================
    // Inner and Anonymous Classes
    // ===========================================================

}

} // namespace