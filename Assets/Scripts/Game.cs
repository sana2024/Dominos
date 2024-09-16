using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Player player1;
    [SerializeField] private Player player2;
    [SerializeField] private GameObject board;
    [SerializeField] private GameObject turnSign;

    [SerializeField] private GameObject nextRoundUI;
    [SerializeField] private List<GameObject> gameOverUI;
    [SerializeField] private List<GameObject> wonUI;
    [Space(10)]

    [Header("Prefabs")]
    [SerializeField] private GameObject placeButton;
    [SerializeField] private GameObject dominoPlaced;
    [Space(10)]


    private List<int[]> dominos;
    private Player[] players;
    private List<Placement> roots = new List<Placement>();
    public List<GameObject> currentScene = new List<GameObject>();


    private int startingPlayer = 0;
    private int isTurn = 0;



    private bool spinnerPlaced = false;
    private bool firstIsDouble = false;
    private bool firstOver = false;
    private Session session;

    public List<bool> BothDrawed;

    void Start()
    {
        dominos = GenerateDominos();
        players = new Player[2] {player1, player2};
        DealDominos();

        if (Session.Instance != null)
        {
            session = Session.Instance;
            players[0].SetScore(session.scorePlayers[0]);
            players[1].SetScore(session.scorePlayers[1]);
            if (player1.isBot)
            {
                if (session.mode == 1)
                {
                    //Easy
                    player1.difficulty = 0.2f;
                }
                else if (session.mode == 2)
                {
                    //Medium
                    player1.difficulty = 0.6f;
                }
                else if (session.mode == 3)
                {
                    //Hard
                    player1.difficulty = 1f;
                }
            }
        }

        if (startingPlayer == 0)
        {   
            //Debug.Log("Deactivating Player");
            //Debug.Log("Activating Bot");
            players[1].DeactivateTurn();
            players[0].ActivateTurn();
            isTurn = 0;
            //Debug.Log(isTurn);
        }
        else
        {
            //Debug.Log("Deactivating Bot");
            //Debug.Log("Activating Player");
            players[0].DeactivateTurn();
            players[1].ActivateTurn();
            isTurn = 1;
            //Debug.Log(isTurn);
        }
    }


//Player Logic======================================================================
    private List<int[]> GenerateDominos()
    {
        List<int[]> result = new List<int[]>();
        for (int i = 0; i < 7; i++)
        {
            for (int e = i; e < 7; e++)
            {
                result.Add(new int[2] {i, e});;
            }
        }

        return result;
    }

    private void DealDominos()
    {
        int highestPair = 0;
        int playerStarting = 0;
        List<int[]> player1Dominos = new List<int[]>();
        List<int[]> player2Dominos = new List<int[]>();
        for (int i = 0; i < 14; i++)
        {
            bool check = false;
            int rand = Random.Range(0, dominos.Count);
            int[] currentDomino = dominos[rand];
            dominos.RemoveAt(rand);

            if (currentDomino[0] == currentDomino[1] && currentDomino[0] >= highestPair)
            {
                highestPair = currentDomino[0];
                check = true;
            }

            if(i % 2 == 0)
            {
                player1Dominos.Add(currentDomino);
                if (check == true) playerStarting = 0;
            }
            else
            {
                player2Dominos.Add(currentDomino);
                if (check == true) playerStarting = 1;
            }
        }

        players[0].SetDominos(player1Dominos);
        players[1].SetDominos(player2Dominos);
        startingPlayer = playerStarting;
    }

    public void SwitchTurns()
    {
        if(player1.hasPlacable == true || player2.hasPlacable == true)
        {
            DeleteTemporary();
            if (isTurn == 0)
            {
                isTurn = 1;
                //Debug.Log("Deactivating Bot");
                //Debug.Log("Activating Player");
                players[0].DeactivateTurn();
                players[1].ActivateTurn();
                //Debug.Log(isTurn);
            }
            else if (isTurn == 1)
            {
                isTurn = 0;
                //Debug.Log("Deactivating Player");
                //Debug.Log("Activating Bot");
                players[1].DeactivateTurn();
                players[0].ActivateTurn();
                //Debug.Log(isTurn);
            }
        }
        else
        {
            Debug.Log("no game left");
        }

    }

    public int[] DrawDomino()
    {
        foreach (var d in dominos)
        {
          //  Debug.Log(d[0] +" "+ d[1]);
        }

        if (dominos.Count == 0)
        {
            BothDrawed.Add(false);
            return null;
        }
        else
        {
        int rand = Random.Range(0, dominos.Count - 1);
        int[] currentDomino = dominos[rand];
        dominos.RemoveAt(rand);
        return currentDomino;
        }
        
    }

    public void Won(int _index)
    {
        players[0].DeactivateTurn();
        players[1].DeactivateTurn();
        nextRoundUI.SetActive(false);
        if (_index == 0)
        {
            gameOverUI[1].SetActive(true);
            wonUI[0].SetActive(true);
        }
        else
        {
            gameOverUI[0].SetActive(true);
            wonUI[1].SetActive(true);           
        }
    }

    public void WonRound(int _index)
    {
        players[0].DeactivateTurn();
        players[1].DeactivateTurn();

        int playerToSum;
        if (_index == 0)
            playerToSum = 1;
        else
            playerToSum = 0;

        int sum = 0;
        foreach (int[] current in players[playerToSum].dominos)
        {
            sum += current[0] + current[1];
        }
        players[_index].AddScore(sum);
        session.AddScore(isTurn, sum);


        nextRoundUI.SetActive(true);
        
    }

    public void NextRound()
    {
        if (player1.isBot)
        {
            SceneManager.LoadScene("BotScene");
        }
        else
        {
            SceneManager.LoadScene("BotScene");
        }
    }

    public void Menu()
    {
        SceneManager.LoadScene("MainMenu");
    }

