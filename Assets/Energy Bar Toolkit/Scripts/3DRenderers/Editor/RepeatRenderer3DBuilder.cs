/*
* Energy Bar Toolkit by Mad Pixel Machine
* http://www.madpixelmachine.com
*/

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace EnergyBarToolkit {

public class RepeatRenderer3DBuilder {

    // ===========================================================
    // Static Methods
    // ===========================================================

    public static RepeatedRenderer3D Create() {
        var panel = MadPanel.UniqueOrNull();

        if (panel == null) {
            var panels = MadPanel.All();
            if (panels.Length == 0) {
                if (EditorUtility.DisplayDialog(
                "Init Scene?",
                "Scene not initialized for 3D bars. You cannot place new bar without proper initialization. Do it now?",
                "Yes", "No")) {
                    MadInitTool.ShowWindow();
                }
            } else {
                CreateMeshBarTool.ShowWindow(EnergyBar3DBase.BarType.Repeated);
            }

            return null;
        } else {
            return Create(panel);
        }
    }

    public static RepeatedRenderer3D Create(MadPanel panel) {
        var bar = MadTransform.CreateChild<RepeatedRenderer3D>(panel.transform, "repeat progress bar");
        TryApplyExampleTextures(bar);
        Selection.activeObject = bar.gameObject;
        
        return bar;
    }
    
    static void TryApplyExampleTextures(RepeatedRenderer3D bar) {
        var textureIcon = AssetDatabase.LoadAssetAtPath(
            "Assets/Energy Bar Toolkit/Example/Textures/RepeatBar/heart.png", typeof(Texture2D)) as Texture2D;
        var textureSlot = AssetDatabase.LoadAssetAtPath(
            "Assets/Energy Bar Toolkit/Example/Textures/RepeatBar/heartSlot.png", typeof(Texture2D)) as Texture2D;
        
        if (textureSlot != null && textureSlot != null) {
            bar.textureIcon = textureIcon;
            bar.textureSlot = textureSlot;
        } else {
            Debug.LogWarning("Failed to locate example textures. This is not something bad, but have you changed "
                + "your Energy Bar Toolkit directory location?");
        }
    }
    
    // ===========================================================
    // Inner and Anonymous Classes
    // ===========================================================

}

} // namespace