using UnityEngine;

public class MineralDeposit : MonoBehaviour
{
    [SerializeField]
    private GameObject _mineralPrefab;

    [Header("Settings")]
    public float weakPointRadius = 0.4f;       // World-space radius of the weak point
    public float weakPointDamageMultiplier = 2.5f;
    public int maxHP = 100;

    [Header("Visuals")]
    public GameObject weakPointIndicatorPrefab;  // A glowing dot/decal

    private int currentHP;
    private Vector3 weakPointPosition;
    private bool isActive = false;
    private GameObject spawnedIndicator;
    public int PowerRequirement;
    public GameObject NextStage;
    
    
    void Start()
    {
        currentHP = maxHP;
    }
    
    public void OnHit(Vector3 hitPoint, Vector3 normal, int baseDamage)
    {
        if (!isActive)
        {
            SpawnWeakPoint();
        }

        int damage = baseDamage;

        if (isActive && Vector3.Distance(hitPoint, weakPointPosition) <= weakPointRadius)
        {
            damage = Mathf.RoundToInt(baseDamage * weakPointDamageMultiplier);
            SpawnWeakPoint();
        }

        currentHP -= damage;
        if (currentHP <= 0)
        {
            BreakRock(hitPoint, normal);
        }
    }

    void SpawnWeakPoint()
    {
        var localPoint = GetRandomSurfacePoint();
        weakPointPosition = transform.TransformPoint(localPoint);
        isActive = true;

        if (spawnedIndicator != null)
            Destroy(spawnedIndicator);

        if (weakPointIndicatorPrefab != null)
        {
            spawnedIndicator = Instantiate(
                weakPointIndicatorPrefab,
                weakPointPosition,
                Quaternion.LookRotation((weakPointPosition - transform.position).normalized)
            );
            spawnedIndicator.transform.SetParent(transform);
        }
    }

    Vector3 GetRandomSurfacePoint()
    {
        var mf = GetComponent<MeshFilter>();
        if (mf == null)
        {
            return Random.onUnitSphere * 0.5f;
        }

        var mesh = mf.sharedMesh;
        var vertices = mesh.vertices;

        var randomVertex = vertices[Random.Range(0, vertices.Length)];

        var outward = randomVertex.normalized;
        return randomVertex + outward * 0.01f;
    }

    void BreakRock(Vector3 hitPoint, Vector3 normal)
    {
        if (spawnedIndicator != null)
            Destroy(spawnedIndicator);
    
        var randomRotation = Quaternion.Euler(
            Random.Range(0f, 360f), 
            Random.Range(0f, 360f), 
            Random.Range(0f, 360f)
        );
        var mineral = Instantiate(_mineralPrefab, hitPoint, randomRotation);
        mineral.GetComponent<Rigidbody>().AddForce((normal + Vector3.up) * 2f, ForceMode.Impulse);

        if (NextStage)
        {
            NextStage.SetActive(true);
        }
        
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        if (isActive)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(weakPointPosition, weakPointRadius);
        }
    }
}
