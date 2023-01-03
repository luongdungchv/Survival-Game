using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationReceiver : MonoBehaviour
{
    private PlayerAnimation animSystem => GetComponent<PlayerAnimation>();
    public void Animate(int animIndex)
    {
        switch (AnimationMapper.GetAnimationName(animIndex))
        {
            case "Idle":
                {
                    animSystem.Idle();
                    break;
                }
            case "Move":
                {
                    animSystem.Walk();
                    break;
                }
            case "Sprint":
                {
                    animSystem.Run();
                    break;
                }
            case "Dash":
                {
                    animSystem.Dash();
                    break;
                }
            case "InAir":
                {
                    animSystem.Jump();
                    break;
                }
            case "SwimNormal":
                {
                    animSystem.SwimNormal();
                    break;
                }
            case "SwimIdle":
                {
                    animSystem.Idle();
                    break;
                }
        }
    }

}
