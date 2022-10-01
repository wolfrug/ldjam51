using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TickConverterSystem {

    [System.Serializable]
    public class ResourceAmount {
        public ResourceAmount () { }
        public ResourceAmount (ResourceData resourceToAdd, int amountToAdd) {
            resource = resourceToAdd;
            amount = amountToAdd;
        }
        public ResourceData resource;
        public int amount = 0;
    }

    [System.Serializable]
    public class ResourceStack {

        public ResourceStack () { }

        public ResourceStack (ResourceAmount initialValue) {
            StackResource = initialValue.resource;
            StackAmount = initialValue.amount;
        }
        private ResourceAmount stackAmount = new ResourceAmount ();
        public int StackAmount { // takes into account max stack
            set {
                stackAmount.amount = Mathf.Clamp (value, 0, stackAmount.resource.m_maxStack);
            }
            get {
                return stackAmount.amount;
            }
        }

        public int StackSpaceLeft {
            get {
                return StackResource.m_maxStack - StackAmount;
            }
        }

        [Tooltip ("Returns how much was added")]
        public int AddStack (int amount) { // returns how much was added
            int oldStack = StackAmount;
            StackAmount += amount;
            return StackAmount - oldStack;
        }

        [Tooltip ("Returns how much was removed")]
        public int RemoveStack (int amount) { // returns how much was removed
            int oldStack = StackAmount;
            StackAmount -= amount;
            return oldStack - StackAmount;

        }
        public ResourceData StackResource {
            set {
                stackAmount.resource = value;
                StackAmount += 0; // just to make sure it's not overflowing
            }
            get {
                return stackAmount.resource;
            }
        }
    }

    [CreateAssetMenu (fileName = "Data", menuName = "TickConverter/Resource Data", order = 1)]
    public class ResourceData : ScriptableObject {
        public string m_ID;
        public int m_maxStack = 99;
        public bool m_usesStackSpace = true; // if this resources does not use stack space it...won't?
    }

}