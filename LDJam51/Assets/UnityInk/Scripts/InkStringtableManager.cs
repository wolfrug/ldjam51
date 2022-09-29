using System.Collections;
using System.Collections.Generic;
using Ink.Runtime;
using TMPro;
using UnityEngine;

/*
Manages using Ink to retrieve strings for whatever reason. Will go to the knot, continue until it cannot any more, then return the string.
Cannot be used as an ersatz writer, will not deal with options, but otherwise should retrieve the text accurately.
*/

public class InkStringtableManager : MonoBehaviour {

    [Tooltip ("If set to not '', waits until it can then sets the string to the default knot")]
    public string m_startingKnot;
    public string m_lineBreakCharacter = "\n";

    [Tooltip ("Trims extra linebreaks etc from the string, especially useful for single-line things")]
    public bool m_trimStrings = true;
    [Tooltip ("Not obligatory, but can be used for ease of use.")]
    public TextMeshProUGUI m_textObject;
    public TypeWriterQueue m_typeWriterQueue;
    public TagListener m_tagListener;
    public List<Choice> m_endChoices = new List<Choice> { };
    public Dictionary<int, List<string>> m_gatheredTags = new Dictionary<int, List<string>> { };
    // Start is called before the first frame update
    void Start () {
        if (m_textObject == null) {
            m_textObject = GetComponentInChildren<TextMeshProUGUI> ();
        }

        if (m_startingKnot != "" && m_textObject != null) {
            StartCoroutine (SelfInitializeWaiter ());
        }
    }

    IEnumerator SelfInitializeWaiter () {
        yield return new WaitUntil (() => InkWriter.main != null);
        yield return new WaitUntil (() => InkWriter.main.story != null);
        SetText (m_startingKnot);
    }

    public string GetKnot (string targetKnot) {
        string returnText = "";
        if (GameManager.instance.GameState != GameStates.NARRATIVE) {
            InkWriter.main.story.ChoosePathString (targetKnot);
            while (InkWriter.main.story.canContinue) {
                returnText += InkWriter.main.story.Continue ();
                returnText += m_lineBreakCharacter;
            }
        } else {
            Debug.LogWarning ("Tried to get stringtable knot " + targetKnot + " during narrative - cancelled!", gameObject);
        }
        return returnText;
    }
    public void SetText (string targetKnot) {
        if (GameManager.instance.GameState != GameStates.NARRATIVE) {
            if (m_textObject != null) {
                m_textObject.SetText (GetKnot (targetKnot));
            } else {
                Debug.LogWarning ("Attempted to set Ink stringtable text to knot " + targetKnot + "but no text object was assigned!", gameObject);
            }
        } else {
            Debug.LogWarning ("Tried to set stringtable text to knot " + targetKnot + " during narrative - cancelled!", gameObject);
        }
    }

    // Creates a string array of all the strings in a specific knot
    public string[] CreateStringArrayKnot (string targetKnot, List<Choice> gatherChoices) {
        if (GameManager.instance.GameState == GameStates.NARRATIVE) {
            Debug.LogWarning ("Tried to get stringtable knot " + targetKnot + " during narrative - cancelled!", gameObject);
            return null;
        }
        InkWriter.main.story.ChoosePathString (targetKnot);
        string[] returnArray = CreateStringArray ();
        if (gatherChoices != null) {
            gatherChoices.Clear ();
            foreach (Choice choice in InkWriter.main.story.currentChoices) {
                gatherChoices.Add (choice);
                Debug.Log ("Added end choice with name: " + choice.text);
            }
        }
        return returnArray;
    }

    // Creates a list of strings starting from a choice, and then gathers the choices
    public string[] CreateStringArrayChoice (Choice startChoice, List<Choice> gatherChoices) {
        if (GameManager.instance.GameState == GameStates.NARRATIVE) {
            Debug.LogWarning ("Tried to get stringtable choice text " + startChoice + " during narrative - cancelled!", gameObject);
            return null;
        }
        if (InkWriter.main.story.currentChoices.Contains (startChoice)) {
            InkWriter.main.story.ChooseChoiceIndex (startChoice.index);
        } else {
            Debug.LogWarning ("Tried to choose a choice that is no longer among the Inkwriter's available choices!");
            InkWriter.main.story.ChoosePath (startChoice.targetPath);
        }
        string[] returnArray = CreateStringArray ();
        if (gatherChoices != null) {
            gatherChoices.Clear ();
            foreach (Choice choice in InkWriter.main.story.currentChoices) {
                gatherChoices.Add (choice);
                Debug.Log ("Added end choice with name: " + choice.text);
            }
        }
        return returnArray;
    }

    string[] CreateStringArray () {
        string returnText = "";
        List<string> returnArray = new List<string> { };
        int currentIndex = 0;
        m_gatheredTags.Clear ();
        while (InkWriter.main.story.canContinue) {
            returnText = InkWriter.main.story.Continue ();
            returnArray.Add (returnText);
            // Add current tags, if any, to the dictionary
            if (InkWriter.main.story.currentTags.Count > 0) {
                m_gatheredTags.Add (currentIndex, new List<string> (InkWriter.main.story.currentTags));
            }
            currentIndex++;
        }
        return returnArray.ToArray ();
    }

    public void PlayWriterQueueFromKnot (string targetKnot) {
        // First we create a list of strings from the knot
        string[] knotStrings = CreateStringArrayKnot (targetKnot, m_endChoices);
        // Then we set it to play on the typewriter
        if (knotStrings.Length > 0) {
            PlayWriterQueue (knotStrings);
        } else {
            Debug.LogWarning ("Could not play writer queue from knot - no strings found! (" + targetKnot + ")");
        }
    }
    public void PlayWriterQueueFromChoice (Choice targetChoice) {
        // First we create a list of strings from the knot
        string[] knotStrings = CreateStringArrayChoice (targetChoice, m_endChoices);
        // Then we set it to play on the typewriter
        if (knotStrings.Length > 0) {
            PlayWriterQueue (knotStrings);
        } else {
            Debug.LogWarning ("Could not play writer queue from choice - no strings found! (" + targetChoice.text + ")");
        }
    }
    public void PlayWriterQueue (string[] targetStrings) {
        WriterAction[] newQueue = TypeWriterQueue.CreateTypeWriterQueue (targetStrings);
        m_typeWriterQueue.SetQueue (newQueue);
        m_typeWriterQueue.StartQueue (0);
        Debug.Log ("Starting new writer queue of length " + targetStrings.Length + " with contents starting with " + targetStrings[0]);
        // Invoke the tags as necessary
        if (m_gatheredTags.Count > 0 && m_tagListener != null) {
            for (int i = 0; i < newQueue.Length; i++) {
                if (m_gatheredTags.ContainsKey (i)) {
                    foreach (string tag in m_gatheredTags[i]) {
                        newQueue[i].startedEvent.AddListener ((x) => m_tagListener.TagListenerFunction (tag));
                    };
                };
            }
        }
    }

}