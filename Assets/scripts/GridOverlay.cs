using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class GridOverlay : MonoBehaviour
{
    public KeyCode toggleKey = KeyCode.G;

    [System.Serializable]
    public struct BuildCost
    {
        public string prefabName;
        public float wood;
        public float stone;
        public float iron;
    }

    [Header("HUD Animation Settings")]
    public RectTransform mainHUD;
    public RectTransform buildingHUD;
    public Vector2 hudActivePos;
    public Vector2 hudOffscreenPos;
    public float hudMoveSpeed = 8f;

    [Header("Economy Settings")]
    public BuildCost[] costs;
    public TextMeshPro costDisplayText;
    public AudioClip insufficientResourcesSound;

    [Header("Grid Settings")]
    public int cellSizePixels = 256;
    public float pixelsPerUnit = 100f;
    public int gridWidth = 10;
    public int gridHeight = 10;

    [Header("Placement Settings")]
    public GameObject[] placeablePrefabs;
    private int selectedIndex = -1;
    private Dictionary<Vector2Int, GameObject> spawnedObjects = new Dictionary<Vector2Int, GameObject>();

    [Header("Rendering")]
    public string sortingLayerName = "UI";
    public int orderInLayer = 10;
    public Color gridColor = new Color(1f, 1f, 1f, 0.5f);

    [Header("Fade Settings")]
    public float fadeSpeed = 4f;
    private float currentFadeAlpha = 0f;
    private List<LineRenderer> cachedLineRenderers = new List<LineRenderer>();

    [Header("Audio")]
    public AudioClip toggleOnSound;
    public AudioClip toggleOffSound;
    public AudioClip switchSelectionSound;
    public AudioClip placeSound;
    public AudioClip destroySound;
    private AudioSource audioSource;

    [Header("Hover Highlight")]
    public Color hoverColor = new Color(1f, 1f, 1f, 0.2f);
    private GameObject hoverCell;
    private MeshRenderer hoverRenderer;
    private SpriteRenderer hoverPreviewRenderer;

    [Header("Hover Animation")]
    public float hoverPulseSpeed = 5f;
    public float hoverMinAlpha = 0.1f;
    public float hoverMaxAlpha = 0.3f;
    public float previewAlphaMultiplier = 0.5f;

    [Header("Range Preview Settings")]
    public Color rangeTileColor = new Color(1f, 0f, 0f, 0.2f);
    private List<GameObject> activeRangeTiles = new List<GameObject>();
    private Vector2Int lastPreviewPos = new Vector2Int(-999, -999);

    private GameObject gridObject;
    private bool isVisible = false;
    private float cellSize;
    private UIManager uiManager;

    void Start()
    {
        cellSize = (float)cellSizePixels / pixelsPerUnit;
        audioSource = gameObject.AddComponent<AudioSource>();
        uiManager = Object.FindObjectOfType<UIManager>();

        CreateGrid();
        CreateHoverCell();

        gridObject.SetActive(false);
        hoverCell.SetActive(false);
        if (costDisplayText) costDisplayText.gameObject.SetActive(false);

        selectedIndex = -1;

        if (mainHUD) mainHUD.anchoredPosition = hudActivePos;
        if (buildingHUD) buildingHUD.anchoredPosition = hudOffscreenPos;
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            isVisible = !isVisible;
            if (isVisible) audioSource.PlayOneShot(toggleOnSound);
            else
            {
                audioSource.PlayOneShot(toggleOffSound);
                ClearRangePreview();
            }
        }

        HandleSelectionInput();
        AnimateHUD();

        float targetAlpha = isVisible ? 1f : 0f;
        currentFadeAlpha = Mathf.MoveTowards(currentFadeAlpha, targetAlpha, Time.deltaTime * fadeSpeed);

        bool shouldBeActive = currentFadeAlpha > 0.001f;
        if (gridObject.activeSelf != shouldBeActive)
        {
            gridObject.SetActive(shouldBeActive);
            hoverCell.SetActive(shouldBeActive);
        }

        if (shouldBeActive)
        {
            UpdateGridPosition();
            UpdateGridAlpha();
            UpdateHoverCell();
            HandlePlacement();
            UpdateCostDisplay();
            UpdateRangeAnimation();
        }
        else if (costDisplayText)
        {
            costDisplayText.gameObject.SetActive(false);
        }
    }

    void ShowRangePreview(Vector3 centerPos, float range)
    {
        ClearRangePreview();
        int radiusInCells = Mathf.RoundToInt(range / cellSize);

        for (int x = -radiusInCells; x <= radiusInCells; x++)
        {
            for (int y = -radiusInCells; y <= radiusInCells; y++)
            {
                if (x * x + y * y <= radiusInCells * radiusInCells)
                {
                    Vector3 tilePos = centerPos + new Vector3(x * cellSize, y * cellSize, 0);
                    CreateRangeTile(tilePos);
                }
            }
        }
    }

    void CreateRangeTile(Vector3 pos)
    {
        GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Quad);
        Destroy(tile.GetComponent<MeshCollider>());
        tile.transform.position = pos;
        tile.transform.localScale = new Vector3(cellSize, cellSize, 1f);

        MeshRenderer mr = tile.GetComponent<MeshRenderer>();
        mr.material = new Material(Shader.Find("Sprites/Default"));
        mr.material.color = rangeTileColor;
        mr.sortingLayerName = sortingLayerName;
        mr.sortingOrder = orderInLayer - 1;

        activeRangeTiles.Add(tile);
    }

    void UpdateRangeAnimation()
    {
        float pulse = Mathf.Sin(Time.time * hoverPulseSpeed) * 0.5f + 0.5f;
        float masterAlpha = Mathf.Lerp(hoverMinAlpha, hoverMaxAlpha, pulse) * currentFadeAlpha;

        foreach (GameObject tile in activeRangeTiles)
        {
            MeshRenderer mr = tile.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                Color c = rangeTileColor;
                c.a = masterAlpha;
                mr.material.color = c;
            }
        }
    }

    public void ClearRangePreview()
    {
        foreach (GameObject tile in activeRangeTiles) Destroy(tile);
        activeRangeTiles.Clear();
        lastPreviewPos = new Vector2Int(-999, -999);
    }

    void UpdateCostDisplay()
    {
        if (costDisplayText == null || uiManager == null) return;

        if (selectedIndex >= 0 && selectedIndex < costs.Length)
        {
            costDisplayText.gameObject.SetActive(true);
            BuildCost cost = costs[selectedIndex];

            string text = "Cost: ";
            if (cost.wood > 0) text += $"{cost.wood} ";
            if (cost.stone > 0) text += $"{cost.stone} ";
            if (cost.iron > 0) text += $"{cost.iron}";

            costDisplayText.text = text;

            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            costDisplayText.transform.position = mousePos + new Vector3(0, 0.8f, 0);

            bool canAfford = (uiManager.wood >= cost.wood && uiManager.stone >= cost.stone && uiManager.iron >= cost.iron);
            costDisplayText.color = canAfford ? Color.white : Color.red;
        }
        else
        {
            costDisplayText.gameObject.SetActive(false);
        }
    }

    void HandlePlacement()
    {
        if (Camera.main == null || uiManager == null) return;

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int gridPos = new Vector2Int(
            Mathf.FloorToInt(mouseWorld.x / cellSize),
            Mathf.FloorToInt(mouseWorld.y / cellSize)
        );

        if (Input.GetMouseButtonDown(0) && selectedIndex != -1)
        {
            if (selectedIndex < costs.Length)
            {
                BuildCost cost = costs[selectedIndex];

                if (uiManager.wood >= cost.wood && uiManager.stone >= cost.stone && uiManager.iron >= cost.iron)
                {
                    if (!spawnedObjects.ContainsKey(gridPos))
                    {
                        uiManager.wood -= cost.wood;
                        int stoneCostInt = (int)cost.stone;
                        uiManager.stone -= stoneCostInt;
                        uiManager.iron -= cost.iron;

                        Vector3 spawnPos = new Vector3((gridPos.x * cellSize) + (cellSize / 2f), (gridPos.y * cellSize) + (cellSize / 2f), 0);
                        GameObject newObj = Instantiate(placeablePrefabs[selectedIndex], spawnPos, Quaternion.identity);
                        spawnedObjects.Add(gridPos, newObj);

                        if (placeSound) audioSource.PlayOneShot(placeSound);
                    }
                }
                else
                {
                    if (insufficientResourcesSound) audioSource.PlayOneShot(insufficientResourcesSound);
                }
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            if (spawnedObjects.ContainsKey(gridPos))
            {
                Destroy(spawnedObjects[gridPos]);
                spawnedObjects.Remove(gridPos);
                if (destroySound) audioSource.PlayOneShot(destroySound);
            }
        }
    }

    void AnimateHUD()
    {
        if (!mainHUD || !buildingHUD) return;

        Vector2 mainTarget = isVisible ? hudOffscreenPos : hudActivePos;
        Vector2 buildTarget = isVisible ? hudActivePos : hudOffscreenPos;

        mainHUD.anchoredPosition = Vector2.Lerp(mainHUD.anchoredPosition, mainTarget, Time.deltaTime * hudMoveSpeed);
        buildingHUD.anchoredPosition = Vector2.Lerp(buildingHUD.anchoredPosition, buildTarget, Time.deltaTime * hudMoveSpeed);
    }

    void HandleSelectionInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            selectedIndex = -1;
            UpdatePreviewSprite();
            ClearRangePreview();
        }

        for (int i = 1; i <= 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i))
            {
                int indexToTry = i - 1;
                if (placeablePrefabs != null && indexToTry < placeablePrefabs.Length)
                {
                    selectedIndex = indexToTry;
                    if (switchSelectionSound) audioSource.PlayOneShot(switchSelectionSound);
                    UpdatePreviewSprite();
                }
            }
        }
    }

    void UpdatePreviewSprite()
    {
        if (selectedIndex == -1 || placeablePrefabs[selectedIndex] == null)
        {
            hoverPreviewRenderer.sprite = null;
            return;
        }

        SpriteRenderer prefabRenderer = placeablePrefabs[selectedIndex].GetComponent<SpriteRenderer>();
        if (prefabRenderer != null)
        {
            hoverPreviewRenderer.sprite = prefabRenderer.sprite;
        }
    }

    void UpdateGridAlpha()
    {
        Color targetColor = gridColor;
        targetColor.a *= currentFadeAlpha;
        foreach (var lr in cachedLineRenderers)
        {
            lr.startColor = targetColor;
            lr.endColor = targetColor;
        }
    }

    void UpdateGridPosition()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Floor(pos.x / cellSize) * cellSize;
        pos.y = Mathf.Floor(pos.y / cellSize) * cellSize;
        pos -= new Vector3((gridWidth * cellSize) / 2f, (gridHeight * cellSize) / 2f, 0);
        gridObject.transform.position = pos;
    }

    void CreateGrid()
    {
        gridObject = new GameObject("GridOverlay");
        for (int x = 0; x <= gridWidth; x++)
            CreateLine(new Vector3(x * cellSize, 0, 0), new Vector3(x * cellSize, gridHeight * cellSize, 0));
        for (int y = 0; y <= gridHeight; y++)
            CreateLine(new Vector3(0, y * cellSize, 0), new Vector3(gridWidth * cellSize, y * cellSize, 0));
    }

    void CreateLine(Vector3 start, Vector3 end)
    {
        GameObject lineObj = new GameObject("GridLine");
        lineObj.transform.parent = gridObject.transform;
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        cachedLineRenderers.Add(lr);
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        lr.startWidth = 0.02f;
        lr.endWidth = 0.02f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.sortingLayerName = sortingLayerName;
        lr.sortingOrder = orderInLayer;
        lr.useWorldSpace = false;
    }

    void CreateHoverCell()
    {
        hoverCell = GameObject.CreatePrimitive(PrimitiveType.Quad);
        Destroy(hoverCell.GetComponent<MeshCollider>());
        hoverCell.name = "HoverCell";
        hoverCell.transform.localScale = new Vector3(cellSize, cellSize, 1f);

        hoverRenderer = hoverCell.GetComponent<MeshRenderer>();
        hoverRenderer.material = new Material(Shader.Find("Sprites/Default"));
        hoverRenderer.sortingLayerName = sortingLayerName;
        hoverRenderer.sortingOrder = orderInLayer + 1;

        GameObject previewObj = new GameObject("PreviewSprite");
        previewObj.transform.parent = hoverCell.transform;
        previewObj.transform.localPosition = Vector3.zero;

        hoverPreviewRenderer = previewObj.AddComponent<SpriteRenderer>();
        hoverPreviewRenderer.sortingLayerName = sortingLayerName;
        hoverPreviewRenderer.sortingOrder = orderInLayer + 2;
    }

    void UpdateHoverCell()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float snappedX = Mathf.Floor(mouseWorld.x / cellSize) * cellSize;
        float snappedY = Mathf.Floor(mouseWorld.y / cellSize) * cellSize;

        Vector2Int currentGridPos = new Vector2Int((int)snappedX, (int)snappedY);
        hoverCell.transform.position = new Vector3(snappedX + cellSize / 2f, snappedY + cellSize / 2f, 0);

        if (selectedIndex != -1 && currentGridPos != lastPreviewPos)
        {
            lastPreviewPos = currentGridPos;
            TurretAI turret = placeablePrefabs[selectedIndex].GetComponent<TurretAI>();
            if (turret != null)
            {
                ShowRangePreview(hoverCell.transform.position, turret.range);
            }
            else
            {
                ClearRangePreview();
            }
        }

        float pulse = Mathf.Sin(Time.time * hoverPulseSpeed) * 0.5f + 0.5f;
        float masterAlpha = Mathf.Lerp(hoverMinAlpha, hoverMaxAlpha, pulse) * currentFadeAlpha;

        Color c = hoverColor;
        c.a = masterAlpha;
        hoverRenderer.material.color = c;

        if (hoverPreviewRenderer.sprite != null)
        {
            Color p = Color.white;
            p.a = masterAlpha * previewAlphaMultiplier;
            hoverPreviewRenderer.color = p;
        }
    }
}