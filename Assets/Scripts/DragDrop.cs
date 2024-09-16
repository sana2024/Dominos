using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragDrop : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler
{
    [Header("References")]
    [SerializeField] private GameObject firstValue;
    [SerializeField] private GameObject secondValue;
    [Space(10)]

    private float distanceToNotice = 150f;

    [HideInInspector] public int first;
    [HideInInspector] public int second;
    [HideInInspector] public Canvas canvas;
    [HideInInspector] public Player parent;
    [HideInInspector] public Vector2 startPosition;
    public bool isInteractable = true;
    private RectTransform rectTransform;
    private List<GameObject> targets = new List<GameObject>();
    private GameObject finalTarget;

    private bool rotationRunning = false;




    public void SetValues(int _first, int _second)
    {
        foreach (Transform child in firstValue.transform)
        {
            if (child.gameObject.name == _first.ToString())
                child.gameObject.SetActive(true);
            else
                child.gameObject.SetActive(false);
        }

        foreach (Transform child in secondValue.transform)
        {
            if (child.gameObject.name == _second.ToString())
                child.gameObject.SetActive(true);
            else
                child.gameObject.SetActive(false);
        }
        first = _first;
        second = _second;
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {

    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isInteractable)
        {
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
            Vector2 myPos = new Vector2(rectTransform.position.x, rectTransform.position.y);
            finalTarget = null;

            if (targets.Count > 0)
            {
                finalTarget = targets[0];
                float minDistance = 10000000;
                foreach (GameObject target in targets)
                {
                    RectTransform tRectTransform = target.GetComponent<RectTransform>();
                    Vector2 targetPos = new Vector2(tRectTransform.position.x, tRectTransform.position.y);
                    float distance = Vector2.Distance(myPos, targetPos);
                    if (distance <= minDistance)
                    {
                        finalTarget = target;
                        minDistance = distance;
                    }
                }
                if (minDistance > distanceToNotice)
                {
                    finalTarget = null;
                }
                //Debug.Log(minDistance);
                
            }
        }

        if (finalTarget == null)
        {
            if (rectTransform.localRotation.eulerAngles.z != 90 && rotationRunning == false) 
            {
                StartCoroutine(RotateCoroutine(90, 0.13f));
            }
                    
        }
        else
        {
            float angleOf = finalTarget.GetComponent<RectTransform>().localRotation.eulerAngles.z;
            if (rectTransform.localRotation.z != angleOf && rotationRunning == false)
            {
                StartCoroutine(RotateCoroutine(angleOf, 0.13f)); 
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (finalTarget != null)
        {
            finalTarget.GetComponent<DominoPlaceHolder>().Choose();
        }
        else
        {
            parent.currentActiveButton = null;
            StartCoroutine(MoveCoroutine(startPosition, 0.2f));
        }

    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isInteractable)
        {
            parent.ButtonClick(first, second);
            targets = new List<GameObject>(); 
            foreach (GameObject current in parent.game.currentScene)
            {
                if (current.GetComponent<DominoPlaceHolder>() != null)
                {
                    targets.Add(current);
                }
            }
        }
    }

    public void OnDrop(PointerEventData eventData)
    {

    }


//Coroutines===============================================================
    IEnumerator MoveCoroutine(Vector2 targetPosition, float timeToMove)
    {
        Vector2 startPosition = rectTransform.anchoredPosition;
        float elapsedTime = 0f;

        while (elapsedTime < timeToMove)
        {
            float t = elapsedTime / timeToMove;
            rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        rectTransform.anchoredPosition = targetPosition;
    }

    IEnumerator RotateCoroutine(float angle, float timeToMove)
    {
        rotationRunning = true;
        float startPosition = rectTransform.localRotation.eulerAngles.z;
        float elapsedTime = 0f;

        while (elapsedTime < timeToMove)
        {
            float t = elapsedTime / timeToMove;
            float zComp = Mathf.LerpAngle(startPosition, angle, t);
            rectTransform.localRotation = Quaternion.Euler(0, 0, zComp);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        rectTransform.localRotation = Quaternion.Euler(0, 0, angle);
        rotationRunning = false;
    }
}
