using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("‘Ì—Í")]
    [SerializeField] private int maxHP = 100;
    private int currentHP;

    [Header("Animator")]
    [SerializeField] private Animator anim;

    [Header("–³“GƒtƒŒ[ƒ€")]
    [SerializeField] private float invincibleTime = 0.8f; // 0.8•b–³“G
    private bool isInvincible = false;

    public bool IsDead { get; private set; } = false;

    void Start()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(int damage)
    {
        if (IsDead || isInvincible) return;

        currentHP -= damage;
        currentHP = Mathf.Max(currentHP, 0);

        Debug.Log($"Player took {damage} damage! Current HP: {currentHP}");

        if (anim != null)
        {
            anim.SetTrigger("Damage");
        }

        // –³“GƒtƒŒ[ƒ€ŠJŽn
        if (invincibleTime > 0)
            StartCoroutine(InvincibleCoroutine());

        if (currentHP <= 0)
            Die();
    }

    private System.Collections.IEnumerator InvincibleCoroutine()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibleTime);
        isInvincible = false;
    }

    private void Die()
    {
        if (IsDead) return;

        IsDead = true;
        Debug.Log("Player Died");
        if (anim != null)
            anim.SetTrigger("Die");
        // ‘€ì’âŽ~‚È‚Ç‚à‚±‚±‚Å
    }

    public int GetCurrentHP() => currentHP;
    public int GetCurrentHPMax() => maxHP;
}