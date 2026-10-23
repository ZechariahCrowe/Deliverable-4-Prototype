using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DroppedItem : MonoBehaviour, IInteractable
{
    private Renderer objRenderer;
    private MaterialPropertyBlock propBlock;
    
    private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");

    [Header("Glow Settings")]
    [ColorUsage(true, true)]
    [SerializeField] private Color glowColor = new Color(0.5f, 0.3f, 0.2f) * 2f; 

    [Header("Item Data")]
    public Item item;

    [Header("State")]
    public bool pickedUp = false;

    private void Awake()
    {
        propBlock = new MaterialPropertyBlock();
    }

    private void Start()
    {
        if (item != null)
        {
            Initialize(item);
        }
    }

    public void Initialize(Item item)
    {
        this.item = item;

        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        if (item != null && item.prefab != null)
        {
            GameObject visual = Instantiate(item.prefab, transform);
            visual.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            objRenderer = visual.GetComponentInChildren<Renderer>();
        }
    }

    // --- IInteractable Implementation ---

public void Interact()
{
    if (pickedUp || item == null) return;

    if (Inventory.Instance != null)
    {
        pickedUp = true;
        Inventory.Instance.AddItem(item);
        Destroy(gameObject);
    }
    else
    {
        Debug.LogWarning("[DroppedItem] No Inventory instance found in the scene!");
    }
}

    public string GetDescription()
    {
        return item != null ? $"Pick up {item.name}" : "Pick up Item";
    }

public void Highlight() 
{
    if (objRenderer == null) return;

    propBlock ??= new MaterialPropertyBlock();

    objRenderer.GetPropertyBlock(propBlock);
    propBlock.SetColor(EmissionColorID, glowColor);
    objRenderer.SetPropertyBlock(propBlock);
}

public void Unhighlight() 
{
    if (objRenderer == null) return;

    propBlock ??= new MaterialPropertyBlock();

    objRenderer.GetPropertyBlock(propBlock);
    propBlock.SetColor(EmissionColorID, Color.black); // Turns off emission
    objRenderer.SetPropertyBlock(propBlock);
}
}