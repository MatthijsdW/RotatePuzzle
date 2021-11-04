using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public float moveSpeed;
    public float rotationSpeed;
    public float raiseDistance;
    public float fillSpeed;
    public GameObject center;
    public AudioSource audioSource;

    [HideInInspector]
    public int x, y;

    [HideInInspector]
    public float targetRotation;
    private float originalRotation;
    private bool isRaised;
    private float baseY;
    private float raisedY;

    [HideInInspector]
    public bool source;
    [HideInInspector]
    public Color sourceColor;
    private bool locked;
    [HideInInspector]
    public List<Color> connectedColors;
    [HideInInspector]
    public List<GameObject> pipes;

    private Color empty = new Color(0.5f, 0.5f, 0.5f);

    private const int northAngle = 0;
    private const int eastAngle = 90;
    private const int southAngle = 180;
    private const int westAngle = 270;

    [HideInInspector]
    public GameManager gameManager;

    private void Awake()
    {
        originalRotation = Random.Range(0, 4) * 90;
        transform.eulerAngles = new Vector3(0, originalRotation, 0);

        foreach (Transform child in transform)
        {
            if (child.gameObject.CompareTag("Pipe"))
            {
                pipes.Add(child.gameObject);
            }
        }
    }

    private void Start()
    {
        baseY = transform.position.y;
        raisedY = baseY + raiseDistance;
    }

    private void Update()
    {
        UpdateColor();
        RotateTile();
        UpdatePipes();
    }

    private void UpdateColor()
    {
        Color targetColor = empty;
        if (connectedColors.Any())
        {
            Color totalColor = new Color(0, 0, 0, 0);
            foreach (Color color in connectedColors)
            {
                totalColor += color;
            }
            targetColor = totalColor;
        }
        List<Renderer> renderers = new List<Renderer>();
        if (!source)
            foreach (Renderer renderer in center.GetComponentsInChildren<Renderer>())
            {
                renderers.Add(renderer);
            }
        foreach (GameObject pipe in pipes)
        {
            Renderer renderer = pipe.GetComponent<Renderer>();
            renderers.Add(renderer);
        }

        if (!renderers.Any())
            return;

        Color currentColor = renderers[0].material.GetColor("_Color");

        foreach (GameObject pipe in pipes)
        {
            ParticleSystem ps = pipe.transform.GetComponentInChildren<ParticleSystem>();
            var main = ps.main;
            main.startColor = currentColor;
            if (!ps.isPlaying)
                ps.Play();
        }
        if (currentColor == targetColor)
        {
            if (!connectedColors.Any())
                foreach(GameObject pipe in pipes)
                {
                    ParticleSystem ps = pipe.transform.GetComponentInChildren<ParticleSystem>();
                    if (ps.isPlaying)
                        ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                }
            return;
        }

        Color nextColor = new Color(
            Mathf.MoveTowards(currentColor.r, targetColor.r, fillSpeed * Time.deltaTime),
            Mathf.MoveTowards(currentColor.g, targetColor.g, fillSpeed * Time.deltaTime),
            Mathf.MoveTowards(currentColor.b, targetColor.b, fillSpeed * Time.deltaTime));

        foreach (Renderer renderer in renderers)
        {
            renderer.material.SetColor("_Color", nextColor);
        }
    }

    private void UpdatePipes()
    {
        List<ParticleSystem> particleSystemsToDisable = new List<ParticleSystem>();
        if (x != 0 && West && GameManager.tileGrid[x - 1, y].East)
            particleSystemsToDisable.Add(WestPipe.transform.GetComponentInChildren<ParticleSystem>());
        if (x != GameManager.tileGrid.GetLength(0) - 1 && East && GameManager.tileGrid[x + 1, y].West)
            particleSystemsToDisable.Add(EastPipe.transform.GetComponentInChildren<ParticleSystem>());
        if (y != 0 && South && GameManager.tileGrid[x, y - 1].North)
            particleSystemsToDisable.Add(SouthPipe.transform.GetComponentInChildren<ParticleSystem>());
        if (y != GameManager.tileGrid.GetLength(1) - 1 && North && GameManager.tileGrid[x, y + 1].South)
            particleSystemsToDisable.Add(NorthPipe.transform.GetComponentInChildren<ParticleSystem>());

        foreach(ParticleSystem ps in particleSystemsToDisable)
            if (ps.isPlaying)
                ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    private void RotateTile()
    {
        if (isRaised)
        {
            if (transform.eulerAngles.y != targetRotation)
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetRotation, Time.deltaTime * rotationSpeed), transform.eulerAngles.z);
            else
            {
                if (transform.position.y == baseY)
                {
                    audioSource.Stop();
                    isRaised = false;
                    gameManager?.UpdateSourceConnections();
                    if (locked)
                        transform.Find("Locks").gameObject.SetActive(true);
                }
                else
                    transform.position = new Vector3(transform.position.x, Mathf.MoveTowards(transform.position.y, baseY, Time.deltaTime * moveSpeed), transform.position.z);
            }
        }
        else
        {
            if (transform.eulerAngles.y != targetRotation)
            {
                if (!audioSource.isPlaying)
                    audioSource.Play();
                if (transform.position.y == raisedY)
                    isRaised = true;
                else
                    transform.position = new Vector3(transform.position.x, Mathf.MoveTowards(transform.position.y, raisedY, Time.deltaTime * moveSpeed), transform.position.z);
            }
            else
                transform.position = new Vector3(transform.position.x, Mathf.MoveTowards(transform.position.y, baseY, Time.deltaTime * moveSpeed), transform.position.z);
        }
    }

    public void LeftClick()
    {
        if (locked)
            return;
        targetRotation -= 90;
        if (targetRotation < 0) targetRotation += 360;
        targetRotation %= 360;
        gameManager?.UpdateSourceConnections();
    }

    public void RightClick()
    {
        if (locked)
            return;
        targetRotation += 90;
        targetRotation %= 360;
        gameManager?.UpdateSourceConnections();
    }

    public void MakeSource(Color sourceColor)
    {
        foreach (Renderer renderer in center.GetComponentsInChildren<Renderer>())
            renderer.material.SetColor("_Color", sourceColor);
        this.sourceColor = sourceColor;
        source = true;
        transform.GetChild(0).localScale *= 2;
    }

    public void MakeLock()
    {
        locked = true;
        transform.Find("Locks").gameObject.SetActive(true);
    }

    public void Solve()
    {
        targetRotation = originalRotation;
        locked = true;
    }

    public bool North { get { return pipes.Any(x => x.transform.rotation.eulerAngles.y < northAngle + 1); } }
    public bool East { get { return pipes.Any(x => x.transform.rotation.eulerAngles.y == eastAngle); } }
    public bool South { get { return pipes.Any(x => x.transform.rotation.eulerAngles.y == southAngle); } }
    public bool West { get { return pipes.Any(x => x.transform.rotation.eulerAngles.y == westAngle); } }

    public GameObject NorthPipe { get { return pipes.Find(x => x.transform.eulerAngles.y < northAngle + 1); } }
    public GameObject EastPipe { get { return pipes.Find(x => x.transform.eulerAngles.y == eastAngle); } }
    public GameObject SouthPipe { get { return pipes.Find(x => x.transform.eulerAngles.y == southAngle); } }
    public GameObject WestPipe { get { return pipes.Find(x => x.transform.eulerAngles.y == westAngle); } }

    public bool Idle { get { return transform.position.y == baseY && transform.rotation.eulerAngles.y == targetRotation; } }

    public List<Tile> ConnectedTiles
    {
        get
        {
            List<Tile> value = TunnelConnections;
            int tileX = GameManager.tileGrid.GetLength(0);
            int tileY = GameManager.tileGrid.GetLength(1);

            if (x != 0 && West && GameManager.tileGrid[x - 1, y].East)
                value.Add(GameManager.tileGrid[x - 1 < 0 ? tileX - 1 : x - 1, y]);
            if (x != tileX - 1 && East && GameManager.tileGrid[x + 1, y].West)
                value.Add(GameManager.tileGrid[x + 1 == tileX ? 0 : x + 1, y]);
            if (y != 0 && South && GameManager.tileGrid[x, y - 1].North)
                value.Add(GameManager.tileGrid[x, y - 1 < 0 ? tileY - 1 : y - 1]);
            if (y != tileY - 1 && North && GameManager.tileGrid[x, y + 1].South)
                value.Add(GameManager.tileGrid[x, y + 1 == tileY ? 0 : y + 1]);

            return value;
        }
    }

    public List<Tile> TunnelConnections
    {
        get
        {
            List<Tile> value = new List<Tile>();
            int tileX = GameManager.tileGrid.GetLength(0);
            int tileY = GameManager.tileGrid.GetLength(1);

            if (x == 0 && West && GameManager.yTunnels.Contains(y) && GameManager.tileGrid[tileX - 1, y].East)
                value.Add(GameManager.tileGrid[tileX - 1, y]);
            if (x == tileX - 1 && East && GameManager.yTunnels.Contains(y) && GameManager.tileGrid[0, y].West)
                value.Add(GameManager.tileGrid[0, y]);
            if (y == 0 && South && GameManager.xTunnels.Contains(x) && GameManager.tileGrid[x, tileY - 1].North)
                value.Add(GameManager.tileGrid[x, tileY - 1]);
            if (y == tileY - 1 && North && GameManager.xTunnels.Contains(x) && GameManager.tileGrid[x, 0].South)
                value.Add(GameManager.tileGrid[x, 0]);

            return value;
        }
    }
    
    public bool Locked
    {
        get
        {
            return locked;
        }
    }
    public bool HasLeaks
    {
        get
        {
            return ConnectedTiles.Count != pipes.Count;
        }
    }

    public float OriginalRotation
    {
        get
        {
            return originalRotation;
        }
    }
}
