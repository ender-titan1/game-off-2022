using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    struct InteractionRes
    {
        public bool canJump;
        public bool hit;

        public static InteractionRes operator +(InteractionRes a, InteractionRes b)
        {
            return new InteractionRes
            {
                canJump = a.canJump || b.canJump,
                hit = a.hit || b.hit,
            };
        }
    }

    private Controls controls;
    private Rigidbody2D rb;
    private float movementAxis;
    private bool canJump;

    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpVel;
    [SerializeField] private LayerMask mask;

    private void Awake()
    {
        controls = new Controls();
        rb = gameObject.GetComponent<Rigidbody2D>();

        SetCallbakcs();
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    private void SetCallbakcs()
    {
        controls.Movement.Move.performed += (ctx) => HandleMovementIn(ctx.ReadValue<float>());
        controls.Movement.Move.canceled += (_) => HandleMovementIn(0);
        controls.Movement.Jump.performed += (_) => HandleJumpIn();
    }

    private void HandleMovementIn(float value)
    {
        if (value > 0)
            movementAxis = 1;
        else if (value < 0)
            movementAxis = -1;
        else
            movementAxis = 0;
    }

    private void HandleJumpIn()
    {
        if (canJump)
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y + jumpVel);
    }

    private InteractionRes Interact()
    {
        BoxCollider2D c = gameObject.GetComponent<BoxCollider2D>();
        Vector2 size = c.size;
        RaycastHit2D[] hits = Physics2D.BoxCastAll(transform.position, size, 0, Vector2.down, size.y, mask);
        InteractionRes res = new InteractionRes();

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider.CompareTag("Player"))
                continue;

            // JUMP
            float angle = Mathf.Abs(Mathf.Atan2(hit.normal.x, hit.normal.y) * (180 / Mathf.PI));

            if (angle < 45f)
                res += new InteractionRes { canJump = true };

            // HIT

            EnvirnomentObject obj = hit.collider.GetComponent<EnvirnomentObject>();

            if (obj != null && obj.hit)
            {
                res += new InteractionRes { hit = true };
            }
        }

        return res;
    }

    private void FixedUpdate()
    {
        canJump = Interact().canJump;

        rb.velocity = new Vector2(movementAxis * moveSpeed, rb.velocity.y);
    }
}
