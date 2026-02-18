using UnityEngine;

public class Mineral : MonoBehaviour, IInteractable
{
    private Transform _outline;
    public string MineralName;

    private void Start()
    {
        _outline = transform.GetChild(0);
    }

    public void ToggleOutline(bool state)
    {
        _outline.gameObject.SetActive(state);
    }
}
