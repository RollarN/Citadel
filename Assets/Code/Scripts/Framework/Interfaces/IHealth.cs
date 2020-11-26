public interface IHealth<ElementDamage>
{
    /// <summary>
    /// Receive damage
    /// </summary>
    /// <param name="amount">Amount of damage to take.</param>
    void TakeDamage(float amount, ElementDamage damageType);
    /// <summary>
    /// Gain health
    /// </summary>
    /// <param name="amount">Amount of health to restore.</param>
    void RestoreHealth(float amount);
}