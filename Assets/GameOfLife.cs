using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameOfLife : MonoBehaviour
{
    public List<Grid> grids;
    public Grid[,] gridPositions;

    public float gridSize = 0.2f;

    public int gridSlotsSetting = 60;
    private int gridSlots;
    public float popValue = 100;

    public int cells;
    private int generation = 0;
    public float cellTick = 0.5f;
    public float generationSpeed = 0.5f;
    public float hue = 0.2f;

    public bool step = false;
    public GameObject gridEntity;

    private Vector3 screen;
    private Camera cam;

    public Scrollbar scrollSpeed;
    public Scrollbar scrollPop;
    public Scrollbar scrollSize;
    public Scrollbar scrollSlots;
    public Scrollbar scrollColor;

    public TMP_Text speedText;
    public TMP_Text popText;
    public TMP_Text sizeText;
    public TMP_Text slotsText;
    public TMP_Text cellsText;
    public TMP_Text genText;
    public TMP_Text colorText;
    public Image colorImage;

    public GameObject loading;

    readonly float CAMERA_MOVE_SPEED = 0.05f;
    void Start()
    {
        cam = Camera.main;
        drawGridsCells();

        StartCoroutine(nextTickCells());
        print(colorImage);
        colorImage.color = Color.HSVToRGB(hue, 0.5f, 1);
    }

    // Update is called once per frame
    void Update()
    {
        cameraControls();
    }

    private void cameraControls()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        Vector2 mousePos = new Vector2(mouseX, mouseY);

        if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            cam.transform.position -= new Vector3(mousePos.x * CAMERA_MOVE_SPEED * cam.orthographicSize, mousePos.y * CAMERA_MOVE_SPEED * cam.orthographicSize);
        }
        cam.orthographicSize = Mathf.Max(cam.orthographicSize -= Input.mouseScrollDelta.y / 2, 2);
    }

    public void setCells(int value)
    {
        cells = value;
        cellsText.text = "Cells: " + cells.ToString();
    }

    private void drawGridsCells()
    {
        gridSlots = gridSlotsSetting;
        gridEntity.transform.localScale = new Vector3(gridSize, gridSize, 1);
        gridPositions = new Grid[gridSlots, gridSlots];
        float offsetRow = 0;

        for (int y = 0; y < gridSlots; y++)
        {
            for (int x = 0; x < gridSlots; x++)
            {
                Grid grid = createGridEntity(x * gridEntity.transform.localScale.x, offsetRow, new Vector2(x, y));
                grid.gameObject.tag = "Cell";
                grid.deleted = false;
                int c = Random.Range(0, 1000);
                if (c < popValue)
                {
                    grid.nextTickResult = 1;
                    grid.populateOrDie();
                }
                else
                {
                    grid.nextTickResult = 0;
                    grid.populateOrDie();
                }
            }
            offsetRow += gridEntity.transform.localScale.x;
        }
        cam.transform.position =  grids[Mathf.RoundToInt((grids.Count / 2) + gridSlots/2)].transform.position - new Vector3(0,0,10);
    }

    private Grid createGridEntity(float x, float y, Vector2 gridPos)
    {
        GameObject newGrid = Instantiate(gridEntity, new Vector3(x, y, 0), gridEntity.transform.rotation);
        Grid gridComponent = newGrid.GetComponent<Grid>();
        gridComponent.gridIndex = grids.Count;
        gridComponent.gridX = (int)gridPos.x;
        gridComponent.gridY = (int)gridPos.y;
        gridPositions[(int)gridPos.x, (int)gridPos.y] = gridComponent;
        grids.Add(gridComponent);
        gridEntity.SetActive(true);
        gridEntity.transform.GetChild(0).gameObject.SetActive(false);
        return gridComponent;
    }

    IEnumerator nextTickCells()
    {
        for (int y = 0; y < gridSlots; y++)
        {
            for (int x = 0; x < gridSlots; x++)
            {
                gridPositions[x, y].populateOrDie();
            }
        }
        yield return new WaitForSeconds(generationSpeed);
        setCells(0);
        for (int y = 0; y < gridSlots; y++)
        {
            for (int x = 0; x < gridSlots; x++)
            {
                checkGrid(x, y);
            }
        }
        if (generationSpeed != 0 && !step)
        {
            StartCoroutine(nextTickCells());
        }
        step = false;
        cellsText.text = "Cells: " + cells.ToString();
        genText.text = "Generation: " + generation.ToString();
        generation += 1;
    }

    private void checkGrid(int x, int y)
    {
        Grid gridObj = gridPositions[x, y];
        gridObj.neighbors = 0;
        if (gridObj.populated)
            setCells(cells + 1);

        for (int gridY = y - 1; gridY < y + 2; gridY++)
        {
            for (int gridX = x - 1; gridX < x + 2; gridX++)
            {
                if ((gridX >= gridSlots || gridY >= gridSlots || gridX < 0 || gridY < 0) || (gridX == x && gridY == y))
                {
                    continue;
                }
                if (gridPositions[gridX, gridY].populated == true)
                {
                    gridObj.neighbors++;
                }
            }
        }
        if ((gridObj.neighbors < 2 || gridObj.neighbors > 3) && gridObj.populated)
        {
            gridObj.nextTickResult = 0;
        }
        if ((gridObj.neighbors == 2 || gridObj.neighbors == 3) && gridObj.populated) {
            gridObj.nextTickResult = 2;
        }
        if (gridObj.neighbors == 3 && !gridObj.populated)
        {
            gridObj.nextTickResult = 1;
        }
    }

    public void pause()
    {
        if (generationSpeed == 0)
        {
            unPause();
        }
        generationSpeed = 0;
        StopCoroutine(nextTickCells());
    }

    public void unPause()
    {
        generationSpeed = cellTick;
        StartCoroutine(nextTickCells());
    }

    public void stepForward()
    {
        StopCoroutine(nextTickCells());
        step = true;
        generationSpeed = 0.01f;
        StartCoroutine(nextTickCells());
    }

    public void scrollbarSpeed()
    {
        cellTick = 1.01f - scrollSpeed.value;
        speedText.text = "Speed: " + scrollSpeed.value.ToString() + "x";
        if (generationSpeed != 0) {
            generationSpeed = cellTick;
        }
    }

    public void popSlider()
    {
        popValue = Mathf.Round(1000 * scrollPop.value);
        popText.text = "Population Value: " + popValue.ToString();
    }

    public void hueSlider()
    {
        hue = scrollColor.value;
        colorImage.color = Color.HSVToRGB(hue, 0.5f, 1);
    }

    public void clearAllCells() {
        for (int y = 0; y < gridSlots; y++)
        {
            for (int x = 0; x < gridSlots; x++)
            {
                gridPositions[x, y].forceDeath();
            }
        }
        setCells(0);
        generation = 0;
        Invoke("revive", 1f);
    }
    void revive()
    {
        for (int y = 0; y < gridSlots; y++)
        {
            for (int x = 0; x < gridSlots; x++)
            {
                gridPositions[x, y].deleted = false;
            }
        }
    }
    public void resetGame()
    {
        loading.GetComponent<TextMeshProUGUI>().enabled = true;
        Invoke("resetFunction", 0.1f);
    }

    void resetFunction()
    {
        grids.Clear();
        GameObject[] cellArray = GameObject.FindGameObjectsWithTag("Cell");
        for (int i = 0; i < cellArray.Length; i++)
        {
            Destroy(cellArray[i]);
        }
        drawGridsCells();
        loading.GetComponent<TextMeshProUGUI>().enabled = false;
    }

    public void changeGridSize()
    {
        gridSize = Mathf.Round(scrollSize.value * 10) / 10;
        sizeText.text = "Grid Size: " + (gridSize * 10).ToString();
    }
    public void changeGridSlot()
    {
        gridSlotsSetting = Mathf.RoundToInt(scrollSlots.value * 300);
        slotsText.text = "Grids: " + gridSlotsSetting + "x" + gridSlotsSetting;
    }
}
