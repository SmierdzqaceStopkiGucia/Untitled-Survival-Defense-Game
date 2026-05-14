using UnityEngine;

public class BuildingData : MonoBehaviour
{
    public string buildingName;
    public Sprite buildingIcon;
    public float currentHP;
    public float maxHP;

    // You can call this from your combat scripts to reduce HP
    public void TakeDamage(float amount)
    {
        currentHP = Mathf.Max(0, currentHP - amount);
    }
}