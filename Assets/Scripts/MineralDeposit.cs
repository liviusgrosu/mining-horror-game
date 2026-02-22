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
    private int previousHP;
    private Vector3 weakPointPosition;
    private bool isActive = false;
    private GameObject spawnedIndicator;
    public int PowerRequirement;
    public MeshFilter weaknessIndiciatorMeshFilter;
    void Start()
    {
        currentHP = maxHP;
    }
    
    public void OnHit(Vector3 hitPoint, Vector3 normal, int baseDamage)
    {
        // First hit — generate the weak point
        if (!isActive)
        {
            SpawnWeakPoint();
        }

        int damage = baseDamage;

        if (isActive && Vector3.Distance(hitPoint, weakPointPosition) <= weakPointRadius)
        {
            damage = Mathf.RoundToInt(baseDamage * weakPointDamageMultiplier);
            Debug.Log($"Weak point hit! Damage: {damage}");

            // Reposition weak point after a successful weak point hit (like Fortnite)
            SpawnWeakPoint();
        }

        currentHP -= damage;

        if (currentHP <= 70 && previousHP > 70 ||
            currentHP <= 40 && previousHP > 40)
        {
            // Spawn minerals at certain points
            var mineral = Instantiate(_mineralPrefab, hitPoint, Quaternion.identity);
            mineral.GetComponent<Rigidbody>().AddForce((normal + Vector3.up) * 2f, ForceMode.Impulse);
        }
        
        previousHP = currentHP;
        if (currentHP <= 0)
        {
            BreakRock();
        }
    }

    void SpawnWeakPoint()
    {
        // Pick a random point on the surface using the mesh
        Vector3 localPoint = GetRandomSurfacePoint();
        weakPointPosition = transform.TransformPoint(localPoint);
        isActive = true;

        // Destroy old indicator
        if (spawnedIndicator != null)
            Destroy(spawnedIndicator);

        // Spawn visual indicator
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
        if (weaknessIndiciatorMeshFilter == null) return Random.onUnitSphere * 0.5f;

        Mesh mesh = weaknessIndiciatorMeshFilter.sharedMesh;
        Vector3[] vertices = mesh.vertices;

        // Pick a random vertex on the mesh surface
        Vector3 randomVertex = vertices[Random.Range(0, vertices.Length)];

        // Push it slightly outward so it sits on the surface
        Vector3 outward = randomVertex.normalized;
        return randomVertex + outward * 0.01f;
    }

    void BreakRock()
    {
        if (spawnedIndicator != null)
            Destroy(spawnedIndicator);

        Destroy();
    }

    void OnDrawGizmosSelected()
    {
        if (isActive)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(weakPointPosition, weakPointRadius);
        }
    }

    private void Destroy()
    {
        for (var i = 0; i < 3; i++)
        {
            var randomLeft = Vector3.right * Random.Range(-0.1f, 0.1f);
            var mineral = Instantiate(_mineralPrefab, transform.position + randomLeft, Quaternion.identity);
            mineral.GetComponent<Rigidbody>().AddForce(Vector3.up + randomLeft, ForceMode.Impulse);
        }
        
        Destroy(transform.parent.gameObject);
    }
}
