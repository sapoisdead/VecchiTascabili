using System;

public interface IHealth
{
    event Action<float> OnHealthChanged;
    event Action OnDeath; 

    float CurrentHP { get; } // -> punti di vita correnti
    float MaxHP { get; } // -> punti di vita massimi
    bool IsAlive { get; }

    void TakeDamage(float amount);
    void Heal(float amount);
}
