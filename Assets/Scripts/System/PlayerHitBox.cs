using UnityEngine;

public class PlayerHitBox : HitBox
{
    protected override void OnHitDetect(RaycastHit hit)
    {
        //Debug.Log(hit.collider.gameObject.name);
        if (hit.collider.TryGetComponent<DamagableObject>(out var target))
        {
            //Debug.Log($"{halfExtents} {origin} {size} ");
            hitVfx.transform.position = hit.point;
            hitVfx.Play();
            CamShake.ins.Shake();
            //target.OnDamage(25, 3, "test");
            PlayerDmgDealer.ins.SetProps(15, tool, this, target);

            //target.OnDamage(new PlayerHitData(15, tool, this));
            PlayerDmgDealer.ins.Excute();
        }
    }
}