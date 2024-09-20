using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public int gridIndex;
    public int gridX;
    public int gridY;
    public bool populated;
    public int neighbors;
    public bool deleted = false;
    public int nextTickResult = 0;
    SpriteRenderer cellRenderer;
    public GameOfLife game;
    Transform cellBlock;
    public int cellHue = 15;
    private void Awake()
    {
        //transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.HSVToRGB((float)Random.Range(0, 100) / 100f, 1f, 0.7f);
        cellBlock = transform.GetChild(0);
        cellRenderer = cellBlock.GetComponent<SpriteRenderer>();
        cellBlock.gameObject.SetActive(false);
    }

    void OnMouseDown()
    {
        if (populated)
        {
            game.setCells(game.cells -= 1);
            nextTickResult = 0;
            populateOrDie();
        }
        else
        {
            game.setCells(game.cells += 1);
            nextTickResult = 1;
            populateOrDie();
        }
    }

    public void forceDeath()
    {
        deleted = true;
        nextTickResult = 0;
        transform.GetChild(0).gameObject.SetActive(false);
        transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.white;
        populated = false;
    }

    public void populateOrDie()
    {
        cellBlock = transform.GetChild(0);
        if (deleted)
        {
            return;
        }
        if (nextTickResult == 0)
        {
            cellBlock.gameObject.SetActive(false);
            populated = false;  
        }
        else if (nextTickResult == 1)
        {
            cellBlock.gameObject.SetActive(true);
            populated = true;
            float H, S, V;
            Color.RGBToHSV(cellRenderer.color, out H, out S, out V);
            S = Mathf.Max(S, 0.2f);
            cellRenderer.color = Color.HSVToRGB(game.hue, S - 0.1f, V);
        }
        else
        {
            float H, S, V;      
            Color.RGBToHSV(cellRenderer.color, out H, out S, out V);
            S = Mathf.Min(S, 0.9f);
            cellRenderer.color = Color.HSVToRGB(game.hue, S + 0.1f, V);
        }
    }
}
