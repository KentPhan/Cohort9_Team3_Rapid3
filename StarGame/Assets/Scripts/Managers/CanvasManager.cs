﻿using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CanvasManager : MonoBehaviour
{
    private static CanvasManager _instance = null;

    public static CanvasManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new CanvasManager();
            }
            return _instance;
        }
    }



    private Vector3 offset;

    [Header("Buttons")]
    public Button startGameButton;
    public Button openConsellationMenuButton;
    public Button closeConstellationMenuButton;
    public Button resetGameButton;

    [Header("Panels")]
    public GameObject startPanel;
    public GameObject freeRoamPanel;
    public GameObject collectionMenuPanel;
    public GameObject lookUpPanel;
    public float lookUpBlinkDelay;
    [Range(0.0f, 1.0f)]
    public float lookUpBlinkPortionUnvisible;
    private float _currentLookUpBlinkTime = 0.0f;

    [Header("External References")]
    public GameObject constellationMatchScreenPanel; // Very different. A sphere that sits outside in world space and rotates to display constellations


    [Header("Placeholder Images")]
    public Sprite UIUnknownConstellationTitle;
    public Sprite UIConstellationPurpleBorder;
    public Sprite UIConstellationBlueBorder;


    [Header("Matching Constellations")]
    public Constellation ConstellationMatch;
    private bool isAnimationOfConstellationMatch;
    private int constellationMatchItemId = -1;
    private float matchTime = 0.5f;

    private CanvasManager()
    {

    }

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else if (_instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    // Use this for initialization
    void Start()
    {
        startPanel.SetActive(true);
        collectionMenuPanel.SetActive(false);
        lookUpPanel.SetActive(false);
        //constellationsPanel.canvasManager = this;

        // Event listeners
        startGameButton.onClick.AddListener(() =>
        {
            GameManager.Instance.GoToFreeRoam();
            MusicManager.Instance.PlaySoundEffect("button_click");
        });
        openConsellationMenuButton.onClick.AddListener(() =>
        {
            GameManager.Instance.GoToCollectionLog();
            MusicManager.Instance.PlaySoundEffect("button_click");
        });
        closeConstellationMenuButton.onClick.AddListener(() =>
        {
            GameManager.Instance.GoToFreeRoam();
            MusicManager.Instance.PlaySoundEffect("button_click");
        });
        resetGameButton.onClick.AddListener(() =>
        {
            // The most hacky ass way to reset the game
            GameManager.Instance.GoToStart();
            Destroy(CanvasManager.Instance.gameObject);
            Destroy(ConstellationManager.Instance.gameObject);
            Destroy(MusicManager.Instance.gameObject);
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);
        });
    }

    public void ShowStart()
    {
        // Deactivate other panels
        collectionMenuPanel.SetActive(false);
        freeRoamPanel.SetActive(false);

        // Show active panel
        startPanel.SetActive(true);
    }

    public void ShowFreeRoam()
    {
        // Deactivate other panels
        collectionMenuPanel.SetActive(false);
        startPanel.SetActive(false);

        // Show active panel
        freeRoamPanel.SetActive(true);
    }

    public void ShowCollectionLog()
    {
        // Deactivate other panels
        startPanel.SetActive(false);
        freeRoamPanel.SetActive(false);

        // Show active panel
        collectionMenuPanel.SetActive(true);
        collectionMenuPanel.GetComponent<ConstellationMenu>().RefreshDisplay();

        // Stuff Im trying to figure out still
        constellationMatchScreenPanel.SetActive(false);
    }

    public void ShowMatchMode(int i_matchId)
    {
        // Deactivate other panels
        collectionMenuPanel.SetActive(false);
        freeRoamPanel.SetActive(true);

        if (constellationMatchItemId >= 0)
        {
            MusicManager.Instance.ChangeChannel("playing_find");

            constellationMatchScreenPanel.SetActive(true);
        }
    }


    public void setConstellationMatch(int idInCostellationItemList)
    {

        constellationMatchScreenPanel.SetActive(true);
        MusicManager.Instance.ChangeChannel("playing_find");
        Debug.Log("Enter set image! id:" + idInCostellationItemList);
        constellationMatchItemId = idInCostellationItemList;
        ConstellationMatch = ConstellationManager.Instance.constellationItemList[idInCostellationItemList];
        var newMaterial = constellationMatchScreenPanel.transform.GetComponentInChildren<Renderer>().materials;
        newMaterial[0] = ConstellationMatch.matchMaterial;
        constellationMatchScreenPanel.transform.GetComponentInChildren<Renderer>().materials = newMaterial;
        constellationMatchScreenPanel.transform.localScale = ConstellationMatch.scale;
    }


    void matchConstellation()
    {
        Debug.Log("Matching!");
        // change status of constellation
        // set collectable to 0
        MusicManager.Instance.PlaySoundEffect("success");
        ConstellationManager.Instance.constellationItemList[constellationMatchItemId].collectable = 0;

        // disable matching constellation in camera and set status to unavailable
        constellationMatchScreenPanel.SetActive(false);
        MusicManager.Instance.ChangeChannel("playing_background");
        constellationMatchItemId = -1;


    }

    private void LateUpdate()
    {
        PlayerEntity currentPlayer = GameManager.Instance.GetPlayer();

        // For displaying lookup
        // If facing down
        if ((GameManager.Instance.GetCurrentState() == GameManager.GameState.FreeRoam || GameManager.Instance.GetCurrentState() == GameManager.GameState.MatchStarsMode) &&
            (Vector3.Dot(currentPlayer.transform.forward, Vector3.down) > 0.3))
        {
            // if time to change visibility
            if (_currentLookUpBlinkTime <= 0)
            {
                // Adjust blinks based upon active or inactive
                if (lookUpPanel.activeSelf)
                {
                    lookUpPanel.SetActive(false);
                    _currentLookUpBlinkTime = lookUpBlinkDelay * lookUpBlinkPortionUnvisible;
                }
                else
                {
                    lookUpPanel.SetActive(true);
                    _currentLookUpBlinkTime = lookUpBlinkDelay;
                }
            }
            _currentLookUpBlinkTime -= Time.deltaTime;
        }
        // If facing up
        else
        {
            if (lookUpPanel.activeSelf)
                lookUpPanel.SetActive(false);
        }




        // For displaying constellation screen
        if (constellationMatchItemId >= 0 && constellationMatchScreenPanel.activeInHierarchy)
        {
            constellationMatchScreenPanel.transform.rotation = Quaternion.LookRotation(currentPlayer.transform.forward, currentPlayer.transform.up);

            MusicManager.Instance.UpdateFindingDistanceMusic(ConstellationMatch, constellationMatchScreenPanel);
            if (ConstellationManager.IsMatchConstellation(ConstellationMatch, constellationMatchScreenPanel))
            {

                if (ConstellationManager.Instance.constellationItemList[constellationMatchItemId].displayInMap.GetComponent<ConstellationDisplayItem>().hasAnimation)
                {
                    ConstellationManager.Instance.constellationItemList[constellationMatchItemId].displayInMap.GetComponent<ConstellationDisplayItem>().isAnimated = true;
                    if (ConstellationManager.Instance.constellationItemList[constellationMatchItemId].displayInMap.GetComponent<ConstellationDisplayItem>().isCongratulation)
                        matchConstellation();
                }
                else
                {
                    matchConstellation();
                }
            }
            else
            {
                ConstellationManager.Instance.constellationItemList[constellationMatchItemId].displayInMap.GetComponent<ConstellationDisplayItem>().isAnimated = false;
            }
        }
    }

}
