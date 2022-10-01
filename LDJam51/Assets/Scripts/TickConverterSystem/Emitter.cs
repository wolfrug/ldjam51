using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TickConverterSystem {

    [System.Serializable]
    public class EmitEvent : UnityEvent<ResourceStorage> { } // resources emitted
    public class Emitter : BaseTickObject // Simply emits resources every tick, either from a storage or from thin air
    {
        public List<ResourceAmount> m_emittedResources = new List<ResourceAmount> { };
        public int m_tickTime = 5;
        public ResourceStorage m_optionalInputStorage;
        public ResourceStorage m_emitStorage;

        public EmitEvent m_emitSuccess;
        public EmitEvent m_emitFailure;

        public override void Activate () {
            if (m_optionalInputStorage != null) {
                bool hasEnough = true;
                // Check every ingredient for if there is enough
                foreach (ResourceAmount amount in m_emittedResources) {
                    if (m_optionalInputStorage.CountAmountInStacks (amount.resource) < amount.amount) {
                        Debug.LogWarning ("Emitter " + m_name + " does not have enough of resource " + amount.resource.m_ID + "(needed: " + amount.amount + ", available: " + m_optionalInputStorage.CountAmountInStacks (amount.resource) + ")");
                        hasEnough = false;
                        break;
                    }
                }
                if (!hasEnough) {
                    Debug.LogWarning ("Emitter " + m_name + " failed because of a lack of resources");
                    m_emitFailure.Invoke (null);
                    TicksLeft += m_tickTime;
                    return;
                }
            }
            // Ok, we have enough - we'll add it to the output if we have space
            if (m_emitStorage.WillFit (m_emittedResources)) {
                foreach (ResourceAmount amount in m_emittedResources) {
                    m_emitStorage.AddResources (amount);
                }
                if (m_optionalInputStorage != null) {
                    foreach (ResourceAmount amount in m_emittedResources) {
                        m_optionalInputStorage.TakeResources (amount);
                    }
                };
            } else {
                Debug.LogWarning ("Cannot emit " + m_name + " because of a lack of space in Emitter Storage");
                m_emitFailure.Invoke (m_emitStorage);
                TicksLeft += m_tickTime;
                return;
            }

            TicksLeft += m_tickTime;
            m_emitSuccess.Invoke (m_emitStorage);
            Debug.Log ("Successfully emitted (" + m_name + ")");
        }
#if UNITY_EDITOR
        void OnDrawGizmos () {
            if (m_optionalInputStorage != null) {
                Vector3 start = transform.position;
                Vector3 end = m_optionalInputStorage.transform.position;
                Color color = Color.blue;
                Gizmos.color = color;
                Gizmos.DrawLine (start, end);
            }
            if (m_emitStorage != null) {
                Vector3 start = transform.position;
                Vector3 end = m_emitStorage.transform.position;
                Color color = Color.green;
                Gizmos.color = color;
                Gizmos.DrawLine (start, end);
            }
        }
#endif
    }
}