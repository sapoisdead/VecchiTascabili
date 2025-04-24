using UnityEngine;
using System;
using System.Collections;

public class HealthComponent : MonoBehaviour, IHealth, IDamageable
{
    [SerializeField] private float _maxHP = 100f;
    [SerializeField] private float _invulnerableDuration = 0.5f;

    public float CurrentHP { get; private set; }
    public float MaxHP => _maxHP;
    public bool IsAlive => CurrentHP > 0f;

    public event Action<float> OnHealthChanged;
    public event Action OnDeath;

    private bool _isInvulnerable;

    private void Awake()
    {
        CurrentHP = _maxHP;
    }

    public void TakeDamage(float amount)
    {
        if (!IsAlive || _isInvulnerable)
            return;

        CurrentHP = Mathf.Clamp(CurrentHP - amount, 0f, _maxHP);
        OnHealthChanged?.Invoke(CurrentHP);

        StartCoroutine(InvulnerabilityCoroutine());

        if (!IsAlive)
            OnDeath?.Invoke();
    }

    public void Heal(float amount)
    {
        if (!IsAlive) return;
        CurrentHP = Mathf.Clamp(CurrentHP + amount, 0f, _maxHP);
        OnHealthChanged?.Invoke(CurrentHP);
    }

    public void TakeDamage(float amount, GameObject source)
    {
        TakeDamage(amount);
    }

    private IEnumerator InvulnerabilityCoroutine()
    {
        _isInvulnerable = true;
        yield return new WaitForSeconds(_invulnerableDuration);
        _isInvulnerable = false;
    }


    //public void TakeControl()          // nell’Enem​yController
    //{
    //    // 1. Disabilita IA
    //    enabled = false;               // se Update gestisce solo IA
    //                                   // 2. Passa al GameInput
    //    SetInputProvider(GameInput.Instance);
    //    // 3. Cambia tag/layer, resetta vita, ecc.
    //    gameObject.tag = "Player";
    //}
}
