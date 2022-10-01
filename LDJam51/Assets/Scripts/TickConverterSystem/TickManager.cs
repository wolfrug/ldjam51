using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TickConverterSystem {
    [System.Serializable]
    public class TickEvent : UnityEvent<int> { } // how many ticks counted?
    public class TickManager : MonoBehaviour {
        public static TickManager instance;

        public float m_tickSpeedUpdateDefault = 1f; // if we use real-time tick speeds
        public float m_tickSpeedUpdate = 1f;

        public TickEvent m_tickEvent;

        [SerializeField]
        private float m_timeUntilNextTickUpdate = 1f;

        void Awake () {
            if (instance == null) {
                instance = this;
                InitTickManager ();
            } else {
                Destroy (gameObject);
            }
        }

        void InitTickManager () {
            m_tickSpeedUpdate = m_tickSpeedUpdateDefault;
            m_timeUntilNextTickUpdate = 1f;

        }

        public void Tick (int tickAmount = 1) {
            m_tickEvent.Invoke (tickAmount);
        }

        void Update () {
            if (m_timeUntilNextTickUpdate <= 0f) {
                Tick ();
                m_timeUntilNextTickUpdate = 1f;
            } else {
                m_timeUntilNextTickUpdate -= Time.deltaTime * m_tickSpeedUpdate;
            }
        }
    }

}