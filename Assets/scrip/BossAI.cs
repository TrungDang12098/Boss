using UnityEngine;
using System.Collections;

public class BossVegetaAI : MonoBehaviour
{
    public Transform player;
    public float detectionRange = 10f;
    public float attackRange = 2f;
    public float moveSpeed = 3f;
    public float patrolDistance = 5f;
    public float waitAfterAttack = 1.5f;

    private Animator anim;
    private Vector3 startPos;
    private bool movingRight = true;
    private bool isBusy = false;
    private bool playerDetected = false;

    void Start()
    {
        anim = GetComponent<Animator>();
        startPos = transform.position;
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (isBusy) return;

        // Phát hiện player
        playerDetected = distance <= detectionRange;

        if (playerDetected)
            HandleCombat(distance);
        else
            Patrol();
    }

    // 🟢 Di chuyển qua lại (tuần tra)
    void Patrol()
    {
        anim.SetBool("isRunning", true);
        anim.SetBool("isAttacking", false);
        anim.SetBool("isKi", false);
        anim.SetBool("isSkill", false);

        float moveDir = movingRight ? 1f : -1f;
        transform.Translate(Vector2.right * moveDir * moveSpeed * Time.deltaTime);

        // Giới hạn phạm vi tuần tra
        if (Vector2.Distance(transform.position, startPos) >= patrolDistance)
        {
            movingRight = !movingRight;
            Flip();
        }
    }

    // 🔴 Khi phát hiện player
    void HandleCombat(float distance)
    {
        if (distance > attackRange)
        {
            // Tiến lại gần player
            anim.SetBool("isRunning", true);
            Vector3 dir = (player.position - transform.position).normalized;
            transform.position += new Vector3(dir.x, 0, 0) * moveSpeed * Time.deltaTime;
            FlipTowards(player.position.x);
        }
        else
        {
            anim.SetBool("isRunning", false);
            StartCoroutine(AttackSequence());
        }
    }

    // ⚡ Lật hướng sprite
    void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    void FlipTowards(float targetX)
    {
        Vector3 scale = transform.localScale;
        if ((targetX > transform.position.x && scale.x < 0) || (targetX < transform.position.x && scale.x > 0))
            scale.x *= -1;
        transform.localScale = scale;
    }

    // 🔥 Chuỗi hành động: Attack → Ki → Skill
    IEnumerator AttackSequence()
    {
        isBusy = true;

        // Attack
        anim.SetBool("isAttacking", true);
        yield return new WaitForSeconds(1.0f);
        anim.SetBool("isAttacking", false);

        yield return new WaitForSeconds(waitAfterAttack);

        // Ki
        anim.SetBool("isKi", true);
        yield return new WaitForSeconds(2.0f);
        anim.SetBool("isKi", false);

        // Skill
        anim.SetBool("isSkill", true);
        yield return new WaitForSeconds(2.0f);
        anim.SetBool("isSkill", false);

        yield return new WaitForSeconds(1.0f);

        isBusy = false;
    }
}