//Game Logic=========================================================================
    public List<int> CanBePlaced()
    {
        if (roots.Count == 0)
        {
            return new List<int> {0, 1, 2, 3, 4, 5, 6};
        }

        List<int> result = new List<int>();

        foreach (Placement current in roots)
        {
            Placement node = current;
            while (node.next != null)
            {
                node = node.next;
            }
            result.Add(node.nextValue);
        }

        return result;
    }

    public void GetPlacments(int _first, int _second)
    {
        DeleteTemporary();
        if (roots.Count != 0)
        {
            AddTemporary(roots[0], _first, _second);
            AddTemporary(roots[1], _first, _second);
            if (spinnerPlaced)
            {
                AddTemporary(roots[2], _first, _second);
                AddTemporary(roots[3], _first, _second);
            }
        }
        DrawDominos(_first, _second);
    }

    public void AddTemporary(Placement current, int _first, int _second)
    {
        if (current.next != null)
        {
            AddTemporary(current.next, _first, _second);
        }
        else
        {
            if (current.nextValue == _first)
            {
                if (_first == _second)
                {
                    if (spinnerPlaced)
                        current.next = new Placement(_first, true, new Placement(_second, true, null, true), true);
                    else
                        current.next = new Placement(_first, true, new Placement(_second, true, null, true), true, true);
                }
                else
                {
                    current.next = new Placement(_first, true, new Placement(_second, true));
                }
            }
            else if (current.nextValue == _second)
            {
                current.next = new Placement(_second, true, new Placement(_first, true));
            }
        }
    }

    public void FakePlace(int _first, int _second, bool _isFirst=false, int _branchIndex=-1)
    {
        if (_isFirst == true)
        {
            if ((_first == _second))
            {
                firstIsDouble = true;
                roots.Add(new Placement(_first, false, null, true, true));
                roots.Add(new Placement(_first, false, null, true, true));
                spinnerPlaced = true;
            }
            firstOver = true;
            players[isTurn].Place(_first, _second);
            roots.Add(new Placement(_first));
            roots.Add(new Placement(_second));
        }
        else
        {
            if ((_first == _second) && (spinnerPlaced == false))
            {
                roots.Add(new Placement(_first, false, null, true, true));
                roots.Add(new Placement(_first, false, null, true, true));
                spinnerPlaced = true;
            }
             
            players[isTurn].Place(_first, _second);
            Placement branchToCommit = roots[_branchIndex];
            CommitBranch(branchToCommit);
        }

        DeleteTemporary();
        Center();
        if (players[isTurn].dominos.Count == 0)
        {
            WonRound(isTurn);
        }

        int pointsScored = GetSumEnds();
        if (pointsScored % 5 == 0)
        {
            session.AddScore(isTurn, pointsScored);
            players[isTurn].AddScore(pointsScored);
            if (players[isTurn].points >= 150)
            {
                Won(isTurn);
            }
        }

        isTurn = 1;
        //Debug.Log("Deactivating Bot");
        //Debug.Log("Activating Player");        
        //player1.DeactivateTurn();
        player2.ActivateTurn();
        Debug.Log(isTurn);
        //SwitchTurns();
        DrawDominos(-1, -1);
    }

    public void Place(int _first, int _second, bool _isFirst=false, int _branchIndex=-1)
    {
        if (_isFirst == true)
        {
            if ((_first == _second))
            {
                firstIsDouble = true;
                roots.Add(new Placement(_first, false, null, true, true));
                roots.Add(new Placement(_first, false, null, true, true));
                spinnerPlaced = true;
            }
            firstOver = true;
            players[isTurn].Place(_first, _second);
            roots.Add(new Placement(_first));
            roots.Add(new Placement(_second));
        }
        else
        {
            if ((_first == _second) && (spinnerPlaced == false))
            {
                roots.Add(new Placement(_first, false, null, true, true));
                roots.Add(new Placement(_first, false, null, true, true));
                spinnerPlaced = true;
            }
             
            players[isTurn].Place(_first, _second);
            Placement branchToCommit = roots[_branchIndex];
            CommitBranch(branchToCommit);
        }

        DeleteTemporary();
        Center();
        if (players[isTurn].dominos.Count == 0)
        {
            WonRound(isTurn);
        }

        int pointsScored = GetSumEnds();
        if (pointsScored % 5 == 0)
        {
            session.AddScore(isTurn, pointsScored);
            players[isTurn].AddScore(pointsScored);
            if (players[isTurn].points >= 150)
            {
                Won(isTurn);
            }
        }

        SwitchTurns();
        DrawDominos(-1, -1);
    }

    private void CommitBranch(Placement root)
    {
        while (root.next != null)
        {
            root.isTemporary = false;
            root = root.next;
        }
        root.isTemporary = false;
    }

    private void DeleteTemporary()
    {
        foreach (Placement current in roots)
        {
            Placement node = current;
            while (node.next != null)
            {
                if (node.next.isTemporary == true)
                    node.next = null;
                else
                    node = node.next;
            }
        }    
    }

