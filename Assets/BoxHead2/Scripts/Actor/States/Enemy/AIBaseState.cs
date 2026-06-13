namespace BoxHead2.Actor
{
    public class AIBaseState : ActorBaseState
    {
        protected AIEnemy AIEnemy;

        public AIBaseState(AIEnemy actor) : base(actor)
        {
            AIEnemy = actor;
        }
    }
}