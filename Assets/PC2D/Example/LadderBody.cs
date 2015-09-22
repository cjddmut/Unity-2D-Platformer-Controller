using UnityEngine;

namespace PC2D
{
    public class LadderBody : SpriteDebug
    {
        public bool enableRestrictedArea = true;
        public SpriteRenderer restrictedArea = null;
        public bool disableRestrictedTop = true;
        public float topHeight = 0;
        public float bottomHeight = 0;

        public override void OnTriggerEnter2D(Collider2D o)
        {
            base.OnTriggerEnter2D(o);

            PlatformerMotor2D motor = o.GetComponent<PlatformerMotor2D>();
            if (motor)
            {
                // some games could want to enable auto bottom/top detection based on
                // restricted area... restrictedArea ? restrictedArea.bounds : _sprite.bounds
                motor.LadderAreaEnter(_sprite.bounds, topHeight, bottomHeight);

                if (enableRestrictedArea)
                {
                    motor.SetRestrictedArea(restrictedArea.bounds, disableRestrictedTop);
                }
                else
                {
                    motor.ClearRestrictedArea();
                }
            }
        }

        public override void OnTriggerStay2D(Collider2D o)
        {
            base.OnTriggerStay2D(o);

            PlatformerMotor2D motor = o.GetComponent<PlatformerMotor2D>();
            if (motor)
            {
                motor.LadderAreaEnter(_sprite.bounds, topHeight, bottomHeight);
            }
        }

        public override void OnTriggerExit2D(Collider2D o)
        {
            base.OnTriggerExit2D(o);

            PlatformerMotor2D motor = o.GetComponent<PlatformerMotor2D>();
            if (motor)
            {
                motor.FreedomAreaExit();
                if (enableRestrictedArea)
                {
                    motor.DisableRestrictedArea();
                    motor.ClearRestrictedArea();
                }
            }
        }
    }
}
