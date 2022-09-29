using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class DialogSnippet {
    public InkStringtableManager talker;
    public string inkknot;
    private int index = -1;
    public int Index {
        set {
            index = value;
        }
        get {
            return index;
        }
    }
}

[System.Serializable]
public class DialogFinished : UnityEvent<InkStringtableDialog> { }

public class InkStringtableDialog : MonoBehaviour {
    [NaughtyAttributes.ReorderableList]
    public List<DialogSnippet> m_orderedDialog = new List<DialogSnippet> { };
    public bool m_running = false;
    public int m_currentIndex = -1;

    public DialogFinished m_dialogFinishedEvent;
    // Start is called before the first frame update
    void Start () {
        int indexStart = 0;
        List<TypeWriterQueue> addedListeners = new List<TypeWriterQueue> { };
        // Add all the listeners pre-emptively.
        foreach (DialogSnippet snippet in m_orderedDialog) {
            if (!addedListeners.Contains (snippet.talker.m_typeWriterQueue)) {
                snippet.talker.m_typeWriterQueue.m_queueEndedEvent.AddListener ((x) => ContinueDialog (snippet));
                addedListeners.Add (snippet.talker.m_typeWriterQueue);
            };
            Debug.Log ("Added dialogsnippet for knot " + snippet.inkknot + " at index " + indexStart);
            snippet.Index = indexStart;
            indexStart++;
        }
    }

    public void StartDialog (int index = 0) {
        if (m_running) {
            Debug.LogWarning ("Cannot interrupt or start a new stringtable dialog!", gameObject);
            return;
        }
        if (m_orderedDialog.Count < 1) {
            Debug.LogWarning ("No dialog snippets assigned!", gameObject);
            return;
        }
        m_running = true;
        m_orderedDialog[index].talker.PlayWriterQueueFromKnot (m_orderedDialog[index].inkknot);
        m_currentIndex = index;

    }
    void ContinueDialog (DialogSnippet writer) {
        if (m_running) {
            // So any one of the typewriters has just finished...
            if (m_currentIndex + 1 >= m_orderedDialog.Count) {
                // Finished!
                EndDialog ();
                return;
            }
            m_currentIndex++;
            Debug.Log ("Finished writing, now going to index " + m_currentIndex);
            if (m_currentIndex < m_orderedDialog.Count) {
                m_orderedDialog[m_currentIndex].talker.PlayWriterQueueFromKnot (m_orderedDialog[m_currentIndex].inkknot);
            };
        };
    }

    public void EndDialog () {
        m_running = false;
        m_currentIndex = -1;
        m_dialogFinishedEvent.Invoke (this);
    }
}