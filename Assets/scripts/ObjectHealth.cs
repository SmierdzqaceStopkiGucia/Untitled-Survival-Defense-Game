using UnityEngine;

public class ObjectHealth : MonoBehaviour
{
    public static Transform MainTarget;

    public bool isMainObject = false;
    public float localHealth = 50f;
    private UIManager uiManager;

    void Awake()
    {
        if (isMainObject)
        {
            MainTarget = this.transform;
        }
    }

    void Start()
    {
        uiManager = Object.FindObjectOfType<UIManager>();
    }

    public void TakeDamage(float amount)
    {
        if (isMainObject && uiManager != null)
        {
            uiManager.health -= amount;
        }
        else
        {
            localHealth -= amount;
            if (localHealth <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}