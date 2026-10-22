using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class NightVisionController : MonoBehaviour
{
    [SerializeField] private PacStudentController player;
    [SerializeField] private Transform manualLevel;
    [SerializeField] private GhostStateController[] ghosts;
    [SerializeField] private CherryController cherryController;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private Text lightningText;

    [SerializeField] private float nearbyDistance = 2f;
    [SerializeField] private float forwardDistance = 6f;
    [SerializeField] private float halfViewAngle = 45f;

    private SpriteRenderer[] mapSprites;
    private Renderer[][] ghostRenderers;
    private Canvas[][] ghostLabels;
    private GameObject lastCherry;
    private SpriteRenderer cherryGlow;
    private static Sprite glowSprite;
    private float flashTime;
    private int charges = 3;

    public int Charges => charges;
    public float FlashRemaining => flashTime;

    private void Start()
    {
        mapSprites = manualLevel.GetComponentsInChildren<SpriteRenderer>(true);
        ghostRenderers = new Renderer[ghosts.Length][];
        ghostLabels = new Canvas[ghosts.Length][];

        for (int i = 0; i < ghosts.Length; i++)
        {
            ghostRenderers[i] = ghosts[i].GetComponentsInChildren<Renderer>(true);
            ghostLabels[i] = ghosts[i].GetComponentsInChildren<Canvas>(true);
        }

        UpdateLightningText();
        UpdateVisibility();
    }

    private void Update()
    {
        if (gameManager.IsDeathSequenceActive || gameManager.IsGameOver)
            flashTime = 0f;
        else if (flashTime > 0f)
            flashTime = Mathf.Max(0f, flashTime - Time.deltaTime);

        Keyboard keyboard = Keyboard.current;
        if (gameManager.CanInteract && keyboard != null &&
            keyboard.spaceKey.wasPressedThisFrame && charges > 0 && flashTime <= 0f)
        {
            charges--;
            flashTime = 2f;
            UpdateLightningText();
        }
    }

    private void LateUpdate()
    {
        UpdateVisibility();
        UpdateCherryGlow();
    }

    public void AddCharge()
    {
        if (charges >= 3) return;

        charges++;
        UpdateLightningText();
    }

    private void UpdateLightningText()
    {
        if (lightningText != null)
            lightningText.text = "LIGHTNING " + charges + "/3\nSPACE";
    }

    private bool CanSee(Vector3 position)
    {
        if (flashTime > 0f) return true;

        Vector2 difference = position - player.transform.position;
        float distance = difference.magnitude;
        if (distance <= nearbyDistance) return true;
        if (distance > forwardDistance) return false;

        return Vector2.Angle(player.FacingDirection, difference) <= halfViewAngle;
    }

    private void UpdateVisibility()
    {
        if (mapSprites == null) return;

        foreach (SpriteRenderer sprite in mapSprites)
        {
            bool visible = CanSee(sprite.transform.position);
            if (sprite.enabled != visible)
                sprite.enabled = visible;
        }

        for (int i = 0; i < ghosts.Length; i++)
        {
            bool visible = CanSee(ghosts[i].transform.position);
            foreach (Renderer part in ghostRenderers[i])
                if (part.enabled != visible) part.enabled = visible;
            foreach (Canvas label in ghostLabels[i])
                if (label.enabled != visible) label.enabled = visible;
        }
    }

    private Sprite GetGlowSprite()
    {
        if (glowSprite != null) return glowSprite;

        Texture2D texture = new Texture2D(32, 32);
        texture.filterMode = FilterMode.Bilinear;
        for (int y = 0; y < 32; y++)
        {
            for (int x = 0; x < 32; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(15.5f, 15.5f));
                float alpha = Mathf.Clamp01(1f - distance / 16f);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha * alpha));
            }
        }
        texture.Apply();
        glowSprite = Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32f);
        return glowSprite;
    }

    private void UpdateCherryGlow()
    {
        if (cherryController == null) return;

        GameObject cherry = cherryController.ActiveBonus;
        if (cherry != lastCherry)
        {
            lastCherry = cherry;
            cherryGlow = null;
            if (cherry != null)
            {
                SpriteRenderer sprite = cherry.GetComponent<SpriteRenderer>();
                if (sprite != null)
                {
                    GameObject glow = new GameObject("Cherry Glow");
                    glow.transform.SetParent(cherry.transform, false);
                    glow.transform.localScale = Vector3.one * 2.8f;
                    cherryGlow = glow.AddComponent<SpriteRenderer>();
                    cherryGlow.sprite = GetGlowSprite();
                    cherryGlow.sortingLayerID = sprite.sortingLayerID;
                    cherryGlow.sortingOrder = sprite.sortingOrder - 1;
                    cherryGlow.maskInteraction = sprite.maskInteraction;
                }
            }
        }

        if (cherryGlow != null)
        {
            float alpha = 0.55f + 0.15f * Mathf.Sin(Time.time * 4f);
            cherryGlow.color = new Color(1f, 0.65f, 0.25f, alpha);
        }
    }
}
