using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
public class playerManager : MonoBehaviour
{
    public GameObject[] players;
    bool[,] playerPickUps;
    public int playerOn = 0;
    bool[,] openedDoors;
    public GameObject[,] doors;
    public GameObject[] clues;
    bool hasMoved;
    List<Vector2> reachableTiles= new List<Vector2>();
    public CameraMovement camMov;
    
    public tileManager tilemanager;
    public GameObject[] keyPads;

    public int[,] codes;

    public TMP_InputField codeInput;
    public TMP_Text clueText;
    public GameObject cluePanel;

    public int movementRadius;
    GameObject[] ingredients;
    public Image[] icons;
    public string winScene;
    public GameObject[] buttons;
    bool buttonPressed;
    public EventSystem uiEventSystem;
    List<string> colorNames =new List<string>{ "red", "orange", "yellow", "green", "blue", "purple" };
    List<Color> colorValues = new List<Color> {
        new Color(Color.red.r,Color.red.g,Color.red.b),
        new Color(1,0.6f,0),
        new Color(Color.yellow.r,Color.yellow.g,Color.yellow.b),
        new Color(Color.green.r,Color.green.g,Color.green.b),
        new Color(Color.blue.r,Color.blue.g,Color.blue.b),
        new Color(1,0,1)
    };
    public void setUp(GameObject[] newPlayers, GameObject[,] newDoors,GameObject[] newKeyPads,GameObject[] newClues,GameObject[] newIngredients) {
        players = newPlayers;
        openedDoors = new bool[players.Length,4];
       // openedDoors[Random.Range(0, openedDoors.GetLength(0)), Random.Range(0, openedDoors.GetLength(1))] = true;
        doors = newDoors;
        for (int i = 0; i < doors.GetLength(0); i++)
        {
            doors[i, 0].GetComponent<door>().open = openedDoors[playerOn, i];
            doors[i, 1].GetComponent<door>().open = openedDoors[playerOn, i];
        }
        hasMoved = false;
        
        reachableTiles = tilemanager.reachibleTiles(new Vector2(players[playerOn].transform.position.x, players[playerOn].transform.position.z), movementRadius);

        camMov.setTarget(players[playerOn].transform);
        keyPads = newKeyPads;

        codes = new int[players.Length,keyPads.Length];
        for (int c = 0; c < players.Length; c++) {
            for (int i = 0; i < codes.GetLength(1); i++) {
                codes[c, i] =  Random.Range(1000, 10000);
            }
        }
        clues = newClues;
        ingredients = newIngredients;
        playerPickUps = new bool[players.Length, ingredients.Length];
        //Debug.Log(reachableTiles.Count);

        for (int i = 0; i < players.Length; i++) {
            int val = Random.Range(0, colorValues.Count);

            players[i].transform.GetChild(0).GetComponent<SpriteRenderer>().color =colorValues[val];
            players[i].name = colorNames[val] + " cat";
            colorNames.RemoveAt(val);
            colorValues.RemoveAt(val);
        }
        EventTrigger buttonDownTriggers = gameObject.AddComponent<EventTrigger>();
        var buttonDown = new EventTrigger.Entry();
        buttonDown.eventID = EventTriggerType.PointerDown;
        buttonDown.callback.AddListener((e) => setButtonPress(true));
        buttonDownTriggers.triggers.Add(buttonDown);
        EventTrigger buttonUpTriggers = gameObject.AddComponent<EventTrigger>();
        var buttonUp = new EventTrigger.Entry();
        buttonDown.eventID = EventTriggerType.PointerUp;
        buttonDown.callback.AddListener((e) => setButtonPress(false));
        buttonDownTriggers.triggers.Add(buttonUp);
    }

