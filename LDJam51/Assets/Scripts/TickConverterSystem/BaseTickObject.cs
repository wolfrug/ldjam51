using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TickConverterSystem {
    public class BaseTickObject : MonoBehaviour {
        public string m_name;
        [SerializeField]
        private int m_ticksLeft = 0;
        [SerializeField]
        private bool m_active = true;

        public virtual void Start () {
            // Init tick waiter
            if (TickManager.instance != null) {
                TickManager.instance.m_tickEvent.AddListener (ProcessTick);
            }
        }

        [NaughtyAttributes.Button]
        public virtual void ProcessTick (int tickAmount = 1) { // attempts to count down ticks for current recipe, or then convert if possible
            if (m_active) {
                if (TicksLeft > 0) {
                    TicksLeft -= tickAmount;
                }
                if (TicksLeft <= 0) {
                    Activate ();
                }
            }
        }

        public virtual void Activate () {

        }
        public virtual int TicksLeft {
            get {
                return m_ticksLeft;
            }
            set {
                m_ticksLeft = value;
            }
        }
        public bool Active {
            get;
            set;
        }
    }
}