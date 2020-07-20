using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField]
    private Inputs.Button action = new Inputs.Button(KeyCode.Mouse0);
    [SerializeField]
    private Inputs.Button cancel = new Inputs.Button(KeyCode.Mouse1);
    [SerializeField]
    private Inputs.Axis lateral = new Inputs.Axis(new KeyCode[] { KeyCode.D, KeyCode.RightArrow }, new KeyCode[] { KeyCode.A, KeyCode.LeftArrow });
    [SerializeField]
    private Inputs.Axis longitudinal = new Inputs.Axis(new KeyCode[] { KeyCode.W, KeyCode.UpArrow }, new KeyCode[] { KeyCode.S, KeyCode.DownArrow });
    [SerializeField]
    private Inputs.Button grab = new Inputs.Button(KeyCode.Space);
    [SerializeField]
    private Inputs.Button cannon = new Inputs.Button(KeyCode.E);
    [SerializeField]
    private Inputs.Button shield = new Inputs.Button(KeyCode.Q);
    [SerializeField]
    private Inputs.Button test = new Inputs.Button(KeyCode.Tab);

    public Inputs.Button Action { get { return action; } }
    public Inputs.Button Cancel { get { return cancel; } }
    public Inputs.Axis Lateral { get { return lateral; } }
    public Inputs.Axis Longitudinal { get { return longitudinal; } }
    public Inputs.Button Grab { get { return grab; } }
    public Inputs.Button Cannon { get { return cannon; } }
    public Inputs.Button Shield { get { return shield; } }
    public Inputs.Button Test { get { return test; } }
    public Inputs.Mouse Mouse { get; } = new Inputs.Mouse();

}
