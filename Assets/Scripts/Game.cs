using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml.Serialization;

public class Game : MonoBehaviour
{
    private static int SCREEN_WIDTH = 64;   //units = 1024 pixels / 16 pixels
    private static int SCREEN_HEIGHT = 48;  //units = 768 pixels / 16 pixels

    public HUD hud;

    public float speed = 0.1f;              //seconds for 1 frame 

    private float timer = 0;

    public bool simulationEnabled = false;  // don't start simulation

    Cell[,] grid = new Cell[SCREEN_WIDTH, SCREEN_HEIGHT];

    // Start is called before the first frame update
    void Start()
    {
        EventManager.StartListening("SavePattern", SavePattern);
        EventManager.StartListening("LoadPattern", LoadPattern);

        PlaceCells(0);
    }

    // Update is called once per frame
    void Update()
    {
        if (simulationEnabled)
        {
            if (timer >= speed)
            {
                timer = 0f;

                CountNeighbors();

                PopulationControl();
            }
            else
            {
                timer += Time.deltaTime;
            }
        }

        UserInput();
    }

    private void LoadPattern()
    {
        string path = "Patterns";

        if (!Directory.Exists(path))
        {
            return;
        }

        XmlSerializer serializer = new XmlSerializer(typeof(Pattern));
        string patternName = hud.loadDialog.patternName.
            options[hud.loadDialog.patternName.value].text;
        path += $"/{patternName}.xml";

        StreamReader reader = new StreamReader(path);
        Pattern pattern = (Pattern)serializer.Deserialize(reader.BaseStream);
        reader.Close();

        bool isAlive;

        int x = 0, y = 0;

        Debug.Log(pattern.patternString);

        foreach (char c in pattern.patternString)
        {
            if (c.ToString() == "1")
            {
                isAlive = true;
            }
            else
            {
                isAlive = false;
            }

            grid[x, y].SetAlive(isAlive);

            x++;
            if (x == SCREEN_WIDTH)
            {
                x = 0;
                y++;
            }
        }
    }

    private void SavePattern()
    {
        string path = "Patterns";

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        Pattern pattern = new Pattern();

        string patternString = null;

        for (int y = 0; y < SCREEN_HEIGHT; y++)
        {
            for (int x = 0; x < SCREEN_WIDTH; x++)
            {
                if (grid[x, y].isAlive == false)
                {
                    patternString += "0";
                }
                else
                {
                    patternString += "1";
                }
            }
        }

        pattern.patternString = patternString;

        XmlSerializer serializer = new XmlSerializer(typeof(Pattern));

        StreamWriter writer = new StreamWriter(path + $"/{hud.saveDialog.patternName.text}.xml");
        serializer.Serialize(writer.BaseStream, pattern);
        writer.Close();

        Debug.Log(pattern.patternString);
    }

    void UserInput()
    {
        // To prevent activate cells behind the hud and load different dialogs on "L" or "S" keys
        if (!hud.isActive)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 mousePoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                int x = Mathf.RoundToInt(mousePoint.x);
                int y = Mathf.RoundToInt(mousePoint.y);

                if (x >= 0 && y >= 0 && x < SCREEN_WIDTH && y < SCREEN_HEIGHT)
                {
                    // We are in bounds
                    grid[x, y].SetAlive(!grid[x, y].isAlive);
                }
            }

            if (Input.GetKeyUp(KeyCode.P))
            {
                // Pause simulation
                simulationEnabled = false;
            }

            if (Input.GetKeyUp(KeyCode.B))
            {
                // Build Simulation / Resume
                simulationEnabled = true;
            }

            if (Input.GetKeyUp(KeyCode.S))
            {
                // Save Pattern
                hud.showSaveDialog();
            }

