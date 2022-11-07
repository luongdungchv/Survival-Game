using UnityEngine;

public class PlayerHitBox : HitBox
{
    [SerializeField] private string tool;

    protected override void OnHitDetect(RaycastHit hit)
    {
        if (hit.collider.TryGetComponent<IDamagable>(out var target))
        {
            hitVfx.transform.position = hit.point;
            hitVfx.Play();
            CamShake.ins.Shake();
            string currentTool = PlayerAttack.ins.currentWieldName;
            float currentBaseDmg = PlayerAttack.ins.currentBaseDmg;
            PlayerDmgDealer.ins.SetProps(currentBaseDmg, currentTool, this, target);

            PlayerDmgDealer.ins.Excute();
        }
    }
}