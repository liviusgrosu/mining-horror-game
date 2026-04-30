using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Video;

public class ViewGemScreen : MonoBehaviour
{
    public static ViewGemScreen Instance;

    [Serializable]
    public class GemEntry
    {
        public string Name;
        [TextArea] public string Description;
        public Sprite Icon;
        public VideoClip Video;
    }

    [SerializeField] private List<GemEntry> _gems = new();
    [SerializeField] private List<ViewGemUISlot> _slots = new();
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _descriptionText;
    [SerializeField] private TextMeshProUGUI _placeholderText;
    [SerializeField] private GameObject _videoPlayerImage;
    [SerializeField] private VideoPlayer _videoPlayer;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        RefreshSlots();
        ResetSelection();
    }

    private void OnDisable()
    {
        if (_videoPlayer)
        {
            _videoPlayer.Stop();
        }
    }

    private void RefreshSlots()
    {
        for (int i = 0; i < _slots.Count; i++)
        {
            if (!_slots[i])
            {
                continue;
            }

            if (i < _gems.Count && _gems[i] != null)
            {
                _slots[i].Bind(this, i, _gems[i].Icon);
            }
            else
            {
                _slots[i].Clear();
            }
        }
    }

    public void SelectGem(int index)
    {
        if (index < 0 || index >= _gems.Count)
        {
            return;
        }

        var gem = _gems[index];

        if (_placeholderText)
        {
            _placeholderText.gameObject.SetActive(false);
        }
        if (_nameText)
        {
            _nameText.gameObject.SetActive(true);
            _nameText.text = gem.Name;
        }
        if (_descriptionText)
        {
            _descriptionText.gameObject.SetActive(true);
            _descriptionText.text = gem.Description;
        }
        if (_videoPlayerImage)
        {
            _videoPlayerImage.SetActive(gem.Video);
        }
        if (_videoPlayer && gem.Video)
        {
            _videoPlayer.clip = gem.Video;
            _videoPlayer.Play();
        }
        else if (_videoPlayer)
        {
            _videoPlayer.Stop();
        }
    }

    private void ResetSelection()
    {
        if (_placeholderText)
        {
            _placeholderText.gameObject.SetActive(true);
        }
        if (_nameText)
        {
            _nameText.gameObject.SetActive(false);
        }
        if (_descriptionText)
        {
            _descriptionText.gameObject.SetActive(false);
        }
        if (_videoPlayerImage)
        {
            _videoPlayerImage.SetActive(false);
        }
    }
}
