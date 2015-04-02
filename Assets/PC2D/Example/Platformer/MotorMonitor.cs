using UnityEngine;
using UnityEngine.UI;

namespace PC2D
{
    public class MotorMonitor : MonoBehaviour
    {
        public Text fallText;
        public Text motorStateText;
        public Text prevMotorStateText;

        public PlatformerMotor2D motorToWatch;

        private PlatformerMotor2D.MotorState _savedMotorState;

        private PlatformerMotor2D.MotorState MotorState
        {
            set
            {
                if (_savedMotorState != value)
                {
                    prevMotorStateText.text = string.Format("Prev Motor State: {0}", _savedMotorState);
                    motorStateText.text = string.Format("Motor State: {0}", value);
                }
                _savedMotorState = value;
            }
        }

        // Use this for initialization
        void Start()
        {
            motorToWatch.onFallFinished += OnFallFinished;
            fallText.color = Color.white;
        }

        public void OnFallFinished(float fallDist)
        {
            fallText.text = string.Format("Fall Distance: {0:F}", fallDist);
            fallText.color = Color.green;
            _justFellTimer = 0.5f;
        }

        float _justFellTimer;

        // Update is called once per frame
        void Update()
        {
            if (_justFellTimer > 0)
            {
                _justFellTimer -= Time.deltaTime;
                if (_justFellTimer <= 0)
                {
                    fallText.color = Color.white;
                }
            }

            MotorState = motorToWatch.motorState;
        }
    }
}
