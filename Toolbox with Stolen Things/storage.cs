using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// old casket code for safekeeping
/*public void OnExplosion(Vector3 position, ExplosionData data, Vector2 dir, Action onbegin, bool ignoreQueues, CoreDamageTypes damagetypes, bool ignoreDamageCaps)
{
    PlayerController player = Owner as PlayerController;
    GameManager.Instance.StartCoroutine(CheckForRocketJump(player, position, data));
}
private IEnumerator CheckForRocketJump(PlayerController player, Vector3 position, ExplosionData data)
{
    bool isAbleToRocketJump = false;
    float timer = 0.25f;
    while (!isAbleToRocketJump && timer > 0)
    {
        timer -= 0.05f;
        float distance = Vector3.Distance(position, player.sprite.WorldCenter.ToVector3ZUp());
        if (distance <= (data.damageRadius + 0.2f) && player.IsDodgeRolling)
        {
            Vector3 vector = player.sprite.WorldCenter.ToVector3ZUp() - position;
            jumpDirection = new Vector2(vector.x, vector.y);
            jumpDirection.x *= 200 * (timer / distance);
            jumpDirection.y *= 200 * (timer / distance);
            isAbleToRocketJump = true;
            GameManager.Instance.StartCoroutine(HandleRocketJump(player));
        }
        yield return new WaitForSeconds(0.05f);
    }

    yield break;
}
private IEnumerator HandleRocketJump(PlayerController player)
{
    //ETGModConsole.Log("HandleRocketJump started");
    cancelJump = false;
    if (player != null)
    {
        ImprovedAfterImage effect = player.gameObject.GetOrAddComponent<ImprovedAfterImage>();
        effect.shadowLifetime = 0.2f;
        effect.shadowTimeDelay = 0.1f;
        effect.dashColor = MoreColours.lightgrey;
        effect.targetHeight = 1f;
        effect.minTranslation = 8f/16f;

        player.ForceStopDodgeRoll();
        player.ToggleHandRenderers(true);
        player.ToggleGunRenderers(true);
        player.ownerlessStatModifiers.Add(damageBoost_jump);
        player.stats.RecalculateStats(player);
        player.OnPreDodgeRoll += OnDodgeRoll;
        player.healthHaver.OnDamaged += OnDamaged;
        player.specRigidbody.OnPreTileCollision += OnHitWall;
        player.specRigidbody.OnPreRigidbodyCollision += OnPreRigidbodyCollision;
        player.SetIsFlying(true, "rocketjump");
        effect.spawnShadows = true;
        player.AdditionalCanDodgeRollWhileFlying.SetOverride("rocketjump", true);
        while (!cancelJump && ((Mathf.Abs(jumpDirection.x) + Mathf.Abs(jumpDirection.y) > 5f) || (Mathf.Abs(player.Velocity.x) + Mathf.Abs(player.Velocity.y) > 5f)))
        {
            player.MovementModifiers += MovementMod;
            jumpDirection.x /= 1.1f;
            jumpDirection.y /= 1.1f;
            yield return new WaitForSeconds(0.1f);
            player.MovementModifiers -= MovementMod;
        }
        player.AdditionalCanDodgeRollWhileFlying.SetOverride("rocketjump", false);
        player.SetIsFlying(false, "rocketjump");
        effect.spawnShadows = false;
        player.ownerlessStatModifiers.Remove(damageBoost_jump);
        player.stats.RecalculateStats(player);
        player.specRigidbody.OnPreRigidbodyCollision -= OnPreRigidbodyCollision;
        player.healthHaver.OnDamaged -= OnDamaged;
        player.specRigidbody.OnPreTileCollision -= OnHitWall;
        player.OnPreDodgeRoll -= OnDodgeRoll;
    }
    yield break;
}
// fly past unflipped table + other stuff with collisions
public void OnPreRigidbodyCollision(SpeculativeRigidbody myrigidbody, PixelCollider mypixelcollider, SpeculativeRigidbody otherrigidbody, PixelCollider otherpixelcollider)
{
    if (otherrigidbody.projectile == null)
    {
        if (otherrigidbody.majorBreakable != null)
        {
            if (otherrigidbody.majorBreakable.GetComponentInParent<FlippableCover>() != null)
            {
                FlippableCover table = otherrigidbody.GetComponentInParent<FlippableCover>();
                if (!table.IsFlipped) { otherrigidbody.RegisterTemporaryCollisionException(myrigidbody, 1f / 150f); }
                else { cancelJump = true; }
            }
        }
        else if (!otherrigidbody.minorBreakable)
        {
            cancelJump = true;
        }
    }
}

// these are the things that will end a jump
public void OnDamaged(float resultValue, float maxValue, CoreDamageTypes damageTypes, DamageCategory damageCategory, Vector2 damageDirection)
{
    cancelJump = true;
}
public void OnHitWall(SpeculativeRigidbody myrigidbody, PixelCollider mypixelcollider, PhysicsEngine.Tile tile, PixelCollider tilepixelcollider)
{
    cancelJump = true;
}
public void OnDodgeRoll(PlayerController player)
{
    cancelJump = true;
}
// the jump itself
public void MovementMod(ref Vector2 voluntaryVal, ref Vector2 involuntaryVal)
{
    //ETGModConsole.Log(voluntaryVal);
    float involuntaryGradient = involuntaryVal.y / involuntaryVal.x;
    float voluntaryGradient = voluntaryVal.y / voluntaryVal.x;
    involuntaryVal += jumpDirection;
    if (voluntaryGradient.IsBetweenRange(involuntaryGradient - 4f, involuntaryGradient + 4f))
    {
        voluntaryVal /= 3f;
    }
    //ETGModConsole.Log(voluntaryVal);
}*/
