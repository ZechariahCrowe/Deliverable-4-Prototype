using UnityEngine;

public class IntColorChanger : MonoBehaviour, IInteractable 
{
    private Renderer objRenderer;
    private MaterialPropertyBlock propBlock;
    
    // Shader property IDs for efficiency
    private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");

    [Header("Glow Settings")]
    [ColorUsage(true, true)] // Enables HDR color picker in Inspector for bloom
    [SerializeField] private Color glowColor = new Color(0f, 0.8f, 0.8f) * 2f; 

    private void Awake() 
    {
        objRenderer = GetComponent<Renderer>();
        propBlock = new MaterialPropertyBlock();
    }

    public string GetDescription() 
    {
        return "Flip rotation 90 degrees";
    }

public void Interact() 
{
    // Rotates the object 180 degrees on its Z axis
    transform.Rotate(0f, 0f, 90f, Space.Self); 
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