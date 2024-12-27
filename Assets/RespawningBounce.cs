using System.Collections;
using UnityEngine;

public class RespawningBounce : Enemy
{
    private Vector3 spawnPosition;
    private bool isDead = false;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        spawnPosition = transform.position; // Store the initial spawn position
        rb.gravityScale = 12f; // You can keep the gravity if needed
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        // Optional: You can check for some conditions to trigger the death (e.g., health <= 0).
        if (isDead)
        {
            // Wait for 5 seconds before respawning
            StartCoroutine(Respawn());
        }
    }

    // This method will handle the respawn logic
    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(5f); // Wait for 5 seconds before respawning

        // Reset position and state to respawn the Zombie
        transform.position = spawnPosition;
        isDead = false; // Reset death state
        gameObject.SetActive(true); // Make the Zombie object active again

        // You can add any other reset logic, like resetting health if needed
    }

    // Example method for marking the zombie as dead (you can call this when the zombie is hit, etc.)
    public void KillZombie()
    {
        isDead = true;
        gameObject.SetActive(false); // Deactivate the zombie when it's "dead"
    }

    // Override this method if you need special behavior when the Zombie is hit
    public override void EnemyHit(float _damageDone, Vector2 _hitDirection, float _hitForce)
    {
        base.EnemyHit(_damageDone, _hitDirection, _hitForce);

        // Optionally, you can check for death condition (e.g., health reaching zero) and call KillZombie
        if (health <= 0)
        {
            KillZombie();
        }
    }
}
