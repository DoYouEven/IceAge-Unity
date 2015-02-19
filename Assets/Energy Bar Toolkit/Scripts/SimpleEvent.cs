/*
* Energy Bar Toolkit by Mad Pixel Machine
* http://www.madpixelmachine.com
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace EnergyBarToolkit {

public class SimpleEvent : MonoBehaviour {

    // ===========================================================
    // Constants
    // ===========================================================

    // ===========================================================
    // Fields
    // ===========================================================
    
    public EnergyBar energyBar;
    
    public Target targetType = default(Target);
    public GameObject[] targetGameObjects;
    public string[] targetTags;
    
    public Action onTriggerEnterAction = new Action();
    public Action onTriggerStayAction = new Action();
    public Action onTriggerLeaveAction = new Action();

    // ===========================================================
    // Methods for/from SuperClass/Interfaces
    // ===========================================================

    // ===========================================================
    // Methods
    // ===========================================================
    
    void OnEnable() {
        onTriggerEnterAction.simpleEvent = this;
        onTriggerStayAction.simpleEvent = this;
        onTriggerLeaveAction.simpleEvent = this;
    }

    void OnTriggerEnter(Collider other) {
        OnTrigger(other.gameObject, TriggerType.Enter);
    }
    
    void OnTriggerStay(Collider other) {
        OnTrigger(other.gameObject, TriggerType.Stay);
    }
    
    void OnTriggerLeave(Collider other) {
        OnTrigger(other.gameObject, TriggerType.Leave);
        onTriggerLeaveAction.Reset();
    }

#if !UNITY_4_1 && !UNITY_4_2
    void OnTriggerEnter2D(Collider2D other) {
        OnTrigger(other.gameObject, TriggerType.Enter);
    }

    void OnTriggerStay2D(Collider2D other) {
        OnTrigger(other.gameObject, TriggerType.Stay);
    }

    void OnTriggerLeave2D(Collider2D other) {
        OnTrigger(other.gameObject, TriggerType.Leave);
        onTriggerLeaveAction.Reset();
    }
#endif

    void OnTrigger(GameObject other, TriggerType type) {
        if (!IsReactingTo(other)) {
            return;
        }
        
        switch (type) {
            case TriggerType.Enter:
                onTriggerEnterAction.Invoke(other);
                break;
            case TriggerType.Stay:
                onTriggerStayAction.Invoke(other);
                break;
            case TriggerType.Leave:
                onTriggerLeaveAction.Invoke(other);
                break;
            default:
                Debug.LogError("Unknown option: " + type);
                break;
        }
    }

    bool IsReactingTo(GameObject other) {
        switch (targetType) {
            case Target.GameObjects:
                return Array.Exists(targetGameObjects, (go) => { return go == other;});
            case Target.Tags:
                return Array.Exists(targetTags, (tag) => { return tag == other.tag;});
            
        }
        
        return false;
    }
    
    // ===========================================================
    // Static Methods
    // ===========================================================

    // ===========================================================
    // Inner and Anonymous Classes
    // ===========================================================
    
    [System.Serializable]
    public class Action {
        internal SimpleEvent simpleEvent;
    
        public bool changeBar = false;
        public Type changeBarType = default(Type);
        public float changeBarValue;
        
        /** True if damage should be done in intervals */
        public bool intervaled = false;
        public float timeInterval = 1;
        private float timeIntervalAccum;
        
        public bool sendMessage;
        public Signal[] signals;
        
        public void Invoke(GameObject collider) {
            if (intervaled) {
                timeIntervalAccum += Time.deltaTime;
                while (timeIntervalAccum >= timeInterval) {
                    timeIntervalAccum -= timeInterval;
                    InvokeActual(collider);
                }
            } else {
                InvokeActual(collider);
            }
        }
        
        private void InvokeActual(GameObject collider) {
            EnergyBar energyBar = simpleEvent.energyBar;
        
            if (changeBar) {
                if (energyBar != null) {
                    switch (changeBarType) {
                        case Type.IncreaseByValue:
                            energyBar.valueCurrent += (int) changeBarValue;
                            break;
                        case Type.IncreaseByPercent:
                            energyBar.ValueF += changeBarValue;
                            break;
                        case Type.DecreaseByValue:
                            energyBar.valueCurrent -= (int) changeBarValue;
                            break;
                        case Type.DecreaseByPercent:
                            energyBar.ValueF -= changeBarValue;
                            break;
                        case Type.SetToValue:
                            energyBar.valueCurrent = (int) changeBarValue;
                            break;
                        case Type.SetToPercent:
                            energyBar.ValueF = changeBarValue;
                            break;
                        default:
                            Debug.LogError("Unknown option: " + changeBarType);
                            break;
                    }
                } else {
                    Debug.LogError("Energy bar not set for this event.", simpleEvent);
                }
            }
            
            if (sendMessage) {
                foreach (Signal s in signals) {
                    s.Invoke(simpleEvent, collider);
                }
            }
        }
        
        public void Reset() {
            timeIntervalAccum = 0;
        }
    
        public enum Type {
            IncreaseByValue,
            IncreaseByPercent,
            DecreaseByValue,
            DecreaseByPercent,
            SetToValue,
            SetToPercent,
        }

    }
    
    [System.Serializable]
    public class Signal {
        public ReceiverType receiverType = default(ReceiverType);
        public GameObject receiver;
        public string methodName;
        public MessageArgument argument = default(MessageArgument);
        
        public void Invoke(SimpleEvent simpleEvent, GameObject collider) {
            GameObject receiver;
            switch (receiverType) {
                case ReceiverType.Self:
                    receiver = simpleEvent.gameObject;
                    break;
                case ReceiverType.Collider:
                    receiver = collider;
                    break;
                case ReceiverType.FixedGameObject:
                    receiver = this.receiver;
                    break;
                default:
                    Debug.LogError("Unknown option: " + receiverType);
                    receiver = null;
                    break;
            }
        
            switch (argument) {
                case MessageArgument.Caller:
                    receiver.SendMessage(methodName, simpleEvent.gameObject, SendMessageOptions.RequireReceiver);
                    break;
                case MessageArgument.BarValue:
                    receiver.SendMessage(methodName, simpleEvent.energyBar.valueCurrent, SendMessageOptions.RequireReceiver);
                    break;
                case MessageArgument.BarValuePercent:
                    receiver.SendMessage(methodName, simpleEvent.energyBar.ValueF, SendMessageOptions.RequireReceiver);
                    break;
                default:
                    Debug.LogError("Unknown option: " + argument);
                    break;
            }
        }
        
        public enum ReceiverType {
            Self,
            Collider,
            FixedGameObject
        }
        
        public enum MessageArgument {
            Caller,
            BarValue,
            BarValuePercent,
        }
    }
    
    public enum Target {
        GameObjects,
        Tags,
    }
    
    enum TriggerType {
        Enter,
        Stay,
        Leave,
    }

}

} // namespace
