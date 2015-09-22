using UnityEngine;

namespace PC2D
{
    public class RestrictedArea : SpriteDebug
    {
        public enum TriggerAction
        {
            DoNothing,
            EnableRestrictedArea,
            EnableRestrictedAreaIfFreemode,
            DisableRestrictedArea,
            DisableRestrictedAreaIfFreemode
        }

        public TriggerAction RestrictedAreaOnEnter = TriggerAction.DoNothing;
        public TriggerAction RestrictedAreaOnExit = TriggerAction.DoNothing;
        public TriggerAction RestrictedAreaOnStay = TriggerAction.DoNothing;

        public bool exitFreeModeOnEnter;
        public bool exitFreeModeOnExit;

        public void DoAction(PlatformerMotor2D motor, TriggerAction action, bool exitFreeMode)
        {
            switch (action)
            {
                case TriggerAction.EnableRestrictedAreaIfFreemode:
                    if (motor.motorState == PlatformerMotor2D.MotorState.FreedomState)
                    {
                        motor.EnableRestrictedArea();
                    }
                    break;
                case TriggerAction.EnableRestrictedArea:
                    motor.EnableRestrictedArea();
                    break;
                case TriggerAction.DisableRestrictedAreaIfFreemode:
                    if (motor.motorState == PlatformerMotor2D.MotorState.FreedomState)
                    {
                        motor.DisableRestrictedArea();
                    }
                    break;
                case TriggerAction.DisableRestrictedArea:
                    motor.DisableRestrictedArea();
                    break;
            }

            if (exitFreeMode)
            {
                if (motor.motorState == PlatformerMotor2D.MotorState.FreedomState)
                {
                    motor.FreedomStateExit();
                }
            }
        }

        public override void OnTriggerEnter2D(Collider2D o)
        {
            base.OnTriggerEnter2D(o);

            PlatformerMotor2D motor = o.GetComponent<PlatformerMotor2D>();
            if (motor)
            {
                DoAction(motor, RestrictedAreaOnEnter, exitFreeModeOnEnter);
            }
        }

        public override void OnTriggerStay2D(Collider2D o)
        {
            base.OnTriggerEnter2D(o);

            PlatformerMotor2D motor = o.GetComponent<PlatformerMotor2D>();

            if (motor)
            {
                DoAction(motor, RestrictedAreaOnStay, false);
            }
        }

        public override void OnTriggerExit2D(Collider2D o)
        {
            base.OnTriggerExit2D(o);

            PlatformerMotor2D motor = o.GetComponent<PlatformerMotor2D>();

            if (motor)
            {
                DoAction(motor, RestrictedAreaOnExit, exitFreeModeOnExit);
            }
        }
    }
}
