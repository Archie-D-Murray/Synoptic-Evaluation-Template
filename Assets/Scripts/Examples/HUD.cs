using UnityEngine;

using TMPro;

using System.Collections.Generic;
using Utilities;

using AI.Examples;

class FloatingPanel {
    public GameObject Panel;
    public TMP_Text Text;
    public CanvasGroup Group;
    public float Lifetime;

    public FloatingPanel(GameObject panel, TMP_Text text, CanvasGroup group, float lifetime) {
        Panel = panel;
        Text = text;
        Group = group;
        Lifetime = lifetime;
    }
}

public class HUD : Singleton<HUD> {
    [SerializeField] private GameObject _prefab;
    [SerializeField] private float _speed = 50.0f;
    [SerializeField] private float _lifetime = 2.5f;

    private List<FloatingPanel> _queue = new List<FloatingPanel>();

    public void OnDamage(DamageSource source, DamageResult result) {
        GameObject panel = Instantiate(_prefab, transform);
        TMP_Text text = panel.GetComponentInChildren<TMP_Text>();
        text.text = $"Took {Mathf.RoundToInt(result.Dealt)} Damage!";
        _queue.Add(new FloatingPanel(panel, text, panel.GetComponent<CanvasGroup>(), _lifetime));
    }

    private void Update() {
        for (int i = 0; i < _queue.Count; i++) {
            _queue[i].Panel.transform.position += Vector3.up * _speed * Time.deltaTime;
            _queue[i].Lifetime -= Time.deltaTime;
            _queue[i].Group.alpha = _queue[i].Lifetime / _lifetime;

            if (_queue[i].Lifetime <= 0.0f) {
                Destroy(_queue[i].Panel, Time.deltaTime);
                _queue.RemoveAt(i--);
            }
        }
    }
}