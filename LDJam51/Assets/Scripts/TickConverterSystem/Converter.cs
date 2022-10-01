using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TickConverterSystem {

    [System.Serializable]
    public class ConverterEvent : UnityEvent<Converter, string> { }
    public class Converter : BaseTickObject {
        public RecipeData m_assignedRecipe;
        public ResourceStorage m_inputStorage;
        public ResourceStorage m_outputStorage;

        public ConverterEvent m_convertSuccess;
        public ConverterEvent m_convertFail;

        public override void Start () {
            // Init recipe
            if (m_assignedRecipe != null) {
                SetRecipe (m_assignedRecipe);
            };
            base.Start ();
        }
        public void SetRecipe (RecipeData newRecipe) {
            m_assignedRecipe = newRecipe;
            TicksLeft = m_assignedRecipe.m_tickTime;
        }

        [NaughtyAttributes.Button]
        public override void Activate () {
            bool hasEnough = true;
            // Check every ingredient for if there is enough
            foreach (ResourceAmount amount in m_assignedRecipe.m_inputResources) {
                if (m_inputStorage.CountAmountInStacks (amount.resource) < amount.amount) {
                    Debug.LogWarning ("Recipe " + m_assignedRecipe.m_ID + " does not have enough of resource " + amount.resource.m_ID + "(needed: " + amount.amount + ", available: " + m_inputStorage.CountAmountInStacks (amount.resource) + ")");
                    hasEnough = false;
                    break;
                }
            }
            if (!hasEnough) {
                Debug.LogWarning ("Cannot craft recipe " + m_assignedRecipe.m_ID + " because of a lack of resources");
                m_convertFail.Invoke (this, "FailLackingResources");
                return;
            }
            // Ok, we have enough - we'll add it to the output if we have space
            if (m_outputStorage.WillFit (m_assignedRecipe.m_outputResources)) {
                foreach (ResourceAmount amount in m_assignedRecipe.m_outputResources) {
                    m_outputStorage.AddResources (amount);
                }
                foreach (ResourceAmount amount in m_assignedRecipe.m_inputResources) {
                    m_inputStorage.TakeResources (amount);
                }
            } else {
                Debug.LogWarning ("Cannot craft recipe " + m_assignedRecipe.m_ID + " because of a lack of space in Output Storage");
                m_convertFail.Invoke (this, "FailNoOutputSpace");
                return;
            }
            TicksLeft += m_assignedRecipe.m_tickTime;
            m_convertSuccess.Invoke (this, "Success");
            Debug.Log ("Successfully crafted recipe " + m_assignedRecipe.m_ID);
        }

#if UNITY_EDITOR
        void OnDrawGizmos () {
            if (m_inputStorage != null) {
                Vector3 start = transform.position;
                Vector3 end = m_inputStorage.transform.position;
                Color color = Color.blue;
                Gizmos.color = color;
                Gizmos.DrawLine (start, end);
            }
            if (m_outputStorage != null) {
                Vector3 start = transform.position;
                Vector3 end = m_outputStorage.transform.position;
                Color color = Color.green;
                Gizmos.color = color;
                Gizmos.DrawLine (start, end);
            }
        }
#endif
    }
}