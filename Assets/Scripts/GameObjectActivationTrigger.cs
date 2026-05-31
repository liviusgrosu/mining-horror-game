using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameObjectActivationTrigger : MonoBehaviour
{
    [SerializeField] private List<GameObject> _targets = new();
    [SerializeField] private bool _setActive = true;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        foreach (var target in _targets.Where(target => target))
        {
            target.SetActive(_setActive);
        }
    }
}
