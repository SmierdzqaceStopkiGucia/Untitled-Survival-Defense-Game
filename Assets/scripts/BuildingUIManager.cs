using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems; // Required to check if we clicked UI

public class BuildingUIManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject infoPanel;
    public TextMeshProUGUI nameText;
    public Image iconImage;
    public Slider hpSlider;
    public TextMeshProUGUI hpText;

    private BuildingData selectedBuilding;

    void Start()
    {
        if (infoPanel) infoPanel.SetActive(false);
    }

    void Update()
    {
        // 1. Right Click Detection
        if (Input.GetMouseButtonDown(1))
        {
            HandleRightClick();
        }

        // 2. Left Click to Close (if not clicking the panel itself)
        if (Input.GetMouseButtonDown(0))
        {
            // If the panel is active and we didn't click on a UI element
            if (infoPanel.activeSelf && !EventSystem.current.IsPointerOverGameObject())
            {
                ClosePanel();
            }
        }

        // 3. Update HP dynamically if the panel is open
        if (infoPanel.activeSelf && selectedBuilding != null)
        {
            UpdateHPUI();
        }
    }

    void HandleRightClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

        if (hit.collider != null)
        {
            BuildingData data = hit.collider.GetComponent<BuildingData>();
            if (data != null)
            {
                OpenPanel(data);
                return;
            }
        }

        // If we right-clicked empty space, close the panel
        ClosePanel();
    }

    void OpenPanel(BuildingData data)
    {
        selectedBuilding = data;
        infoPanel.SetActive(true);

        if (nameText) nameText.text = data.buildingName;
        if (iconImage) iconImage.sprite = data.buildingIcon;

        UpdateHPUI();
    }

    void UpdateHPUI()
    {
        if (hpSlider)
        {
            hpSlider.maxValue = selectedBuilding.maxHP;
            hpSlider.value = selectedBuilding.currentHP;
        }

        if (hpText)
        {
            hpText.text = $"{Mathf.CeilToInt(selectedBuilding.currentHP)} / {selectedBuilding.maxHP}";
        }

        // If the building is destroyed while looking at it, close the panel
        if (selectedBuilding.currentHP <= 0)
        {
            ClosePanel();
        }
    }

    public void ClosePanel()
    {
        selectedBuilding = null;
        infoPanel.SetActive(false);
    }

}