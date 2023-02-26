using UnityEngine;

public class InteractableHitbox : HitBox
{
    private InteractableObject owner;

    protected override bool OnHitDetect(RaycastHit hit)
    {
        return owner.TouchDetect(hit);
    }
    protected override void OnNoHitDetect()
    {
        owner.ExitDetect();

    }
    public void SetOwner(InteractableObject owner)
    {
        this.owner = owner;
    }
}