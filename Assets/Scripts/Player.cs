using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class Player : MonoBehaviour
{

    [Header("Settings")]
    [SerializeField] public bool isBot = false;
    [Space(10)]

    [Header("References")]
    [SerializeField] public GameObject buttonParent;
    [SerializeField] public GameObject draggableButton;
    [SerializeField] public GameObject placementParent;
    [SerializeField] public Canvas playerCanvas;
    [SerializeField] public Game game;
    [SerializeField] public GameObject score;
    [Space(10)]

    [Header("Prefabs")]
    [SerializeField] private GameObject dominoButton;
    [SerializeField] private GameObject dominoBar;
    [Space(10)]

    public int points = 0;
    public List<int[]> dominos;

    public List<GameObject> dominosButtons = new List<GameObject>();
    public GameObject currentActiveButton = null;

    DominoPlaceHolder toPlace = null;
    GameObject travelingDomino = null;
    public List<int> branchIndexes = new List<int>();
    public int branchToPlace = 0;
    public float difficulty = 0.5f;

    public List<bool> BothDrawed;

    public bool hasPlacable = false;
    public void ActivateTurn()
    {
        if (isBot)
        {
            toPlace = null;
            Debug.Log("Bot Activating");
            StartCoroutine(WaitCoroutine(0.5f));
        }
        else
        {
            foreach (GameObject current in dominosButtons)
            {
                current.GetComponent<DragDrop>().isInteractable = true;
                current.GetComponent<Image>().color = Color.HSVToRGB(0, 0, 1);


            }            
        }
        UpdateDominos(true);
    }

    public void DeactivateTurn()
    {
        UpdateDominos(false);
        if (currentActiveButton != null)
        {
            currentActiveButton = null;
        }

        if (!isBot)
        {
            foreach (GameObject current in dominosButtons)
            {
                current.GetComponent<DragDrop>().isInteractable = false;
                current.GetComponent<Image>().color = Color.HSVToRGB(0, 0, 0.7f);
            }            
        }   

    }

    public void SetScore(int value)
    {
        points = value;
        UpdateScore();
    }

    public void AddScore(int value)
    {
        points += value;
        UpdateScore();
    }

    public void UpdateScore()
    {
        score.GetComponent<Text>().text = points.ToString();
    }

    public void UpdateDominos(bool Activation)
    {
        if (dominos.Count == 0) { return; }


        currentActiveButton = null;
        List<int> xPos = new List<int>();

        // 70 is number of pixels between each piece in the bar (distance between pieces)
        int step = 70;
        int count = dominos.Count;

        //Getting the positions of dominos in bar
        //calculating where the piece should be placed based 
        //on the distance btween pieces in bar and count of pieces

        int xStart;
        if (count % 2 == 0) {

            xStart = (step/2 +  ((count / 2) - 1) * step) * (-1);
        }
        else {

            xStart = ((count - 1) / 2) * step * (-1);
        }

        for (int i = 0; i < count; i++)
        {
            xPos.Add(xStart);
            xStart += step;
        }

        //Deleting old Dominos
        foreach (GameObject current in dominosButtons) 
        { 
            Destroy(current);
        }
        dominosButtons = new List<GameObject>();


        List<int> placable = game.CanBePlaced();


        int e = 0;
        
        foreach (int[] domino in dominos)
        {
            GameObject newPiece;
            if (isBot)
            {
                newPiece = Instantiate(dominoBar, buttonParent.transform);
                newPiece.GetComponent<Image>().color = Color.HSVToRGB(0, 0, 0.7f);
            }
            else
            {

                newPiece = Instantiate(draggableButton, buttonParent.transform);
                DragDrop dragDrop = newPiece.GetComponent<DragDrop>();
                dragDrop.SetValues(dominos[e][0], dominos[e][1]);
                dragDrop.startPosition = new Vector2(xPos[e], 0);
                dragDrop.canvas = playerCanvas;
                dragDrop.parent = this;
            }
            

            newPiece.GetComponent<RectTransform>().anchoredPosition = new Vector3(xPos[e], 0, 0);

            dominosButtons.Add(newPiece);

            if (placable.Contains(dominos[e][0]) || placable.Contains(dominos[e][1]))
            {
                hasPlacable = true;
            }
            else
            {
                newPiece.GetComponent<Image>().color = Color.HSVToRGB(0, 0, 0.7f);
                if (newPiece.GetComponent<DragDrop>() != null)
                    newPiece.GetComponent<DragDrop>().isInteractable = false;

                hasPlacable = false;
            }
            e++;
        }
        if (Activation == true)
        {
        //if cant place any domino
        if (hasPlacable == false)
        {
            Debug.Log("fakt nema co dat");
            int[] drawedDomino = game.DrawDomino();
            if (drawedDomino != null)
                Draw(drawedDomino[0], drawedDomino[1]);
            
            else
                game.SwitchTurns();
        }
        }

    }

    private int[] Decide()
    {
        List<int> placableValues = game.CanBePlaced();
        List<int[]> placableDominos = new List<int[]>();

        foreach (int[] domino in dominos)
        {
            if (placableValues.Contains(domino[0]) || placableValues.Contains(domino[1]))
                placableDominos.Add(domino);
        }

        if (placableValues.Count < 5)
        {
            int initialSumOfEnds = game.GetSumEnds();
            List<int> sumWhenPlaced = GetSumWhenPlaced(placableDominos);


            var combinedList = sumWhenPlaced.Zip(branchIndexes, (x, y) => new { IntValue = x, StringValue = y })
                                    .Zip(placableDominos, (xy, z) => new { IntValue = xy.IntValue, StringValue = xy.StringValue, GameObject = z });

            var orderedCombinedList = combinedList
                .OrderByDescending(item => item.IntValue % 5 == 0)
                .ThenByDescending(item => item.IntValue);

            sumWhenPlaced = orderedCombinedList.Select(item => item.IntValue).ToList();
            branchIndexes = orderedCombinedList.Select(item => item.StringValue).ToList();
            placableDominos = orderedCombinedList.Select(item => item.GameObject).ToList();
            Debug.Log("sumWhenPlaced : " + sumWhenPlaced.Count);
            Debug.Log("branchIndexes : " + branchIndexes.Count);
            Debug.Log("placableDominos : " + placableDominos.Count);
        }

        if (branchIndexes.Count < 1)
        {
            branchToPlace = -1;
            return placableDominos[0];
        }


        int position = GetIndexByValue(placableDominos, difficulty);
        int[] dominoToPlace = placableDominos[position];
        branchToPlace = branchIndexes[position];

        return dominoToPlace;   
    }

    private int GetIndexByValue(List<int[]> list, float value)
    {
        if (value == 1)
        {
            return 0;
        }
        else if (value == 0)
        {
            return Random.Range(0, list.Count);
        }
        else
        {
            int halfIndex = (int)Mathf.Floor(list.Count * value);
            return halfIndex < list.Count ? halfIndex : list.Count - 1;
        }
    }

    public void Choose(int _first, int _second)
    {
        //Debug.Log("wants to play : (" + _first + ";" + _second + "), in branch : " + branchToPlace);
        List<GameObject> currentScene = game.currentScene;
        GameObject rightOne;
        foreach (GameObject current in currentScene)
        {
            DominoPlaceHolder DPH = current.GetComponent<DominoPlaceHolder>();
            if (DPH != null)
            {
                current.GetComponent<Image>().color = new Color(0, 0, 0, 0);
            }
        }

        bool goOn = true;

        foreach (GameObject current in currentScene)
        {
            DominoPlaceHolder DPH = current.GetComponent<DominoPlaceHolder>();
            if (DPH != null)
            {
                if (branchToPlace == -1 || DPH.branchIndex == branchToPlace)
                {
                    if ((DPH.first == _first && DPH.second == _second) || (DPH.first == _second && DPH.second == _first))
                    {
                        //game.FakePlace(DPH.first, DPH.second, DPH.isFirst, DPH.branchIndex);
                        goOn = false;
                        toPlace = DPH;
                        travelingDomino = Instantiate(dominoBar, buttonParent.transform);
                        travelingDomino.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);
                        travelingDomino.GetComponent<RectTransform>().localScale = new Vector3(0, 0, 0);
                        travelingDomino.GetComponent<DominoButton>().SetValues(_first, _second);
                        StartCoroutine(MoveCoroutine(travelingDomino.GetComponent<RectTransform>(), current.GetComponent<RectTransform>().position, 0.3f));
                        StartCoroutine(RotateCoroutine(travelingDomino.GetComponent<RectTransform>(), current.GetComponent<RectTransform>().localRotation.eulerAngles.z, 0.3f));
                        StartCoroutine(ResizeCoroutine(travelingDomino.GetComponent<RectTransform>(), new Vector3(1, 1, 1), 0.1f));
                        break;
                    }
                }
                
            }
        }

        //Second Try
        if (goOn)
        {
            Debug.Log(":(");
            foreach (GameObject current in currentScene)
            {
                DominoPlaceHolder DPH = current.GetComponent<DominoPlaceHolder>();
                if (DPH != null)
                {
                    if ((DPH.first == _first && DPH.second == _second) || (DPH.first == _second && DPH.second == _first))
                    {
                        goOn = false;
                        toPlace = DPH;
                        travelingDomino = Instantiate(dominoBar, buttonParent.transform);
                        travelingDomino.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);
                        travelingDomino.GetComponent<RectTransform>().localScale = new Vector3(0, 0, 0);
                        travelingDomino.GetComponent<DominoButton>().SetValues(_first, _second);
                        StartCoroutine(MoveCoroutine(travelingDomino.GetComponent<RectTransform>(), current.GetComponent<RectTransform>().position, 0.3f));
                        StartCoroutine(RotateCoroutine(travelingDomino.GetComponent<RectTransform>(), current.GetComponent<RectTransform>().localRotation.eulerAngles.z, 0.3f));
                        StartCoroutine(ResizeCoroutine(travelingDomino.GetComponent<RectTransform>(), new Vector3(1, 1, 1), 0.1f));
                        break;
                    }
                }
            }
        }
    }


    public void ButtonClick(int _first, int _second)
    {
        foreach (GameObject i in dominosButtons)
        {
            DragDrop current = i.GetComponent<DragDrop>();

            if ((current.first == _first && current.second == _second) || (current.first == _second && current.second == _first))
            {
                currentActiveButton = i;
                game.GetPlacments(_first, _second);
                break;
            }
        }
    }

    public void Draw(int _first, int _second)
    {
        dominos.Add(new int[2] {_first, _second});
        UpdateDominos(true);
    }

    public void Place(int _first, int _second)
    {
        int i = 0;
        foreach (int[] current in dominos)
        {
            if ((current[0] == _first && current[1] == _second) || (current[1] == _first && current[0] == _second))
            {
                dominos.RemoveAt(i);
                break;
            }
            i++;
        }
    }

    public void SetDominos(List<int[]> _dominos)
    {
        dominos = _dominos;
        UpdateDominos(true);
    }

    private List<int> GetSumWhenPlaced(List<int[]> placableDominos)
    {
        List<int> sumWhenPlaced = new List<int>();
        List<int> branches = new List<int>();
        int initialSum = game.GetSumEnds();
        List<Placement> ends = game.GetEnds();
        

        foreach (int[] current in placableDominos)
        {
            int highest = 0;
            List<int> options = new List<int>();
            List<int> _branchIndexes = new List<int>();
            Placement end = null;
            int i = 0;
            foreach (Placement placement in ends)
            {
                if (placement.nextValue == current[0])
                {
                    int sumToAdd;
                    if (placement.isDouble)
                    {
                        sumToAdd = initialSum - 2 * placement.nextValue + current[1];
                        
                    }
                    else
                    {
                        if (current[0] == current[1])
                        {
                            sumToAdd = initialSum - current[0] + 2 * current[1];
                        }
                        else
                        {
                           sumToAdd = initialSum - current[0] + 2 * current[1]; 
                        }
                    }
                    options.Add(sumToAdd);
                    _branchIndexes.Add(i);
                }
                else if(placement.nextValue == current[1])
                {
                    int sumToAdd;
                    if (placement.isDouble)
                    {
                        sumToAdd = initialSum - 2 * placement.nextValue + current[1];
                        
                    }
                    else
                    {
                        if (current[0] == current[1])
                        {
                            sumToAdd = initialSum - current[0] + 2 * current[1];
                        }
                        else
                        {
                            sumToAdd = initialSum - current[0] + 2 * current[1]; 
                        }
                    }
                    options.Add(sumToAdd);
                    _branchIndexes.Add(i);   
                }
                i++;
            }
            if (options.Count > 0)
            {
                if (options.Count > 1)
                {
                    int currentBest = 0;
                    int currentBranchIndex = 0;
                    int e = 0;
                    foreach (int option in options)
                    {
                        if (option % 5 == 0)
                        {
                            if (currentBest % 5 == 0)
                            {
                                if (option > currentBest)
                                {
                                    currentBest = option;
                                    currentBranchIndex = _branchIndexes[e];
                                }
                            }
                            else
                            {
                                currentBest = option;
                                currentBranchIndex = _branchIndexes[e];
                            }
                        }
                        else
                        {
                            if (currentBest % 5 != 0)
                            {
                                if (option >  currentBest)
                                {
                                    currentBest = option;
                                    currentBranchIndex = _branchIndexes[e];
                                }
                            }
                        }
                        e++;
                    }
                    branches.Add(currentBranchIndex);
                    Debug.Log("adding to list");
                    sumWhenPlaced.Add(currentBest);

                }
                else
                {
                    branches.Add(_branchIndexes[0]);
                    Debug.Log("adding to list");
                    sumWhenPlaced.Add(options[0]);
                }
            }
        }


        
        branchIndexes = branches;
        return sumWhenPlaced;
    }

