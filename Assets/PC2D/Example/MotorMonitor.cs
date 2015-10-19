using UnityEngine;
using UnityEngine.UI;

namespace PC2D
{
    public class MotorMonitor : MonoBehaviour
    {
        public Color textColor = Color.white;
        public string title = "";
        public int titleFontSize = 20;
        [TextArea(3,10)]
        public string instructions = "Keyboard\nMove: WASD\nJump: Space\nDash: Left Ctrl\n\nXBox Controller\nMove: Left Joystick\nJump: A\nDash: X";
        public int instructionsFontSize = 18;
        public int debugFontSize = 15;
        public Vector2 position = Vector2.zero;

        private string fallText;
        private string motorStateText;
        private string prevMotorStateText;
        private string extra;

        private GUIStyle guiStyle = new GUIStyle();

        public PlatformerMotor2D motorToWatch;

        private PlatformerMotor2D.MotorState _savedMotorState;

        private PlatformerMotor2D.MotorState MotorState
        {
            set
            {
                if (_savedMotorState != value)
                {
                    prevMotorStateText = string.Format("Prev Motor State: {0}", _savedMotorState);
                    motorStateText = string.Format("Motor State: {0}", value);
                }
                _savedMotorState = value;
            }
        }

        // Use this for initialization
        void Start()
        {
            motorToWatch.onLanded += OnFallFinished;
            //fallText.color = Color.white;
        }

        void OnGUI() {
            guiStyle.normal.textColor = textColor;

            GUILayout.BeginArea(new Rect(position.x, position.y, Screen.width - position.x, Screen.height - position.y));
            if (title.Length != 0)
            {
                guiStyle.fontSize = titleFontSize;
                GUILayout.Label(title, guiStyle);
                GUILayout.Space(titleFontSize);
            }

            if (instructions.Length != 0)
            {
                guiStyle.fontSize = instructionsFontSize;
                GUILayout.Label(instructions, guiStyle);
                GUILayout.Space(instructionsFontSize);
            }

            guiStyle.fontSize = debugFontSize;
            GUILayout.Label(
                fallText + "\n" +
                motorStateText + "\n" +
                prevMotorStateText + "\n" +
                extra
            , guiStyle);
            GUILayout.EndArea();
        }

        public void OnFallFinished()
        {
            fallText = string.Format("Fall Distance: {0:F}", motorToWatch.amountFallen);
            //fallText.color = Color.green;
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
                    //fallText.color = Color.white;
                }
            }

            MotorState = motorToWatch.motorState;

            extra = string.Format(
                "Colliding Against: {0}\n" +
                "inArea: {8}\n" +
                "Ladder Zone: {1}\n" +
                "Restricted? {2}\n" +
                "OneWayPlatforms? {3}\n" +
                "oneWayPlatformsAreWalls? {4}\n" +
                "normalizedMovement ({5:F} {6:F})\n" +
                "velocity {7}\n",
                motorToWatch.collidingAgainst,
                motorToWatch.ladderZone,
                motorToWatch.IsRestricted(),
                motorToWatch.enableOneWayPlatforms,
                motorToWatch.oneWayPlatformsAreWalls,
                motorToWatch.normalizedXMovement,
                motorToWatch.normalizedYMovement,
                motorToWatch.velocity,
                motorToWatch.inArea
            );
        }
    }
}
