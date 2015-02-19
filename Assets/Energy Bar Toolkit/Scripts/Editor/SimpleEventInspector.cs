/*
* Energy Bar Toolkit by Mad Pixel Machine
* http://www.madpixelmachine.com
*/

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace EnergyBarToolkit {

[CustomEditor(typeof(SimpleEvent))]
public class SimpleEventInspector : EditorBase {

    // ===========================================================
    // Constants
    // ===========================================================

    // ===========================================================
    // Fields
    // ===========================================================
    
    SerializedProperty energyBar;
    SerializedProperty targetType;
    SerializedProperty targetGameObjects;
    SerializedProperty targetTags;
    SerializedProperty onTriggerEnterAction;
    SerializedProperty onTriggerStayAction;
    SerializedProperty onTriggerLeaveAction;

    // ===========================================================
    // Methods for/from SuperClass/Interfaces
    // ===========================================================

    // ===========================================================
    // Methods
    // ===========================================================
    
    void OnEnable() {
        energyBar = serializedObject.FindProperty("energyBar");
        targetType = serializedObject.FindProperty("targetType");
        targetGameObjects = serializedObject.FindProperty("targetGameObjects");
        targetTags = serializedObject.FindProperty("targetTags");
        onTriggerEnterAction = serializedObject.FindProperty("onTriggerEnterAction");
        onTriggerStayAction = serializedObject.FindProperty("onTriggerStayAction");
        onTriggerLeaveAction = serializedObject.FindProperty("onTriggerLeaveAction");
    }
    
    public override void OnInspectorGUI() {
        serializedObject.Update();
        
        var t = target as SimpleEvent;
        var collider = t.GetComponent<Collider>();

#if !UNITY_4_1 && !UNITY_4_2
        var collider2d = t.GetComponent<Collider2D>();
#endif

        if (collider != null
#if !UNITY_4_1 && !UNITY_4_2
            || collider2d != null
#endif
            ) {

            if (collider != null) {
                if (!collider.isTrigger) {
                    if (MadGUI.ErrorFix("This game object collider must be marked as 'Trigger'. Change it?")) {
                        collider.isTrigger = true;
                    }
                }
            }
#if !UNITY_4_1 && !UNITY_4_2
            else {
                if (!collider2d.isTrigger) {
                    if (MadGUI.ErrorFix("This game object collider must be marked as 'Trigger'. Change it?")) {
                        collider2d.isTrigger = true;
                    }
                }
            }
#endif
        } else {
            if (MadGUI.ErrorFix("This game object doesn't have a collider. Attach it now?")) {
#if !UNITY_4_1 && !UNITY_4_2
                if (EditorUtility.DisplayDialog("Choose Collider Type", "Which collider type do you want to create? 2D or 3D?", "2D", "3D")) {
                    var c = t.gameObject.AddComponent<BoxCollider2D>();
                    c.isTrigger = true;
                } else {
#endif
                    var c = t.gameObject.AddComponent<BoxCollider>();
                    c.isTrigger = true;

#if !UNITY_4_1 && !UNITY_4_2
                }
#endif
            }
        }
    
        MadGUI.PropertyField(energyBar, "Energy Bar");
        MadGUI.PropertyField(targetType, "Trigger With");
        
        MadGUI.Indent(() => {
            switch ((SimpleEvent.Target) targetType.enumValueIndex) {
                case SimpleEvent.Target.GameObjects:
                    GUITargetGameObjects();
                    break;
                case SimpleEvent.Target.Tags:
                    GUITargetTags();
                    break;
                default:
                    Debug.LogError("Unknown option: " + targetType.enumValueIndex);
                    break;
            }
        });
        
        if (MadGUI.Foldout("On Trigger Enter", false)) {
            MadGUI.Indent(() => {
                GUIAction(onTriggerEnterAction, false);
            });
        }
        
        if (MadGUI.Foldout("On Trigger Stay", false)) {
            MadGUI.Indent(() => {
                GUIAction(onTriggerStayAction, true);
            });
        }
        
        if (MadGUI.Foldout("On Trigger Leave", false)) {
            MadGUI.Indent(() => {
                GUIAction(onTriggerLeaveAction, false);
            });
        }
        
        serializedObject.ApplyModifiedProperties();
    }

    void GUITargetGameObjects() {
        ArrayList(targetGameObjects, "Game Object List");
    }
    
    void GUITargetTags() {
        ArrayList(targetTags, "Tag List", (prop) => {
            prop.stringValue = EditorGUILayout.TagField(prop.stringValue);
        });
    }
    
    void GUIAction(SerializedProperty prop, bool withInterval) {
        var changeBar = prop.FindPropertyRelative("changeBar");
        var changeBarType = prop.FindPropertyRelative("changeBarType");
        var changeBarValue = prop.FindPropertyRelative("changeBarValue");
        var sendMessage = prop.FindPropertyRelative("sendMessage");
        var intervaled = prop.FindPropertyRelative("intervaled");
        var timeInterval = prop.FindPropertyRelative("timeInterval");
        var signals = prop.FindPropertyRelative("signals");
        
        MadGUI.PropertyFieldToggleGroup(changeBar, "Change Bar", () => {
            MadGUI.Indent(() => {
                MadGUI.PropertyField(changeBarType, "Change Method");
                MadGUI.Indent(() => {
                    string example;
                    switch ((SimpleEvent.Action.Type) changeBarType.enumValueIndex) {
                        case SimpleEvent.Action.Type.DecreaseByPercent:
                        case SimpleEvent.Action.Type.IncreaseByPercent:
                        case SimpleEvent.Action.Type.SetToPercent:
                            example = " (0.0 - 1.0)";
                            break;
                        default:
                            example = "";
                            break;
                    }
                    MadGUI.PropertyField(changeBarValue, "Value" + example);
                    
                    intervaled.boolValue = withInterval;
                    if (withInterval) {
                        MadGUI.PropertyField(timeInterval, "Time Interval");
                    }
                });
            });
        });
        
        MadGUI.PropertyFieldToggleGroup(sendMessage, "Send Message", () => {
            MadGUI.Indent(() => {
                ArrayList(signals, "Signals", (signalProp) => {
                    GUISignal(signalProp);
                });
            });
        });
    }
    
    void GUISignal(SerializedProperty prop) {
        var receiverType = prop.FindPropertyRelative("receiverType");
        var receiver = prop.FindPropertyRelative("receiver");
        var methodName = prop.FindPropertyRelative("methodName");
        var argument = prop.FindPropertyRelative("argument");
        
        MadGUI.PropertyField(receiverType, "Receiver Type");
        if (receiverType.enumValueIndex == (int) SimpleEvent.Signal.ReceiverType.FixedGameObject) {
            MadGUI.Indent(() => {
                MadGUI.PropertyField(receiver, "Receiver Object");
            });
        }
        MadGUI.PropertyField(methodName, "Method Name");
        MadGUI.PropertyField(argument, "Method Argument");
        
        string argumentStr;
        switch ((SimpleEvent.Signal.MessageArgument) argument.intValue) {
            case SimpleEvent.Signal.MessageArgument.BarValue:
                argumentStr = "int value";
                break;
            case SimpleEvent.Signal.MessageArgument.BarValuePercent:
                argumentStr = "float valuePercent";
                break;
            case SimpleEvent.Signal.MessageArgument.Caller:
                argumentStr = "GameObject caller";
                break;
            default:
                Debug.LogError("Unknown option: " + argument.intValue);
                argumentStr = "-error-";
                break;
        }
        
        MadGUI.Info("Receiver: OnEvent(" + argumentStr + ")");
    }

    // ===========================================================
    // Static Methods
    // ===========================================================

    // ===========================================================
    // Inner and Anonymous Classes
    // ===========================================================

}

} // namespace
