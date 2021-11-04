using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    public GameObject tilePrefab;
    public GameObject tunnelPrefab;
    public int tileX;
    public int tileY;
    public int sourcesCount;
    public int locksCount;
    public int tunnelsCount;
    public bool useSourceColors;
    public int connectionsToRemove;
    public int removeAttempts;

    public List<Color> sourceColors;

    public MeshRenderer plane;
    public List<Material> backgroundMaterials;

    private new Camera camera;
    private Vector3 cameraBasePosition;
    public static Tile[,] tileGrid;
    public static List<Tile> sources;
    public static List<int> xTunnels;
    public static List<int> yTunnels;
    public static bool hasWon;

    public GameObject menu;
    public GameObject winText;
    public AudioSource winSfxSource;

    public static int CurrentSeed { get; set; }


    void Start()
    {
        if (PlayerPrefs.HasKey("X"))
            tileX = PlayerPrefs.GetInt("X");
        if (PlayerPrefs.HasKey("Y"))
            tileY = PlayerPrefs.GetInt("Y");
        if (PlayerPrefs.HasKey("Sources"))
            sourcesCount = PlayerPrefs.GetInt("Sources");
        if (PlayerPrefs.HasKey("Locks"))
            locksCount = PlayerPrefs.GetInt("Locks");
        if (PlayerPrefs.HasKey("Tunnels"))
            tunnelsCount = PlayerPrefs.GetInt("Tunnels");
        if (PlayerPrefs.HasKey("SourceColors"))
            useSourceColors = PlayerPrefs.GetInt("SourceColors") == 1;
        if (PlayerPrefs.HasKey("Seed"))
        {
            CurrentSeed = PlayerPrefs.GetInt("Seed");
            PlayerPrefs.DeleteKey("Seed");
        }
        else
        {
            CurrentSeed = Random.Range(0, 1000000);
        }
        Random.InitState(CurrentSeed);


        tileGrid = null;
        sources = new List<Tile>();
        hasWon = false;

        camera = FindObjectOfType<Camera>();
        camera.GetComponent<CameraController>().InitCamera(tileX, tileY);

        if (tileX * tileY < 20)
        {
            plane.material = backgroundMaterials[0];
        }
        else if (tileX * tileY < 60)
        {
            plane.material = backgroundMaterials[1];
        }
        else
        {
            plane.material = backgroundMaterials[2];
        }

        AddTunnels();
        GenerateGrid();
        PlaceSources();
        RemoveConnections();
        PlaceLocks();
        RotateTiles();
        UpdateSourceConnections();
    }


    private void AddTunnels()
    {
        xTunnels = new List<int>();
        yTunnels = new List<int>();

        for (int i = 0; i < tunnelsCount && i < tileX + tileY; i++)
        {
            int dimension, value;
            int validXCount = tileX - xTunnels.Count;
            int validYCount = tileY - yTunnels.Count;
            dimension = Random.Range(0, validXCount + validYCount) < validXCount ? 0 : 1;
            do
            {
                value = Random.Range(0, dimension == 0 ? tileX : tileY);
            } while (dimension == 0 ? xTunnels.Contains(value) : yTunnels.Contains(value));

            if (dimension == 0)
            {
                xTunnels.Add(value);
                GameObject newTunnel = Instantiate(tunnelPrefab);
                newTunnel.transform.position = new Vector3(value, 0.3f, -1);
                newTunnel.transform.eulerAngles = new Vector3(0, 180, 0);

                GameObject newTunnel2 = Instantiate(tunnelPrefab);
                newTunnel2.transform.position = new Vector3(value, 0.3f, tileY);
            }
            else
            {
                yTunnels.Add(value);
                GameObject newTunnel = Instantiate(tunnelPrefab);
                newTunnel.transform.position = new Vector3(-1, 0.3f, value);
                newTunnel.transform.eulerAngles = new Vector3(0, 270, 0);

                GameObject newTunnel2 = Instantiate(tunnelPrefab);
                newTunnel2.transform.position = new Vector3(tileX, 0.3f, value);
                newTunnel2.transform.eulerAngles = new Vector3(0, 90, 0);
            }
        }
    }

    private void GenerateGrid()
    {
        tileGrid = new Tile[tileX, tileY];
        for (int i = 0; i < tileX; i++)
            for (int j = 0; j < tileY; j++)
            {
                GameObject newTile = Instantiate(tilePrefab);
                newTile.transform.position = new Vector3(i, 0, j);
                tileGrid[i, j] = newTile.GetComponent<Tile>();
                tileGrid[i, j].x = i;
                tileGrid[i, j].y = j;
                tileGrid[i, j].gameManager = this;

                if (i == 0 && !yTunnels.Contains(j))
                {
                    GameObject pipeToRemove = tileGrid[i, j].WestPipe;
                    pipeToRemove.SetActive(false);
                    tileGrid[i, j].pipes.Remove(pipeToRemove);
                }

                if (i == tileX - 1 && !yTunnels.Contains(j))
                {
                    GameObject pipeToRemove = tileGrid[i, j].EastPipe;
                    pipeToRemove.SetActive(false);
                    tileGrid[i, j].pipes.Remove(pipeToRemove);
                }

                if (j == 0 && !xTunnels.Contains(i))
                {
                    GameObject pipeToRemove = tileGrid[i, j].SouthPipe;
                    pipeToRemove.SetActive(false);
                    tileGrid[i, j].pipes.Remove(pipeToRemove);
                }

                if (j == tileY - 1 && !xTunnels.Contains(i))
                {
                    GameObject pipeToRemove = tileGrid[i, j].NorthPipe;
                    pipeToRemove.SetActive(false);
                    tileGrid[i, j].pipes.Remove(pipeToRemove);
                }
            }
    }

    private void PlaceSources()
    {
        for (int i = 0; i < sourcesCount && sources.Count < tileX * tileY; i++)
        {
            int x, y;
            do
            {
                x = Random.Range(0, tileX);
                y = Random.Range(0, tileY);
            } while (sources.Any(s => s.x == x && s.y == y));

            tileGrid[x, y].MakeSource(useSourceColors ? sourceColors[i] : new Color(0f, 0.5f, 1f));
            sources.Add(tileGrid[x, y]);
        }
    }

    private void RemoveConnections()
    {
        List<Tile> validTiles = new List<Tile>();
        foreach (Tile tile in tileGrid)
        {
            if (tile.ConnectedTiles.Count > 1)
                validTiles.Add(tile);
        }

        int removedConnections = 0;
        int failedAttempts = 0;
        while (validTiles.Count > 2 && removedConnections < connectionsToRemove && failedAttempts < removeAttempts)
        {
            Tile currentTile = validTiles[Random.Range(0, validTiles.Count)];
            Tile otherTile = currentTile.ConnectedTiles[Random.Range(0, currentTile.ConnectedTiles.Count)];

            if (!validTiles.Contains(otherTile) || currentTile.TunnelConnections.Contains(otherTile))
            {
                failedAttempts++;
                continue;
            }

            GameObject currentPipe, otherPipe;
            if (currentTile.x < otherTile.x)
            {
                currentPipe = currentTile.EastPipe;
                otherPipe = otherTile.WestPipe;
            }
            else if (currentTile.x > otherTile.x)
            {
                currentPipe = currentTile.WestPipe;
                otherPipe = otherTile.EastPipe;
            }
            else if (currentTile.y < otherTile.y)
            {
                currentPipe = currentTile.NorthPipe;
                otherPipe = otherTile.SouthPipe;
            }
            else
            {
                currentPipe = currentTile.SouthPipe;
                otherPipe = otherTile.NorthPipe;
            }

            currentPipe.SetActive(false);
            currentTile.pipes.Remove(currentPipe);

            otherPipe.SetActive(false);
            otherTile.pipes.Remove(otherPipe);

            if (!LevelValid())
            {
                currentPipe.SetActive(true);
                currentTile.pipes.Add(currentPipe);

                otherPipe.SetActive(true);
                otherTile.pipes.Add(otherPipe);

                failedAttempts++;
                continue;
            }

            removedConnections++;
            failedAttempts = 0;

            if (currentTile.ConnectedTiles.Count < 2)
                validTiles.Remove(currentTile);
            if (otherTile.ConnectedTiles.Count < 2)
                validTiles.Remove(otherTile);
        }
    }

    private bool LevelValid()
    {
        List<Tile> tilesToCheck = new List<Tile>();
        foreach (Tile tile in tileGrid)
        {
            tilesToCheck.Add(tile);
        }
        foreach (Tile source in sources)
        {
            CheckConnections(source, tilesToCheck);
        }
        return !tilesToCheck.Any();
    }

    private void CheckConnections(Tile currentTile, List<Tile> tilesToCheck)
    {
        if (tilesToCheck.Contains(currentTile))
        {
            tilesToCheck.Remove(currentTile);
            foreach (Tile tile in currentTile.ConnectedTiles)
            {
                CheckConnections(tile, tilesToCheck);
            }
        }
    }

    private void PlaceLocks()
    {
        for (int i = 0; i < locksCount && i < tileX * tileY; i++)
        {
            int x, y;
            do
            {
                x = Random.Range(0, tileX);
                y = Random.Range(0, tileY);
            } while (tileGrid[x,y].Locked);

            tileGrid[x, y].MakeLock();
        }
    }

    private void RotateTiles()
    {
        foreach (Tile tile in tileGrid)
        {
            if (!tile.Locked)
            {
                float randomRotation = Random.Range(0, 4) * 90;
                tile.transform.eulerAngles = new Vector3(0, randomRotation, 0);
            }   

            tile.targetRotation = tile.transform.eulerAngles.y;
        }
    }

    public void UpdateSourceConnections()
    {
        bool winning = true;
        List<Tile> allTiles = new List<Tile>();
        foreach (Tile tile in tileGrid)
        {
            tile.connectedColors = new List<Color>();
            allTiles.Add(tile);
        }

        foreach (Tile source in sources)
        {
            List<Tile> tilesToUpdate = new List<Tile>();
            foreach (Tile tile in tileGrid)
                tilesToUpdate.Add(tile);
            UpdateConnections(source, tilesToUpdate, source.sourceColor, ref winning);
        }

        if (winning && allTiles.All(tile => !tile.HasLeaks && tile.connectedColors.Any()))
        {
            winText.SetActive(true);
            hasWon = true;
            winSfxSource.Play();
        }
    }

    private void UpdateConnections(Tile currentTile, List<Tile> tilesToUpdate, Color sourceColor, ref bool winning)
    {
        if (!currentTile.Idle || currentTile.HasLeaks)
            winning = false;
        if ((currentTile.Idle || currentTile.source) && !currentTile.connectedColors.Contains(sourceColor))
            currentTile.connectedColors.Add(sourceColor);
        if (tilesToUpdate.Contains(currentTile) && currentTile.Idle)
        {
            tilesToUpdate.Remove(currentTile);
            foreach (Tile tile in currentTile.ConnectedTiles)
            {
                UpdateConnections(tile, tilesToUpdate, sourceColor, ref winning);
            }
        }
    }

    private void Update()
    {
        if (hasWon && Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject() && !menu.activeInHierarchy)
        {
            ToggleMenu();
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }
    }

    public void ToggleMenu()
    {
        menu.SetActive(!menu.activeInHierarchy);
    }

    public void HintButton()
    {
        List<Tile> unlockedTiles = new List<Tile>();
        foreach (Tile tile in tileGrid)
            if (CheckTileHint(tile))
                unlockedTiles.Add(tile);

        if (unlockedTiles.Any())
            unlockedTiles[Random.Range(0, unlockedTiles.Count)].Solve();
    }

    private bool CheckTileHint(Tile tile)
    {
        return
            !tile.Locked &&
            tile.targetRotation != tile.OriginalRotation &&
            tile.pipes.Count < 4 &&
            !(tile.pipes.Count == 2 &&
            ((tile.North && tile.South) || tile.West && tile.East) &&
            tile.targetRotation == (tile.OriginalRotation + 180) % 360);
    }
}
