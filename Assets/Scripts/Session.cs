using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Session : MonoBehaviour
{
    public static Session Instance;

    public int[] scorePlayers = new int[2] {0, 0};

    //0=Player, 1=Easy, 2=Medium, 3=Hard
    public int mode;
    public int lastTurn = -1;



    private void Awake()
    {
        Application.targetFrameRate = 60;
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }


    public void AddScore(int _index, int _value)
    {
        scorePlayers[_index] += _value;
    }
}
