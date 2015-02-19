/*
* Energy Bar Toolkit by Mad Pixel Machine
* http://www.madpixelmachine.com
*/

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace EnergyBarToolkit {

// editor utilities for Energy Bar Toolkit
public class EnergyBarUtils : MonoBehaviour {

    // ===========================================================
    // Constants
    // ===========================================================

    // ===========================================================
    // Methods
    // ===========================================================

    public static EnergyBar3DBase Create3DBar(EnergyBar3DBase.BarType type) {
        var panel = MadPanel.FirstOrNull(null);
        if (panel == null) {
            EditorUtility.DisplayDialog("Not Initialized", "You have to initialize EBT first", "OK");
            MadInitTool.ShowWindow();
            return null;
        } else {
            switch (type) {
                case EnergyBar3DBase.BarType.Filled:
                    return FilledRenderer3DBuilder.Create();
                case EnergyBar3DBase.BarType.Repeated:
                    return RepeatRenderer3DBuilder.Create();
                case EnergyBar3DBase.BarType.Sequence:
                    return SequenceRenderer3DBuilder.Create();
                case EnergyBar3DBase.BarType.Transform:
                    return TransformRenderer3DBuilder.Create();
                default:
                    Debug.LogError("Unknown bar type: " + type);
                    return null;
            }

        }
    }

    public static EnergyBar3DBase Create3DBar(EnergyBar3DBase.BarType type, MadPanel panel) {
        switch (type) {
            case EnergyBar3DBase.BarType.Filled:
                return FilledRenderer3DBuilder.Create(panel);
            case EnergyBar3DBase.BarType.Repeated:
                return RepeatRenderer3DBuilder.Create(panel);
            case EnergyBar3DBase.BarType.Sequence:
                return SequenceRenderer3DBuilder.Create(panel);
            case EnergyBar3DBase.BarType.Transform:
                return TransformRenderer3DBuilder.Create(panel);
            default:
                Debug.LogError("Unknown bar type: " + type);
                return null;
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