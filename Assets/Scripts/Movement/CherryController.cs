using UnityEngine;
using System.Collections;

public class CherryController : MonoBehaviour
{
    [SerializeField] private GameObject bonusPrefab;
    [SerializeField] private GameObject activeBonus;
    public GameObject ActiveBonus => activeBonus;

    public bool TryCollectBonus()
    {
        if (activeBonus == null) return false;

        GameObject collected = activeBonus;
        activeBonus = null;
        Destroy(collected);
        return true;
    }
    private Vector3 endPoint;
    [SerializeField] private float bonusSpeed = 6f;

    void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        if (bonusPrefab == null)
          yield break; // Exit if the bonusPrefab is not assigned

        while (true)
        {
            yield return new WaitForSeconds(5f);
            Vector3 startPoint = ChooseStartPoint();
            endPoint = -startPoint.normalized * 35f;
            activeBonus = Instantiate(bonusPrefab, startPoint, Quaternion.identity);
            StartCoroutine(MoveBonus(startPoint, endPoint));
            Debug.DrawLine(startPoint, endPoint, Color.cyan, 5f);
            yield return new WaitUntil(() => activeBonus == null);
        }
    }
    private IEnumerator MoveBonus(Vector3 start, Vector3 target)
    {
        float duration = Vector3.Distance(start, target) / Mathf.Max(0.1f, bonusSpeed);
        float elapsed = 0f;

        while (elapsed < duration && activeBonus != null)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            activeBonus.transform.position = Vector3.Lerp(start, target, progress);
            yield return null;
        }

        if (activeBonus != null)
        {
            activeBonus.transform.position = target;
            Destroy(activeBonus);
        }
    }
    private Vector3 ChooseStartPoint()
    {
        int side = Random.Range(0, 4);
        float x = Random.Range(-13.5f, 13.5f);
        float y = Random.Range(-14f, 14f);

        if (side == 0) return new Vector3(-15.5f, y, 0f);
        if (side == 1) return new Vector3(15.5f, y, 0f);
        if (side == 2) return new Vector3(x, 16f, 0f);
        return new Vector3(x, -16f, 0f);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
