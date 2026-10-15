using UnityEngine;
using System.Collections.Generic;

public class PlayerInteractionController : MonoBehaviour
{

    [SerializeField] private Transform manualLevel;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameSoundEffects soundEffects;
    [SerializeField] private CherryController cherryController;

    private readonly Dictionary<Vector2Int, GameObject> normalFoodsByCell = new();
    private readonly Dictionary<Vector2Int, GameObject> powerFoodsByCell = new();

    private void Awake()
    {
        if (manualLevel == null) return;

        foreach (SpriteRenderer sprite in manualLevel.GetComponentsInChildren<SpriteRenderer>())
        {
            string objectName = sprite.gameObject.name;
            Vector2Int key = CellKey(sprite.transform.position);

            if (objectName.EndsWith("_Type05"))
                normalFoodsByCell[key] = sprite.gameObject;
            else if (objectName.EndsWith("_Type06"))
                powerFoodsByCell[key] = sprite.gameObject;
        }
    }

    public void CollectAt(Vector3 cellCenter)
    {
        Vector2Int key = CellKey(cellCenter);

        if (normalFoodsByCell.TryGetValue(key, out GameObject food) &&
            food != null && food.activeInHierarchy)
        {
            normalFoodsByCell.Remove(key);
            food.SetActive(false);

            if (gameManager != null)
                gameManager.AddScore(10);
            if (soundEffects != null)
                soundEffects.PlayEatCrumb();
            return;
        }

        if (!powerFoodsByCell.TryGetValue(key, out GameObject powerFood) ||
            powerFood == null || !powerFood.activeInHierarchy)
            return;

        powerFoodsByCell.Remove(key);
        powerFood.SetActive(false);

        if (gameManager != null)
            gameManager.AddScore(50);
        if (soundEffects != null)
            soundEffects.PlayEatCrumb();
    }

    private Vector2Int CellKey(Vector3 position)
    {
        return new Vector2Int(
            Mathf.RoundToInt(position.x * 2f),
            Mathf.RoundToInt(position.y * 2f)
        );
    }

    void Start()
    {

    }

    private void Update()
    {
        if (cherryController == null) return;

        GameObject bonus = cherryController.ActiveBonus;
        if (bonus == null) return;
        if (Vector2.Distance(transform.position, bonus.transform.position) > 0.5f) return;
        if (!cherryController.TryCollectBonus()) return;

        if (gameManager != null)
            gameManager.AddScore(100);

        if (soundEffects != null)
            soundEffects.PlayEatBonus();
    }
}
