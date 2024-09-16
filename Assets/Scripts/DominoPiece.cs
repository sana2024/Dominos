using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DominoPiece : MonoBehaviour
{

    [Header("References")]
    [SerializeField] private GameObject firstValue;
    [SerializeField] private GameObject secondValue;
    


    [HideInInspector] public int first;
    [HideInInspector] public int second;




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

}
