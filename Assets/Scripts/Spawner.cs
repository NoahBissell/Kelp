using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject prefab;
    public float radius;
    public int amount;
    public Vector2 scaleRange;
    public LayerMask terrainMask;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Spawn()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < amount; i++)
        {
            var kelp = Instantiate(prefab, transform, true) as GameObject;
            Vector2 xyPos = new Vector2(transform.position.x, transform.position.z) + Random.insideUnitCircle * radius;
            Ray ray = new Ray(new Vector3(xyPos.x, 0, xyPos.y), Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit hit, 1000, terrainMask))
            {
                kelp.transform.position = hit.point;
                kelp.transform.localScale = Vector3.one * Random.Range(scaleRange.x, scaleRange.y);
            }
        }
    }
}
