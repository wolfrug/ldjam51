using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TickConverterSystem {
    public class ResourceStorage : MonoBehaviour {
        public string m_name = "Storage";
        public List<ResourceAmount> m_visualizedContents = new List<ResourceAmount> { }; // only for visuals
        private List<ResourceStack> m_stackContent = new List<ResourceStack> { };
        public int m_maxStacks = 64;
        public int m_stacksLeft = 64;

        [NaughtyAttributes.Button]
        public void ClearContents () {
            m_stackContent.Clear ();
            VisualizeContent ();
        }

        [NaughtyAttributes.Button]
        public void InitContents () { // inits the contents as if the initial values were added manually - this is just for show
            foreach (ResourceAmount addAmount in m_visualizedContents) {
                AddResources (addAmount.resource, addAmount.amount);
            }
            VisualizeContent ();
        }

        [NaughtyAttributes.Button]
        public void VisualizeContent () { // Visualizes the actual content
            m_visualizedContents.Clear ();
            foreach (ResourceStack stack in m_stackContent) {
                ResourceAmount visualizedResource = m_visualizedContents.Find ((x) => x.resource == stack.StackResource);
                if (visualizedResource != null) {
                    visualizedResource.amount = CountAmountInStacks (stack.StackResource);
                } else {
                    m_visualizedContents.Add (new ResourceAmount (stack.StackResource, CountAmountInStacks (stack.StackResource)));
                }
            }

            m_stacksLeft = StacksLeft;
        }

        public int StacksLeft {
            get {
                return m_maxStacks - m_stackContent.FindAll ((x) => x.StackResource.m_usesStackSpace).Count;
            }
        }
        public List<ResourceStack> Content {
            get {
                return m_stackContent;
            }
            private set {
                m_stackContent = value;
            }
        }
        public bool WillFit (ResourceAmount amountToAdd) { // returns true if it will fit, false if it will not
            if (!amountToAdd.resource.m_usesStackSpace) {
                return true;
            }
            int spaceAvailableInCurrentStacks = 0;
            foreach (ResourceStack stack in m_stackContent.FindAll ((x) => x.StackResource == amountToAdd.resource)) {
                if (stack.StackAmount < stack.StackResource.m_maxStack) {
                    spaceAvailableInCurrentStacks += stack.StackSpaceLeft;
                }
            }
            if (spaceAvailableInCurrentStacks >= amountToAdd.amount) {
                return true;
            }
            // not enough space in available stacks - can we add more, and how many do we need?
            int amountLeft = amountToAdd.amount - spaceAvailableInCurrentStacks;

            int stacksNeeded = Mathf.CeilToInt (amountLeft / amountToAdd.resource.m_maxStack);

            if (stacksNeeded <= StacksLeft) { // We can spawn enough stacks!
                return true;
            } else {
                return false;
            }
        }
        public bool WillFit (List<ResourceAmount> amountToAddList) { // will -all- of these resources fit?
            int totalStacksNeeded = 0;

            foreach (ResourceAmount amountToAdd in amountToAddList) {
                int spaceAvailableInCurrentStacks = 0;
                foreach (ResourceStack stack in m_stackContent.FindAll ((x) => x.StackResource == amountToAdd.resource)) {
                    if (stack.StackAmount < stack.StackResource.m_maxStack) {
                        spaceAvailableInCurrentStacks += stack.StackSpaceLeft;
                    }
                }
                if (spaceAvailableInCurrentStacks >= amountToAdd.amount) { // no need to add more stacks - all good
                    break;
                }
                // not enough space in available stacks - can we add more, and how many do we need?
                int amountLeft = amountToAdd.amount - spaceAvailableInCurrentStacks;

                int stacksNeeded = Mathf.CeilToInt (amountLeft / amountToAdd.resource.m_maxStack);
                if (!amountToAdd.resource.m_usesStackSpace) { // If they don't use stack space we don't need to count 'em
                    stacksNeeded = 0;
                }

                totalStacksNeeded += stacksNeeded;
            }
            if (totalStacksNeeded <= StacksLeft) {
                return true;
            } else {
                return false;
            }
        }
        public ResourceStack GetFirstEmptyStack (ResourceData input) {
            foreach (ResourceStack stack in m_stackContent.FindAll ((x) => x.StackResource == input)) {
                if (stack.StackAmount < stack.StackResource.m_maxStack) {
                    return stack;
                }
            }
            return null;
        }
        public ResourceStack GetFirstEmptyOrFullStack (ResourceData input) {
            ResourceStack emptyStack = GetFirstEmptyStack (input);
            if (emptyStack != null) {
                return emptyStack;
            } else {
                ResourceStack fullStack = m_stackContent.Find ((x) => x.StackResource == input);
                return fullStack; // might be null
            }
        }
        int CreateStack (ResourceAmount newResource) { // returns how much was successfully added
            ResourceStack newStack = new ResourceStack ();
            newStack.StackResource = newResource.resource;
            newStack.AddStack (newResource.amount);
            m_stackContent.Add (newStack);
            Debug.Log ("Created a new stack of " + newResource.resource.m_ID + " containing " + newStack.StackAmount);
            return newStack.StackAmount;
        }
        void RemoveAllStacks (ResourceData resource) {
            m_stackContent.RemoveAll ((x) => x.StackResource == resource);
        }
        void RemoveStack (ResourceStack stack) {
            if (m_stackContent.Contains (stack)) {
                m_stackContent.Remove (stack);
            }
        }

        public int CountAmountInStacks (ResourceData data) {
            int returnValue = 0;
            foreach (ResourceStack stack in m_stackContent.FindAll ((x) => x.StackResource == data)) {
                returnValue += stack.StackAmount;
            }
            Debug.Log ("Counting amount of " + data.m_ID + " in stacks (" + returnValue + ")");
            return returnValue;
        }

        public int TakeResources (ResourceAmount resourceTake, bool forceAdd = false) {
            return TakeResources (resourceTake.resource, resourceTake.amount, forceAdd);
        }
        public int TakeResources (ResourceData resource, int amount, bool forceTake = false) { // return how many were -not- removed
            if (CountAmountInStacks (resource) < amount && !forceTake) {
                return 0;
            }
            if (CountAmountInStacks (resource) == 0) {
                return 0;
            }
            while (amount > 0 && CountAmountInStacks (resource) > 0) {
                ResourceStack stack = GetFirstEmptyOrFullStack (resource);
                amount -= stack.RemoveStack (amount);
                if (stack.StackAmount == 0) {
                    RemoveStack (stack);
                }
            }
            return amount;
        }
        public int AddResources (ResourceAmount resourceAdd, bool forceAdd = false) {
            return AddResources (resourceAdd.resource, resourceAdd.amount, forceAdd);
        }
        public int AddResources (ResourceData resourceAdd, int amountAdd, bool forceAdd = false) { // returns how many were -not- added
            int initialAmount = CountAmountInStacks (resourceAdd);
            int amountLeft = amountAdd;
            while (amountLeft > 0) {
                if (!resourceAdd.m_usesStackSpace) { // things that don't use stack space can only have one stack
                    ResourceStack singleStack = GetFirstEmptyOrFullStack (resourceAdd);
                    if (singleStack != null) { // we try to add to it if there is a stack, full or empty
                        amountLeft -= singleStack.AddStack (amountLeft);
                    } else { // otherwise we make one
                        amountLeft -= CreateStack (new ResourceAmount (resourceAdd, amountLeft));
                    }
                    break;
                }
                if (StacksLeft == 0 && !forceAdd) {
                    break;
                }
                ResourceStack stack = GetFirstEmptyStack (resourceAdd);
                if (stack != null) {
                    amountLeft -= stack.AddStack (amountLeft);
                } else {
                    amountLeft -= CreateStack (new ResourceAmount (resourceAdd, amountLeft));
                }

            }
            Debug.Log ("Added " + (amountAdd - amountLeft) + " of " + resourceAdd.m_ID);
            return amountLeft;
        }
    }
}