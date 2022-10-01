using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TickConverterSystem {

    [System.Serializable]
    public class TransportedResource {

        public TransportedResource () { }
        public TransportedResource (ResourceData resourceData, int amount) {
            resource = new ResourceAmount (resourceData, amount);
        }
        public ResourceAmount resource;
        public float percentageTravelled = 0f;
        public float distanceTravelled = 0f;
    }
    public class Transporter : BaseTickObject {

        public ResourceStorage m_startPoint;
        public ResourceStorage m_endPoint;
        public List<TransportedResource> m_transportedResources = new List<TransportedResource> { };
        public float m_transportDistance;
        public float m_transportPerTick = 1f;
        public int m_transportAmountMax = 1;

        [Tooltip ("Leave the list empty to transport everything")]
        public List<ResourceData> m_transportables = new List<ResourceData> { };

        public override void Start () {
            TransportDistance = Vector3.Distance (m_startPoint.transform.position, m_endPoint.transform.position);
            base.Start ();
        }

        public float TransportDistance {
            get {
                return m_transportDistance;
            }
            set {
                m_transportDistance = value;
            }
        }

        public override void Activate () {
            if (m_startPoint.Content.Count == 0) {
                // Only transport I guess
            }

            foreach (TransportedResource trR in m_transportedResources) {
                trR.distanceTravelled += m_transportPerTick;
                trR.percentageTravelled = m_transportDistance / trR.distanceTravelled;
                if (trR.percentageTravelled >= 1f){
                    
                }
            }

            // Take max transport amount, or as much as we can, from start
            ResourceStack takeStack = null;
            if (m_transportables.Count < 1) { // take anything
                takeStack = m_startPoint.Content[0];
            } else { // try to find one that matches
                foreach (ResourceStack stack in m_startPoint.Content) {
                    foreach (ResourceData data in m_transportables) {
                        if (stack.StackResource == data) {
                            takeStack = stack;
                        }
                    }
                }
            }
            if (takeStack != null) {
                int takeAmountFailed = m_startPoint.TakeResources (takeStack.StackResource, m_transportAmountMax);
                int actualAmount = m_transportAmountMax - takeAmountFailed;
                TransportedResource newTransportedResource = new TransportedResource (takeStack.StackResource, actualAmount);
                m_transportedResources.Add (newTransportedResource);
            }

        }

    }
}