using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameWinTrigger : MonoBehaviour
{
    [SerializeField]
    private Image whiteScreen;
    [SerializeField]
    private TextMeshProUGUI winText;
    [SerializeField]
    private float fadeTime = 2f;
    
    private bool _triggered;
    
    private void OnTriggerEnter(Collider other)
    {
        if (!_triggered && other.transform.CompareTag("Player"))
        {
            _triggered = true;
            StartCoroutine(StartWinScreen());    
        }
    }

    private IEnumerator StartWinScreen()
    {
        var i = 0f;
        while (i < fadeTime)
        {
            i += Time.deltaTime;
            whiteScreen.color = new Color(1f, 1f, 1f, i / fadeTime);
            yield return null;
        }

        GameManager.Instance.WinGame();
        
        yield return new WaitForSeconds(0.5f);

        i = 0;
        while (i < fadeTime / 2f)
        {
            i += Time.deltaTime;
            winText.color = new Color(0f, 0f, 0f, i / (fadeTime / 2f));
            yield return null;
        }
    }
}
