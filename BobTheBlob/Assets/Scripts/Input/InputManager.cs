using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField]
    private Inputs.Button fire = new Inputs.Button(new KeyCode[] { KeyCode.Mouse0, KeyCode.Space });
    [SerializeField]
    private Inputs.Axis lateral = new Inputs.Axis(new KeyCode[] { KeyCode.D, KeyCode.RightArrow }, new KeyCode[] { KeyCode.A, KeyCode.LeftArrow });
    [SerializeField]
    private Inputs.Axis longitudinal = new Inputs.Axis(new KeyCode[] { KeyCode.W, KeyCode.UpArrow }, new KeyCode[] { KeyCode.S, KeyCode.DownArrow });
    [SerializeField]
    private Inputs.Button weapon1 = new Inputs.Button(KeyCode.Alpha1);
    [SerializeField]
    private Inputs.Button weapon2 = new Inputs.Button(KeyCode.Alpha2);
    [SerializeField]
    private Inputs.Button weapon3 = new Inputs.Button(KeyCode.Alpha3);
    [SerializeField]
    private Inputs.Button weapon4 = new Inputs.Button(KeyCode.Alpha4);

    public Inputs.Button Fire { get { return fire; } }
    public Inputs.Axis Lateral { get { return lateral; } }
    public Inputs.Axis Longitudinal { get { return longitudinal; } }
    public Inputs.Button Weapon1 { get { return weapon1; } }
    public Inputs.Button Weapon2 { get { return weapon2; } }
    public Inputs.Button Weapon3 { get { return weapon3; } }
    public Inputs.Button Weapon4 { get { return weapon4; } }
    public Inputs.Mouse Mouse { get; } = new Inputs.Mouse();

}