//Organizing and Creating layout of the board================================================================
    public void DrawDominos(int _first, int _second)
    {
        foreach(GameObject current in currentScene) Destroy(current);
        currentScene = new List<GameObject>();


        if (roots.Count == 0)//First Move
        {
            GameObject newPiece = Instantiate(placeButton, board.transform);
            currentScene.Add(newPiece);
            newPiece.GetComponent<DominoPlaceHolder>().SetAttributs(_first, _second, true, -1, this);
            //newPiece.GetComponent<Button>().onClick.AddListener(delegate{Place(_first, _second, true);});
            if (_first == _second)
                newPiece.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, 90);
        }
        else//All Other Moves
        {
            //Setting up first piece
            GameObject newPiece = Instantiate(dominoPlaced, board.transform); 
            currentScene.Add(newPiece);
            newPiece.GetComponent<DominoPiece>().SetValues(roots[0].nextValue, roots[1].nextValue);
            if (firstIsDouble)
                newPiece.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, 90);

            
            //Build Two branches (without spinner branches)
            if (roots[0].next != null)//Left Branch
            {
                if (firstIsDouble || roots[0].next.isDouble)
                    DrawBranch(roots[0].next, new Vector3(-84, 0, 0), new Vector3(-110, 0, 0), new Vector3(-84, 0, 0), 0, 1, 0);
                else
                    DrawBranch(roots[0].next, new Vector3(-110, 0, 0), new Vector3(-110, 0, 0), new Vector3(-84, 0, 0), 0, 1, 0);
            }

            if (roots[1].next != null)//Right Branch
            {
                if (firstIsDouble || roots[1].next.isDouble)
                    DrawBranch(roots[1].next, new Vector3(84, 0, 0), new Vector3(110, 0, 0), new Vector3(84, 0, 0), 1, 1, 0);
                else
                    DrawBranch(roots[1].next, new Vector3(110, 0, 0), new Vector3(110, 0, 0), new Vector3(84, 0, 0), 1, 1, 0);
            }

            //DrawSpinner
            if (spinnerPlaced)
                DrawSpinner();		
        }
    }

    private void DrawBranch(Placement node, Vector3 current, Vector3 normalStep, Vector3 doubleStep, int _branchIndex, int _index, int _rotation)
    {
        GameObject newPiece;
        if (!node.isTemporary)
        {
            newPiece = Instantiate(dominoPlaced, board.transform);
        }
        else
        {
            newPiece = Instantiate(placeButton, board.transform);
            newPiece.GetComponent<DominoPlaceHolder>().SetAttributs(node.nextValue, node.next.nextValue, false, _branchIndex, this);
            //newPiece.GetComponent<Button>().onClick.AddListener(delegate{Place(node.nextValue, node.next.nextValue, false, _branchIndex);});	
        }
        currentScene.Add(newPiece);

        RectTransform transformComponent = newPiece.GetComponent<RectTransform>();
        DominoPiece dominoComponent = newPiece.GetComponent<DominoPiece>();
        transformComponent.anchoredPosition = current;

        if (_branchIndex == 0)
        {
            if (dominoComponent != null) dominoComponent.SetValues(node.next.nextValue, node.nextValue);

            
            if (node.isDouble)
                transformComponent.localRotation = Quaternion.Euler(0, 0, _rotation + 90);
            else
                transformComponent.localRotation = Quaternion.Euler(0, 0, _rotation);

            if (_index == 3)
            {
                _rotation -= 90;
                normalStep = new Vector3(0, 110, 0);
                doubleStep = new Vector3(0, 84, 0);
                if (!node.isDouble)
                    current += new Vector3(-25, -26, 0);
                else
                    current += new Vector3(0, 26, 0);
            }

        
        }
        else if (_branchIndex == 1)
        {
            if (dominoComponent != null) dominoComponent.SetValues(node.nextValue, node.next.nextValue);

            if (node.isDouble)
                transformComponent.localRotation = Quaternion.Euler(0, 0, _rotation + 90);
            else
                transformComponent.localRotation = Quaternion.Euler(0, 0, _rotation);
             

            if (_index == 3)
            {
                _rotation -= 90;
                normalStep = new Vector3(0, -110, 0);
                doubleStep = new Vector3(0, -84, 0);
                if (!node.isDouble)
                    current += new Vector3(25, 26, 0);
                else
                    current += new Vector3(0, -26, 0);
            }        
            
        }



        if (node.next.next != null)
        {
            if (node.next.next.isDouble || node.isDouble)
                current += doubleStep;
            else
                current += normalStep;
            DrawBranch(node.next.next, current, normalStep, doubleStep, _branchIndex, _index + 1, _rotation);
        }     
    }

    private void DrawSpinner()
    {
        GameObject spinner = null;
        if (firstIsDouble)
            spinner = currentScene[0];

        //find spinner
        if (spinner == null)
        {
            int spinnerValue = roots[2].nextValue;
            foreach (GameObject current in currentScene)
            {
                DominoPiece currentPiece = current.GetComponent<DominoPiece>();
                if (currentPiece != null)
                {   
                    if ((currentPiece.first == currentPiece.second) && (currentPiece.first == spinnerValue))
                    {
                        spinner = current;
                        break;
                    }	
                }
                //DominoPlaceHolder currentPlace = current.GetComponent
            }            
        }

        Vector3 spinnerPosition = spinner.GetComponent<RectTransform>().anchoredPosition;
        int spinnerRotation = (int) spinner.GetComponent<RectTransform>().localRotation.eulerAngles.z;

        int spinnerOrientation = 0;
        //Get Spinner branch (Left, Middle, Right)
        if (spinnerRotation != 90)
        {
            if (spinnerPosition.x > 0)
                spinnerOrientation = 1;
            else
                spinnerOrientation = -1;
        }

        //If spinner is 3rd special case
        bool isThird = IsSpinnerThird();

        

        Vector3 start;
        Vector3 stepDirection;
        int normalStepSize = 110;
        int doubleStepSize = 84;
        if (!isThird)
        {
            if (GetLength(roots[2]) > 1)
            {
                if (spinnerOrientation == 0) 
                {
                    start = spinnerPosition + (new Vector3(0, -1, 0) * normalStepSize);
                    stepDirection = new Vector3(0, -1, 0);
                }
                else
                {
                    start = spinnerPosition + (new Vector3(-1, 0, 0)  * normalStepSize);
                    stepDirection = new Vector3(-1, 0, 0);
                }
                DrawSpinnerBranch(roots[2].next, start, stepDirection * normalStepSize , stepDirection * doubleStepSize, 2, 1, spinnerRotation, spinnerOrientation) ;
            }

            if (GetLength(roots[3]) > 1)
            {
                if (spinnerOrientation == 0) 
                {
                    start = spinnerPosition + (new Vector3(0, 1, 0) * normalStepSize);
                    stepDirection = new Vector3(0, 1, 0);
                }
                else
                {
                    start = spinnerPosition + (new Vector3(1, 0, 0)  * normalStepSize);
                    stepDirection = new Vector3(1, 0, 0);
                }
                DrawSpinnerBranch(roots[3].next, start, stepDirection * normalStepSize , stepDirection * doubleStepSize, 3, 1, spinnerRotation, spinnerOrientation) ;
            }
        }
        else
        {
            if (GetLength(roots[2]) > 1)
            {
                if (spinnerPosition.x < 0)
                {
                    //left almost normal
                    start = spinnerPosition + (new Vector3(0, -1, 0) * normalStepSize);
                    stepDirection = new Vector3(0, -1, 0);
                    DrawSpinnerBranch(roots[2].next, start, stepDirection * normalStepSize , stepDirection * doubleStepSize, 2, 1, spinnerRotation, 2);
                }
                else
                {
                    //right
                    start = spinnerPosition + (new Vector3(1, 0, 0) * doubleStepSize);
                    stepDirection = new Vector3(1, 0, 0);
                    DrawSpinnerBranch(roots[2].next, start, stepDirection * normalStepSize , stepDirection * doubleStepSize, 2, 1, 0, 3);
                }
            }

            if (GetLength(roots[3]) > 1)
            {
                if (spinnerPosition.x < 0)
                {
                    //left
                    start = spinnerPosition + (new Vector3(-1, 0, 0) * doubleStepSize);
                    stepDirection = new Vector3(-1, 0, 0);
                    DrawSpinnerBranch(roots[3].next, start, stepDirection * normalStepSize , stepDirection * doubleStepSize, 3, 1, 0, 2);
                }
                else
                {
                    //right almost normal
                    start = spinnerPosition + (new Vector3(0, 1, 0) * normalStepSize);
                    stepDirection = new Vector3(0, 1, 0);
                    DrawSpinnerBranch(roots[3].next, start, stepDirection * normalStepSize , stepDirection * doubleStepSize, 3, 1, spinnerRotation, 3);
                }
            }
        }
    }

    private void DrawSpinnerBranch(Placement node, Vector3 current, Vector3 normalStep, Vector3 doubleStep, int _branchIndex, int _index, int _rotation, int _orientation)
    {
        GameObject newPiece;
        if (!node.isTemporary)
        {
            newPiece = Instantiate(dominoPlaced, board.transform);
        }
        else
        {
            newPiece = Instantiate(placeButton, board.transform);
            newPiece.GetComponent<DominoPlaceHolder>().SetAttributs(node.nextValue, node.next.nextValue, false, _branchIndex, this);
            //newPiece.GetComponent<Button>().onClick.AddListener(delegate{Place(node.nextValue, node.next.nextValue, false, _branchIndex);});	
        }
        currentScene.Add(newPiece);
    
        RectTransform transformComponent = newPiece.GetComponent<RectTransform>();
        DominoPiece dominoComponent = newPiece.GetComponent<DominoPiece>();
        transformComponent.anchoredPosition = current;

        if (_branchIndex == 2)
        {
            if (dominoComponent != null) dominoComponent.SetValues(node.next.nextValue, node.nextValue);
        }
        else
        {
            if (dominoComponent != null) dominoComponent.SetValues(node.nextValue, node.next.nextValue);
        }


        if (node.isDouble)
            transformComponent.localRotation = Quaternion.Euler(0, 0, _rotation + 90);
        else
            transformComponent.localRotation = Quaternion.Euler(0, 0, _rotation);

        if (_orientation == 0 && _branchIndex == 3)
        {
            if(_index == 3)
            {
                _rotation -= 90;
                normalStep = new Vector3(110, 0, 0);
                doubleStep = new Vector3(84, 0, 0);
                if (!node.isDouble)
                    current += new Vector3(-26, 26, 0);
                else
                    current += new Vector3(26, 0, 0);  
            }
        }
        else if (_orientation == 0 && _branchIndex == 2)
        {
            if (_index == 3)
            {
                _rotation -= 90;
                normalStep = new Vector3(-110, 0, 0);
                doubleStep = new Vector3(-84, 0, 0);
                if (!node.isDouble)
                    current += new Vector3(26, -26, 0);
                else
                    current += new Vector3(-26, 0, 0);
            }
        }
        else if (_orientation == -1 && _branchIndex == 2)
        {
            if(_index == 1)
            {
                _rotation -= 90;
                normalStep = new Vector3(0, -110, 0);
                doubleStep = new Vector3(0, -84, 0);
                if (!node.isDouble)
                    current += new Vector3(-26, 26, 0);
                else
                    current += new Vector3(-26, 110, 0);
            }
        }
        else if (_orientation == 1 && _branchIndex == 3)
        {
            if(_index == 1)
            {
                _rotation += 90;//change to plus
                normalStep = new Vector3(0, 110, 0);
                doubleStep = new Vector3(0, 84, 0);
                if (!node.isDouble)
                    current += new Vector3(26, -26, 0);
                else
                    current += new Vector3(26, -110, 0);            
            }
        }
        else if (_orientation == 2 && _branchIndex == 2)
        {
            if (_index == 2)
            {
                _rotation -= 90;
                normalStep = new Vector3(110, 0, 0);
                doubleStep = new Vector3(84, 0, 0);
                if (!node.isDouble)
                    current += new Vector3(-26, -26, 0);
                else
                    current += new Vector3(26, -110, 0);                 
            }
        } 
        else if (_orientation == 3 && _branchIndex == 2)
        {
            if (_index == 1)
            {
                if (dominoComponent != null) dominoComponent.SetValues(node.next.nextValue, node.nextValue);
                _rotation -= 90;
                normalStep = new Vector3(0, 110, 0);
                doubleStep = new Vector3(0, 84, 0);
                if (!node.isDouble)
                    current += new Vector3(26, -26, 0);
                else
                    current += new Vector3(26, -110, 0);                 
            }            
        }
        else if (_orientation == 2 && _branchIndex == 3)
        {
            if (_index == 1)
            {
                if (dominoComponent != null) dominoComponent.SetValues(node.next.nextValue, node.nextValue);
                _rotation -= 90;
                normalStep = new Vector3(0, -110, 0);
                doubleStep = new Vector3(0, -84, 0);
                if (!node.isDouble)
                    current += new Vector3(-26, 26, 0);
                else
                    current += new Vector3(-26, 26, 0); 
            }
        }
        else if (_orientation == 3 && _branchIndex == 3)
        {
            if (_index == 2)
            {
                if (dominoComponent != null) dominoComponent.SetValues(node.next.nextValue, node.nextValue);
                _rotation += 90;
                normalStep = new Vector3(-110, 0, 0);
                doubleStep = new Vector3(-84, 0, 0);
                if (!node.isDouble)
                    current += new Vector3(26, -26, 0);
                else
                    current += new Vector3(26, -110, 0); 
            }
        }

        if (node.next.next != null)
        {
            if (node.next.next.isDouble || node.isDouble)
                current += doubleStep;
            else
                current += normalStep;
            DrawSpinnerBranch(node.next.next, current, normalStep, doubleStep, _branchIndex, _index + 1, _rotation, _orientation);
        }
    }

    private bool IsSpinnerThird()
    {
        Placement current = roots[0];
        if (GetLength(current) > 5)
        {   
            int i = 0;
            while (current.next != null)
            {
                if (current.isSpiner && (i == 5 || i == 6))
                {
                    return true;
                }
                i++;
                current = current.next;
            }
        }

        current = roots[1];
        if (GetLength(current) > 5)
        {   
            int i = 0;
            while (current.next != null)
            {
                if (current.isSpiner && (i == 5 || i == 6))
                {
                    return true;
                }
                i++;
                current = current.next;
            }
        }

        return false;
    }

    private void Center()
    {
        float currentScale = 2f;
        if (currentScene.Count >= 16) {currentScale = 0.84f;}
        else if (currentScene.Count >= 12) {currentScale = 0.87f;}
        else if (currentScene.Count >= 9) {currentScale = 0.9f;}
        else if (currentScene.Count >= 7) {currentScale = 0.95f;}
        else if (currentScene.Count >= 5) {currentScale = 1f;}
        else if (currentScene.Count >= 4) {currentScale = 1.2f;}
        else if (currentScene.Count >= 3) {currentScale = 1.6f;;}
        else if (currentScene.Count >= 2) {currentScale = 2f;}
        else if (currentScene.Count >= 1) {currentScale = 2f;}

        StartCoroutine(ResizeCoroutine(board.transform, new Vector3(currentScale, currentScale, currentScale), 0.2f));


        Vector2 center = new Vector2(0, 0);
        float maxX = -10000;
        float minX = 10000;
        float maxY = -10000;
        float minY = 10000;

        foreach (GameObject current in currentScene)
        {
            Vector2 currentPosition =  current.GetComponent<RectTransform>().anchoredPosition;

            if (currentPosition.x > maxX)
                maxX = currentPosition.x;
            if (currentPosition.x < minX)
                minX = currentPosition.x;
            if (currentPosition.y > maxY)
                maxY = currentPosition.y;
            if (currentPosition.y < minY)
                minY = currentPosition.y;

            
        }
        center = new Vector2(maxX + minX, maxY + minY) / 2;
        StartCoroutine(MoveCoroutine(board, new Vector3(-center.x * currentScale, -center.y  * currentScale), 0.2f));

    }


