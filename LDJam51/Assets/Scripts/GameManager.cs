using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[System.Serializable]
public class GameStateEvent : UnityEvent<GameState> { }

public enum GameStates {
    NONE = 0000,
    INIT = 1000,
    LATE_INIT = 1100,
    GAME = 2000,
    NARRATIVE = 3000,
    NARRATIVE_INGAME = 3001,
    LOADING = 4000,
    DEFEAT = 5000,
    WIN = 6000,
    PAUSE = 7000,

}

[System.Serializable]
public class GameState {
    public GameStates state;
    public GameStates nextState;
    public GameStateEvent evtStart;

}

public class GameManager : MonoBehaviour {
    public static GameManager instance;
    public bool initOnStart = true;
    [NaughtyAttributes.ReorderableList]
    public GameState[] gameStates;

    public TypeWriterQueue m_thoughtWriter;
    public InkStringtableManager m_inkStringtableManager;

    public Transform m_normalWorldStart;
    public Transform m_darkWorldStart;

    [SerializeField]
    private GameState currentState;
    public float lateInitWait = 0.1f;
    private Dictionary<GameStates, GameState> gameStateDict = new Dictionary<GameStates, GameState> { };

    void Awake () {
        if (instance == null) {
            instance = this;
        } else {
            Destroy (gameObject);
        }
        foreach (GameState states in gameStates) {
            gameStateDict.Add (states.state, states);
        }
    }
    void Start () {
        if (initOnStart) {
            Invoke ("Init", 1f); // uncomment if not going via mainmenu
        };
        //AudioManager.instance.PlayMusic ("MusicBG");
    }

    [NaughtyAttributes.Button]
    public void Init () {
        SetState (GameStates.INIT);
        //Invoke ("FixTerribleBug", 5f);
        //NextState ();
    }

    void Late_Init () {
        currentState = gameStateDict[GameStates.LATE_INIT];
        Debug.Log ("Invoking late init");
        currentState.evtStart.Invoke (currentState);
        if (currentState.nextState != GameStates.NONE) {
            NextState ();
        }
    }
    public void NextState () {
        if (currentState.nextState != GameStates.NONE) {
            if (gameStateDict[currentState.state].nextState == GameStates.LATE_INIT) { // late init inits a bit late and only works thru nextstate
                Invoke ("Late_Init", lateInitWait);
                // Debug.Log ("Invoking late init");
                return;
            } else {
                Debug.Log ("Invoking Next State " + "(" + gameStateDict[currentState.state].nextState.ToString () + ")");
                SetState (gameStateDict[currentState.state].nextState);
            };
        }
    }
    public void SetState (GameStates state) {
        if (state != GameStates.NONE) {
            GameState = state;
        };
    }
    public GameState GetState (GameStates state) {
        foreach (GameState getState in gameStates) {
            if (getState.state == state) {
                return getState;
            }
        }
        return null;
    }
    public GameStates GameState {
        get {
            if (currentState != null) {
                return currentState.state;
            } else {
                return GameStates.NONE;
            }
        }
        set {
            Debug.Log ("Changing state to " + value);
            currentState = gameStateDict[value];
            currentState.evtStart.Invoke (currentState);
            if (currentState.nextState != GameStates.NONE) {
                NextState ();
            };
        }
    }

    public void WinGame () {
        GameState = GameStates.WIN;
        Debug.Log ("Victory!!");
        SceneManager.LoadScene ("endscene");
    }
    public void Defeat () {

        currentState = gameStateDict[GameStates.DEFEAT];
        currentState.evtStart.Invoke (currentState);
        ActionWaiter (2f, new System.Action (() => SceneManager.LoadScene ("defeatscene")));

    }

    public void Restart () {
        Time.timeScale = 1f;
        SceneManager.LoadScene (SceneManager.GetActiveScene ().name, LoadSceneMode.Single);
    }

    public static void LoadGameScene () {
        SceneManager.LoadScene ("SampleScene");
    }

    public void DualLoadScenes () {
        SceneManager.LoadScene ("ManagersScene", LoadSceneMode.Additive);
        SceneManager.LoadScene ("SA_Demo", LoadSceneMode.Additive);
    }

    [NaughtyAttributes.Button]
    public void BackToMenu () {
        Time.timeScale = 1f;
        SceneManager.LoadScene ("mainmenu");
    }
    public void Quit () {
        Application.Quit ();
    }

    /* [NaughtyAttributes.Button]
    public void SaveGame () {
        Debug.Log ("Saving game");
        SaveManager.instance.IsNewGame = false;
        InkWriter.main.SaveStory ();
        foreach (InventoryController ctrl in InventoryController.allInventories) {
            ctrl.SaveInventory ();
        }
        AudioManager.instance.SaveVolume ();
        SceneController.instance.SaveScene ();
        SaveManager.instance.SaveCache ();
    }

    [NaughtyAttributes.Button]
    public void LoadGame () {
        Debug.Log ("Loading game");
        Restart ();
    }
    public void Pause () {
        GameState oldState = currentState;
        GameState pauseState = gameStateDict[GameStates.PAUSE];
        GameState = GameStates.PAUSE;
        pauseState.evtStart.Invoke (gameStateDict[GameStates.PAUSE]);
        StartCoroutine (PauseWaiter (oldState.state));
        Time.timeScale = 0f;
    }
    public void UnPause () {
        GameState = GameStates.NONE;
        Time.timeScale = 1f;
    }
    IEnumerator PauseWaiter (GameStates continueState) {
        yield return new WaitUntil (() => GameState != GameStates.PAUSE);
        GameState = continueState;
    }
*/


    public void ActionWaiter (float timeToWait, System.Action callBack) {
        StartCoroutine (ActionWaiterCoroutine (timeToWait, callBack));
    }
    IEnumerator ActionWaiterCoroutine (float timeToWait, System.Action callBack) {
        yield return new WaitForSeconds (timeToWait);
        callBack.Invoke ();
    }

    public void DelayActionUntil (System.Func<bool> condition, System.Action callBack) {
        StartCoroutine (DelayActionCoroutine (condition, callBack));
    }
    IEnumerator DelayActionCoroutine (System.Func<bool> condition, System.Action callBack) {

        bool outvar = false;
        while (!outvar) {
            outvar = condition ();
            yield return new WaitForSeconds (0.1f);
        };
        Debug.Log ("Delayed action finished?!");
        callBack.Invoke ();
    }

}