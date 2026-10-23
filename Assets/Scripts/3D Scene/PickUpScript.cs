using UnityEngine;

public class PickUpScript : MonoBehaviour, IInteractable 
{
    private Renderer objRenderer;
    private MaterialPropertyBlock propBlock;
    
    // Shader property IDs for efficiency
    private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");

    [Header("Glow Settings")]
    [ColorUsage(true, true)]
    [SerializeField] private Color glowColor = new Color(0.5f, 0.3f, 0.2f) * 2f; 

    private void Awake() 
    {
        objRenderer = GetComponent<Renderer>();
        propBlock = new MaterialPropertyBlock();
    }

    public string GetDescription() 
    {
        return "Pick Up.";
    }

public void Interact() 
{
    // Implementation for picking up the object
}

    public void Highlight() 
    {
        objRenderer.GetPropertyBlock(propBlock);
        propBlock.SetColor(EmissionColorID, glowColor);
        objRenderer.SetPropertyBlock(propBlock);
    }

    public void Unhighlight() 
    {
        objRenderer.GetPropertyBlock(propBlock);
        propBlock.SetColor(EmissionColorID, Color.black); // Turns off emission
        objRenderer.SetPropertyBlock(propBlock);
    }

    private void OnDisable()
    {
        Unhighlight();
    }
}