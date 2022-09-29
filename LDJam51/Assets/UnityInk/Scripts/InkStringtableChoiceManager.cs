using System.Collections;
using System.Collections.Generic;
using Ink.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InkStringtableChoiceManager : MonoBehaviour {
    // Very ugly way of doing this lolol

    public bool m_active = true;

    // I.e. if the thread has moved on
    public bool m_allowCurrentlyNonActiveChoices = true;
    public InkStringtableManager m_inkStringtableManager;
    public RectTransform m_choiceParent;
    public GameObject m_ButtonPrefab;
    // Start is called before the first frame update
    void Awake () {
        // Listen to the end of the manager's typewriter queueu
        if (m_inkStringtableManager.m_typeWriterQueue != null) {
            m_inkStringtableManager.m_typeWriterQueue.m_queueEndedEvent.AddListener (OnQueueEnded);
        }
    }

    void OnQueueEnded (TypeWriter queue) {
        if (m_active) {
            // Check if there are choices
            if (m_inkStringtableManager.m_endChoices.Count > 0) {
                ClearButtons ();
                CreateButtons ();
            }
        };
    }

    public void ClearButtons () {
        // DESTROY CHILDREN
        foreach (Transform child in m_choiceParent) {
            Destroy (child.gameObject);
        }
    }
    public void CreateButtons () {
        foreach (Choice choice in m_inkStringtableManager.m_endChoices) {
            Button newButton = NewButton ();
            newButton.GetComponentInChildren<TextMeshProUGUI> ().SetText (choice.text);
            newButton.onClick.AddListener (() => OnClickedButton (choice));
        }
    }

    Button NewButton () {
        GameObject newButton = Instantiate (m_ButtonPrefab, m_choiceParent);
        return newButton.GetComponentInChildren<Button> ();
    }

    void OnClickedButton (Choice choice) {
        if (m_active) {
            if (InkWriter.main.story.currentChoices.Contains (choice) || m_allowCurrentlyNonActiveChoices) {
                m_inkStringtableManager.PlayWriterQueueFromChoice (choice);
            };
            ClearButtons ();
        };
    }
}