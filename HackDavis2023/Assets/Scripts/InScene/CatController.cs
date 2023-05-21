using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class CatController : MonoBehaviour
{
    public static CatController Instance;
    
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator anim;
    [SerializeField] private BoxCollider2D physColl;
    [SerializeField] private SpriteRenderer ren;
    
    
    [Header("Movement Options")]
    [SerializeField] private float distanceMargin;
    [SerializeField] private float jumpAngle;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float fric;
    [SerializeField] private float accel;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private LayerMask platformMask;

    private bool grounded;
    
    private static readonly int Grounded = Animator.StringToHash("Grounded");
    private static readonly int XVel = Animator.StringToHash("XVel");
    private static readonly int YVel = Animator.StringToHash("YVel");

    private List<PlatformEffector2D> platforms = new();

    private bool droppingDown;
    
    private void Awake()
    {
        Instance = this;
        platforms = FindObjectsOfType<PlatformEffector2D>().ToList();
    }

    private void FixedUpdate()
    {
        GroundCheck();
        
        anim.SetBool(Grounded, grounded);
        anim.SetFloat(XVel, Mathf.Abs(rb.velocity.x));
        anim.SetFloat(YVel, rb.velocity.y);

        rb.velocity = new Vector2(Mathf.MoveTowards(rb.velocity.x, 0f, fric * Time.fixedDeltaTime), rb.velocity.y);
    }

    private void GroundCheck()
    {
        var mask = groundMask;
        if (!droppingDown)
            mask |= platformMask;
        // Ground cast oh yeah
        var cast = Physics2D.BoxCastAll(
            physColl.bounds.center,
            (Vector2)physColl.bounds.size - Vector2.right * 0.001f,
            0,
            Vector2.down,
            0.02f,
            mask
        );

        grounded = false;
        float bottom = physColl.bounds.center.y - physColl.bounds.extents.y;
        foreach (var hit in cast)
        {
            if (hit.collider == null) continue;
            if (hit.point.y <= bottom)
            {
                grounded = true;
                break;
            }
        }
        
    }

    public async UniTask<bool> MoveToLocation(Vector2 target)
    {
        if (this == null) return false;
        var cts = destroyCancellationToken;
        Vector2 targetVec = target - (Vector2)rb.position;
        while (targetVec.sqrMagnitude > distanceMargin * distanceMargin)
        {
            // See if jump is needed
            float angle = Mathf.Rad2Deg * Mathf.Atan2(Mathf.Abs(targetVec.y), Mathf.Abs(targetVec.x));

            if (angle > jumpAngle && targetVec.y > 0)
            {
                Jump(targetVec.y);
            }
            else if(grounded)
            {
                float xVel = Mathf.MoveTowards(rb.velocity.x, moveSpeed * Mathf.Sign(targetVec.x), accel * Time.fixedDeltaTime);
                rb.velocity = new Vector2(xVel, rb.velocity.y);
            }

            // Dropdown through platforms
            if (targetVec.y < 0)
            {
                rb.velocity = new Vector2(0f, rb.velocity.y);
                SetDroppingThroughPlatforms(true);
            }
            else if(grounded)
            {
                SetDroppingThroughPlatforms(false);
            }
            
            targetVec = target - (Vector2)rb.position;

            ren.flipX = targetVec.x < 0;
            
            await UniTask.WaitForFixedUpdate();
            if (cts.IsCancellationRequested)
                break;
        }
        SetDroppingThroughPlatforms(false);
        return true;
    }

    private void SetDroppingThroughPlatforms(bool val)
    {
        droppingDown = val;
        Physics2D.IgnoreLayerCollision(3, 8, val);
        foreach (var effector in platforms)
        {
            if(!val)
                effector.colliderMask |= 1 << 3;
            else
                effector.colliderMask &= ~(1 << 3);
        }
    }

    private void Jump(float height)
    {
        height += 0.2f;
        float yVel = Mathf.Sqrt(2 * Mathf.Abs(Physics2D.gravity.y) * height);
        rb.velocity = new Vector2(0, yVel);
    }


    public Transform target;

    [ContextMenu("Go To Pos")]

    public void GoToDebugPos()
    {
        MoveToLocation(target.position);
    }
}
