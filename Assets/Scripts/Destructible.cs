using System;
using UnityEngine;

public class Destructible : MonoBehaviour
{
    [SerializeField] private float[] stageHp;
    [SerializeField] private float powerRequirement;
    [SerializeField] private InventoryItem requiredGem;

    public float PowerRequirement => powerRequirement;
    public InventoryItem RequiredGem => requiredGem;

    private int _currentStageIndex;
    private float _currentHp;

    private void Start()
    {
        for (var i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(i == 0);
        }

        if (stageHp.Length > 0)
            _currentHp = stageHp[0];
    }

    public void TakeDamage(float damage = 1f)
    {
        _currentHp -= damage;
        if (_currentHp <= 0f)
        {
            AdvanceStage();
        }
    }

    private void AdvanceStage()
    {
        if (_currentStageIndex < transform.childCount)
        {
            transform.GetChild(_currentStageIndex).gameObject.SetActive(false);
        }

        _currentStageIndex++;

        if (_currentStageIndex >= transform.childCount)
        {
            Destroy(gameObject);
            return;
        }

        _currentHp = _currentStageIndex < stageHp.Length ? stageHp[_currentStageIndex] : 1f;
        transform.GetChild(_currentStageIndex).gameObject.SetActive(true);
    }
}
