using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI woodText;
    public TextMeshProUGUI stoneText;
    public TextMeshProUGUI ironText;
    public TextMeshProUGUI healthText;

    [Header("Data to Display")]
    public float wood = 0f;
    public int stone = 0;
    public float iron = 0f;
    public float health = 100f;

    void Update()
    {
        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        if (woodText) woodText.text = "Wood: " + wood.ToString("F0");
        if (stoneText) stoneText.text = "Stone: " + stone.ToString("F0");
        if (ironText) ironText.text = "Iron: " + iron.ToString("F0");
        if (healthText) healthText.text = "Health: " + health.ToString("F0");
    }
}