using UnityEngine;
using System;

public class HealthComponent : MonoBehaviour, IHealth
{
    [SerializeField] private float _maxHP = 100f;
    public float CurrentHP { get; private set; }
    public float MaxHP => _maxHP;
    public bool IsAlive => CurrentHP > 0f;

    public event Action<float> OnHealthChanged;
    public event Action OnDeath;

    private void Awake()
    {
        CurrentHP = _maxHP;
    }

    public void TakeDamage(float amount)
    {
        if (!IsAlive) return;
        CurrentHP = Mathf.Clamp(CurrentHP - amount, 0f, _maxHP);
        OnHealthChanged?.Invoke(CurrentHP);
        if (!IsAlive) OnDeath?.Invoke();
    }

    public void Heal(float amount)
    {
        if (!IsAlive) return;
        CurrentHP = Mathf.Clamp(CurrentHP + amount, 0f, _maxHP);
        OnHealthChanged?.Invoke(CurrentHP);
    }
}