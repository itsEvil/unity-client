using Static;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class NewCharacterRect : MonoBehaviour
{
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private Image _charImage;
    [SerializeField] private Button _button;
    private ushort _classType;
    public void Init(PlayerDesc desc) {
        _classType = desc.Type;
        _nameText.text = desc.DisplayId;
        _charImage.sprite = desc.TextureData.GetTexture(0);
    }
    public void OnEnable() => _button.onClick.AddListener(OnClick);
    public void OnDisable() => _button.onClick.RemoveAllListeners();
    public void OnDestroy() => _button.onClick.RemoveAllListeners();
    private void OnClick() => NewCharacterController.Instance.OnClassSelect(_classType);
}
