namespace BoxHead2.Skills
{
    public abstract class TargetPositionModifySkill : Skill
    {
        bool isModifying;
        protected abstract Actor.Actor ModifyTarget { get; }
        protected abstract void UpdateTargetPosition();

        protected override void PrepareSkill()
        {
            base.PrepareSkill();
            isModifying = false;
        }

        protected override void DoStop()
        {
            base.DoStop();
            OnEndModify();
        }

        protected virtual void OnStartModify()
        {
            ModifyTarget.DisableMovement();
            isModifying = true;
        }

        protected virtual void OnEndModify()
        {
            ModifyTarget.EnableMovement();
            isModifying = false;
        }
    }
}