//Helpers========================================================================================
    public int GetSumEnds()
    {
        if (roots.Count == 0) {return 0;}
        int sum = 0;
        Placement node;
        List<int> viableRoots = new List<int> {0, 1};
        if (roots.Count > 2)
        {
            if (GetLength(roots[2]) > 1) viableRoots.Add(2);
            if (GetLength(roots[3]) > 1) viableRoots.Add(3);
        }

        if (GetLength(roots[0]) == 1 && GetLength(roots[1]) == 1)
        {
            if (roots[0].isDouble) return 2 * roots[0].nextValue;
        }

        foreach (int i in viableRoots)
        {
            node = roots[i];
            while (node.next != null)
            {
                node = node.next;
            }

            if (i == 2 || i == 3)
            {
                if (node.isDouble)
                    sum += 2 * node.nextValue;
                else
                    sum += node.nextValue;
            }
            else
            {
                if (node.isSpiner)
                {
                    if (!(GetLength(roots[2]) > 1 || GetLength(roots[3]) > 1))
                    {
                        sum += 2 * node.nextValue;
                    }
                }
                else
                {
                    if (node.isDouble)
                        sum += 2 * node.nextValue;
                    else
                        sum += node.nextValue;                   
                }
            }
        }

        return sum;
    }

    public int GetLength(Placement current)
    {
        if (current == null) return 0;

        int l = 1;
        while (current.next != null)
        {
            l++;
            current = current.next;
        }

        return l;
    }

    public List<int> GetPlacedValues()
    {
        List<int> result = new List<int>();
        for(int i=0; i < roots.Count; i++)
        {
            Placement current = roots[i];
            if (i > 2) {current = current.next;}

            while (current != null)
            {
                result.Add(current.nextValue);
                current = current.next;
            }
        }
        return result;
    }

    public List<Placement> GetEnds()
    {
        List<Placement> result = new List<Placement>();
        foreach (Placement current in roots)
        {
            Placement node = current;
            while (node.next != null)
            {
                node = node.next;
            }
            result.Add(node);
        }
        return result;
    }


