using UnityEngine;

public class MineralDeposit : MonoBehaviour
{
    [SerializeField]
    private GameObject _mineralPrefab;

    public int PowerRequirement;
    public void ProduceMineral(Vector3 spawnPoint, Vector3 normal)
    {
        var mineral = Instantiate(_mineralPrefab, spawnPoint, Quaternion.identity);
        mineral.GetComponent<Rigidbody>().AddForce((normal + Vector3.up) * 2f, ForceMode.Impulse);
    }
}
