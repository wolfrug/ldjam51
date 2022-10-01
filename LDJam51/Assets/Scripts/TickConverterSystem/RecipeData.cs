using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TickConverterSystem {

  

    [CreateAssetMenu (fileName = "Data", menuName = "TickConverter/Recipe Data", order = 1)]
    public class RecipeData : ScriptableObject {
        public string m_ID;
        public List<ResourceAmount> m_inputResources = new List<ResourceAmount> { };
        public List<ResourceAmount> m_outputResources = new List<ResourceAmount> { };
        public int m_tickTime;

    }
}