    public void setButtonPress(bool set)
    {
        Debug.Log("test");
        buttonPressed = set;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        playerMovement();

        bool thereIsClue = false;
        for (int i = 0; i < clues.Length; i++) {
            if (tilemanager.nextToEachOther(tilemanager.toVector3Int(players[playerOn].transform.position), tilemanager.toVector3Int(clues[i].transform.position))) {
                clueText.text=(clues[i].GetComponent<ClueScript>().getCode(codes[playerOn, i]));
               // Debug.Log(codes[playerOn, i]);
                thereIsClue = true;
            }
        
        }
        for (int i = 0; i < ingredients.Length; i++)
        {
            if (tilemanager.nextToEachOther(tilemanager.toVector3Int(players[playerOn].transform.position), tilemanager.toVector3Int(ingredients[i].transform.position)))
            {
                playerPickUps[playerOn, i] = true;
             //   Debug.Log("pick up");
            }

        }
        for (int i = 0; i < icons.Length; i++)
        {
            for (int c = 0; c < ingredients.Length; c++)
            {
               // Debug.Log(playerPickUps[playerOn, c]);
                if (icons[i].gameObject.name.Contains(ingredients[c].name))
                {
                  //  Debug.Log("a");
                    if (playerPickUps[playerOn, c])
                    {
                        icons[i].gameObject.SetActive(true);
                    }
                    else
                    {
                        icons[i].gameObject.SetActive(false);
                    }
                }
            }
        }

        bool hasWon = true;
        for (int i = 0; i < playerPickUps.GetLength(1); i++)
        {
            if (!playerPickUps[playerOn, i])
            {
                hasWon = false;
            }
        }
        if (hasWon)
        {

            Debug.Log(players[playerOn].name+" hasWon");
            PlayerPrefs.SetString("whoWon", players[playerOn].name);


            Debug.Log(players[playerOn].name + " hasWon");
            PlayerPrefs.SetString("whoWon", players[playerOn].name);
            SceneManager.LoadScene(winScene);
            // PlayerPrefs.GetString("whoWon");
        }
        if (!thereIsClue)
        {
            clueText.text = "";
        }
        cluePanel.SetActive(thereIsClue);

    }
    void playerMovement() {

        if (Input.GetMouseButtonDown(0))
        {
            bool tileWorks = false;
            Vector2 tilePos = tilemanager.mousePosToTilePos();

            for (int i = 0; i < reachableTiles.Count; i++)
            {
                if (tilePos.Equals(reachableTiles[i]))
                {
                    tileWorks = true;
                }
            }

            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1));
            //Vector3 hit;

            RaycastHit[] hit;

            hit =Physics.RaycastAll(ray.origin, ray.direction);
            bool hitUI = false;
            for (int i = 0; i < hit.Length; i++)
            {
                if (hit[i].collider.gameObject.tag == "UI")
                {
                    hitUI = true;
                
                }
            }
           
            
            if (tileWorks&&!hitUI&&!uiEventSystem.IsPointerOverGameObject())
            {
                players[playerOn].transform.position = new Vector3(tilePos.x, 0, tilePos.y);
                hasMoved = true;
            }
        }

    }
    public void nextTurn() {
        playerOn=(playerOn+1)%players.Length;
        for (int i = 0; i < doors.GetLength(0); i++) {
            doors[i, 0].GetComponent<door>().open = openedDoors[playerOn, i];
            doors[i, 1].GetComponent<door>().open = openedDoors[playerOn, i];
        }
        hasMoved = false; 
        reachableTiles = tilemanager.reachibleTiles(new Vector2(players[playerOn].transform.position.x, players[playerOn].transform.position.z), movementRadius);

        camMov.setTarget(players[playerOn].transform);

    }
   

    public void checkCode() { 
        bool nextToKeyPad=false;
        int whichKeyPad = 0;

        for (int i = 0; i < keyPads.Length; i++) {
            if (tilemanager.nextToEachOther(tilemanager.toVector3Int(players[playerOn].transform.position), tilemanager.toVector3Int(keyPads[i].transform.position))) {
                nextToKeyPad = true;
                whichKeyPad = i;
            }
        }
        if (nextToKeyPad && codes[playerOn, whichKeyPad]+"" ==(codeInput.text)) {
            openedDoors[playerOn, whichKeyPad] = true;
            doors[whichKeyPad, 0].GetComponent<door>().open = true;
            doors[whichKeyPad, 1].GetComponent<door>().open = true;
        }
    }

}
