using UnityEngine;

namespace PC2D
{
    public class LadderZone : SpriteDebug
    {
        public PlatformerMotor2D.LadderZone zone;

        public override void OnTriggerEnter2D(Collider2D o)
        {
            base.OnTriggerEnter2D(o);

            PlatformerMotor2D motor = o.GetComponent<PlatformerMotor2D>();
            if (motor)
            {
                motor.SetLadderZone(zone);
            }
        }
        public override void OnTriggerStay2D(Collider2D o)
        {
            base.OnTriggerStay2D(o);

            PlatformerMotor2D motor = o.GetComponent<PlatformerMotor2D>();
            if (motor)
            {
                motor.SetLadderZone(zone);
            }
        }

    }
}