            if (Input.GetKeyUp(KeyCode.L))
            {
                // Load Pattern
                hud.showLoadDialog();
            }
        }
    }

    /// <summary>
    /// Create cell instantiates
    /// 0 - all cells are dead
    /// 1 - random cells are alive
    /// </summary>
    void PlaceCells(int type)
    {
        if (type == 0)
        {
            for (int y = 0; y < SCREEN_HEIGHT; y++)
            {
                for (int x = 0; x < SCREEN_WIDTH; x++)
                {
                    Cell cell = Instantiate(Resources.Load("Prefabs/Cell", typeof(Cell)),
                        new Vector2(x, y), Quaternion.identity) as Cell;
                    grid[x, y] = cell;
                    grid[x, y].SetAlive(false);
                }
            }
        }
        else if (type == 1)
        {
            for (int y = 0; y < SCREEN_HEIGHT; y++)
            {
                for (int x = 0; x < SCREEN_WIDTH; x++)
                {
                    Cell cell = Instantiate(Resources.Load("Prefabs/Cell", typeof(Cell)),
                        new Vector2(x, y), Quaternion.identity) as Cell;
                    grid[x, y] = cell;
                    grid[x, y].SetAlive(RandomAliveCell());
                }
            }
        }
    }

    /// <summary>
    /// Count number of alive neighbors and save in grid.numNeighbors
    /// </summary>
    void CountNeighbors()
    {
        for (int y = 0; y < SCREEN_HEIGHT; y++)
        {
            for (int x = 0; x < SCREEN_WIDTH; x++)
            {
                int numNeighbors = 0;

                // North
                if (y + 1 < SCREEN_HEIGHT)
                {
                    if (grid[x, y + 1].isAlive)
                    {
                        numNeighbors++;
                    }
                }
                // East
                if (x + 1 < SCREEN_WIDTH)
                {
                    if (grid[x + 1, y].isAlive)
                    {
                        numNeighbors++;
                    }
                }
                // South
                if (y - 1 >= 0)
                {
                    if (grid[x, y - 1].isAlive)
                    {
                        numNeighbors++;
                    }
                }
                // West
                if (x - 1 >= 0)
                {
                    if (grid[x - 1, y].isAlive)
                    {
                        numNeighbors++;
                    }
                }
                // NorthEast
                if (x + 1 < SCREEN_WIDTH && y + 1 < SCREEN_HEIGHT)
                {
                    if (grid[x + 1, y + 1].isAlive)
                    {
                        numNeighbors++;
                    }
                }
                // NorthWest
                if (x - 1 >= 0 && y + 1 < SCREEN_HEIGHT)
                {
                    if (grid[x - 1, y + 1].isAlive)
                    {
                        numNeighbors++;
                    }
                }
                // SouthEast
                if (x + 1 < SCREEN_WIDTH && y - 1 >= 0)
                {
                    if (grid[x + 1, y - 1].isAlive)
                    {
                        numNeighbors++;
                    }
                }
                // SouthWest
                if (x - 1 >= 0 && y - 1 >= 0)
                {
                    if (grid[x - 1, y - 1].isAlive)
                    {
                        numNeighbors++;
                    }
                }

                grid[x, y].numNeighbors = numNeighbors;
            }
        }
    }

    /// <summary>
    /// Implementation of rules to create new generation
    /// </summary>
    void PopulationControl()
    {
        for (int y = 0; y < SCREEN_HEIGHT; y++)
        {
            for (int x = 0; x < SCREEN_WIDTH; x++)
            {
                // Rules:
                // - Any live cell with 2 or 3 live neighbors survives;
                // - Any dead cell with 3 live neighbors becomes a live cell;
                // - All other live cells die in the next generation
                // and all other dead cells stay dead.
                if (grid[x, y].isAlive)
                {
                    // Cell is Alive

                    if (grid[x, y].numNeighbors != 2 && grid[x, y].numNeighbors != 3)
                    {
                        grid[x, y].SetAlive(false);
                    }
                }
                else
                {
                    // Cell is Dead
                    if (grid[x, y].numNeighbors == 3)
                    {
                        grid[x, y].SetAlive(true);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Create random true or false
    /// </summary>
    /// <returns>true or false</returns>
    bool RandomAliveCell()
    {
        int rand = UnityEngine.Random.Range(0, 100);

        if (rand > 75)
        {
            return true;
        }

        return false;
    }
}