//Courutines=======================================================================================
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

    IEnumerator MoveCoroutine(Transform objectToMove, Vector3 targetPosition, float timeToMove)
    {
        Vector3 startPosition = objectToMove.position;
        float elapsedTime = 0f;
        while (elapsedTime < timeToMove)
        {
            float t = elapsedTime / timeToMove;
            objectToMove.position = Vector3.Lerp(startPosition, targetPosition, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        objectToMove.position = targetPosition;
    }

    IEnumerator RotateCoroutine(Transform objectToRotate, float angle, float timeToRotate)
    {
        float startRotation = objectToRotate.localRotation.eulerAngles.z;
        float elapsedTime = 0f;

        while (elapsedTime < timeToRotate)
        {
            float t = elapsedTime / timeToRotate;
            float zComp = Mathf.LerpAngle(startRotation, angle, t);
            objectToRotate.localRotation = Quaternion.Euler(0, 0, zComp);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        objectToRotate.localRotation = Quaternion.Euler(0, 0, angle);
        if (travelingDomino != null)
        {
            Destroy(travelingDomino);
            travelingDomino = null;
        }
        game.FakePlace(toPlace.first, toPlace.second, toPlace.isFirst, toPlace.branchIndex);
    }

    IEnumerator WaitCoroutine(float timeToWait)
    {
        yield return new WaitForSeconds(timeToWait);
        int[] currentPick = Decide();
        //Debug.Log(currentPick[0].ToString() + ":" + currentPick[1].ToString());
        game.GetPlacments(currentPick[0], currentPick[1]);
        Choose(currentPick[0], currentPick[1]);       
    }



}
