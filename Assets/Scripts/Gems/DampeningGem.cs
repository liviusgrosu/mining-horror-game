using System.Collections;
using UnityEngine;

public class DampeningGem : MonoBehaviour
{
    [SerializeField] private float _duration = 5f;
    [SerializeField] private float _dampenedSpeedMultiplier = 0.1f;
    [SerializeField] [Range(0f, 1f)] private float _dampenedVolumeMultiplier = 0.2f;
    [SerializeField] private CharacterFootsteps _footsteps;
    [SerializeField] private ParticleSystem _activationVFX;

    private float _timer;
    private bool _isActive;

    private void Update()
    {
        if (GameManager.Instance && (GameManager.Instance.InMenu || GameManager.Instance.HasDied))
        {
            return;
        }

        var gemUI = GemSelectionUI.Instance;
        if (!gemUI || gemUI.IsOpen || gemUI.SelectedGem != GemType.Dampening)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            if (!GemSelectionUI.Instance.TryConsumeUse(GemType.Dampening))
            {
                return;
            }

            _timer = _duration;

            if (_activationVFX)
            {
                _activationVFX.Play(true);
            }

            if (!_isActive)
            {
                StartCoroutine(ActivateDampening());
            }
        }
    }

    private IEnumerator ActivateDampening()
    {
        _isActive = true;
        _footsteps.DampenedSpeedMultiplier = _dampenedSpeedMultiplier;
        _footsteps.DampenedVolumeMultiplier = _dampenedVolumeMultiplier;
        _footsteps.IsDampened = true;

        while (_timer > 0f)
        {
            _timer -= Time.deltaTime;
            yield return null;
        }

        _footsteps.IsDampened = false;
        _isActive = false;
    }
}
