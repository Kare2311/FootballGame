using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public List<Player> teamA;
    public List<Player> teamB;
    public Player currentPlayer;


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && !IsAnyPlayerWithBall())
        {
            SwitchDefender();
        } 
    }

    bool IsAnyPlayerWithBall()
    {
        foreach(Player p in teamA) if (p.hasBall) return true;
        foreach(Player p in teamB) if (p.hasBall) return true;
        return false;
    }

    void SwitchDefender()
    {
        int index = teamA.IndexOf(currentPlayer);
        currentPlayer = teamA[(index + 1) % teamA.Count];
    }
}
