using System;
using UnityEngine;

public class EndChaseCollider : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.CompareTag("Enemy"))
        {
            other.GetComponent<ShadeBehaviour>().EndChase();
        }
    }
}
