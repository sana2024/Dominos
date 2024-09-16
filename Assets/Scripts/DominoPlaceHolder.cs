using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DominoPlaceHolder : MonoBehaviour
{

    private Game game;
    public int first;
    public int second;
    public int branchIndex;
    public bool isFirst;


    public void SetAttributs(int _first, int _second, bool _isFirst, int _branchIndex, Game _game)
    {
        first = _first;
        second = _second;
        branchIndex = _branchIndex;
        isFirst = _isFirst;
        game = _game;
    }


    public void Choose()
    {
        //Debug.Log("DPH choose");
        game.Place(first, second, isFirst, branchIndex);
    }
}
