using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public bool isAlive = false;
    public int numNeighbors = 0;

    public void SetAlive(bool alive)
    {
        isAlive = alive;

        // it's faster to enable/disable sprite rendering, than destroy game object
        if (alive)
        {
            GetComponent<SpriteRenderer>().enabled = true;
        }
        else
        {
            GetComponent<SpriteRenderer>().enabled = false;
        }
    }
}
