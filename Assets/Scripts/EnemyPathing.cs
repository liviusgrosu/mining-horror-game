using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class EnemyPathing : MonoBehaviour
{
    public List<Transform> Points = new ();

    void OnDrawGizmos()
    {
        if (Points == null || Points.Count < 2) return;

        Gizmos.color = Color.green;

        for (int i = 0; i < Points.Count; i++)
        {
            Transform current = Points[i];
            Transform next = (i == Points.Count - 1) ? Points[0] : Points[i + 1];

            if (current != null && next != null)
            {
                Gizmos.DrawLine(current.position, next.position);
            }
        }
    }
}