using UnityEngine;

/// <summary>
/// Handles ground and head bump collision checks for the player.
/// </summary>
public class CollisionChecker
{
    private readonly Collider2D _feetCollider;
    private readonly Collider2D _bodyCollider;
    private readonly SOMovementStats _stats;

    public CollisionChecker(Collider2D feetCollider, Collider2D bodyCollider, SOMovementStats stats)
    {
        _feetCollider = feetCollider;
        _bodyCollider = bodyCollider;
        _stats = stats;
    }

    /// <summary>
    /// Returns true if the player is currently touching the ground.
    /// </summary>
    public bool IsGrounded()
    {
        Vector2 origin = new Vector2(_feetCollider.bounds.center.x, _feetCollider.bounds.min.y);
        Vector2 size = new Vector2(_feetCollider.bounds.size.x, _stats.GroundDetectionRayLength);
        RaycastHit2D hit = Physics2D.BoxCast(origin, size, 0f, Vector2.down, _stats.GroundDetectionRayLength, _stats.GroundLayer);
        return hit.collider != null;
    }

    /// <summary>
    /// Returns true if the player has bumped their head.
    /// </summary>
    public bool BumpedHead()
    {
        Vector2 origin = new Vector2(_bodyCollider.bounds.center.x, _bodyCollider.bounds.max.y);
        Vector2 size = new Vector2(_bodyCollider.bounds.size.x * _stats.HeadWidth, _stats.HeadDetectionRayLength);
        RaycastHit2D hit = Physics2D.BoxCast(origin, size, 0f, Vector2.up, _stats.HeadDetectionRayLength, _stats.GroundLayer);
        return hit.collider != null;
    }
}