//Courutines=======================================================================================
    IEnumerator MoveCoroutine(GameObject objectToMove, Vector2 targetPosition, float timeToMove)
    {
        Vector2 startPosition = objectToMove.GetComponent<RectTransform>().anchoredPosition;
        float elapsedTime = 0f;

        while (elapsedTime < timeToMove)
        {
            float t = elapsedTime / timeToMove;
            objectToMove.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        objectToMove.GetComponent<RectTransform>().anchoredPosition = targetPosition;
    }

    IEnumerator ResizeCoroutine(Transform objectToResize, Vector3 targetSize, float timeToResize)
    {
        Vector3 startSize = objectToResize.localScale;
        float elapsedTime = 0f;

        while (elapsedTime < timeToResize)
        {
            float t = elapsedTime / timeToResize;
            objectToResize.localScale = Vector3.Lerp(startSize, targetSize, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        objectToResize.localScale = targetSize;
    }

}



public class Placement
{
    public Placement next = null;
    public int nextValue;
    public bool isTemporary;
    public bool isDouble;
    public bool isSpiner;

    public Placement( int _nextValue, bool _isTemporary=false, Placement _next=null, bool _isDouble=false, bool _isSpiner=false)
    {
        nextValue = _nextValue;
        isTemporary = _isTemporary;
        next = _next;
        isDouble = _isDouble;
        isSpiner = _isSpiner;
    }
}




