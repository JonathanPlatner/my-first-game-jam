using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManagerM : MonoBehaviour
{
    [SerializeField]
    private KeyCode up = KeyCode.W;
    [SerializeField]
    private KeyCode right = KeyCode.D;
    [SerializeField]
    private KeyCode down = KeyCode.S;
    [SerializeField]
    private KeyCode left = KeyCode.A;
    [SerializeField]
    private KeyCode grab = KeyCode.Space;
    [SerializeField]
    private KeyCode shootMode = KeyCode.E;
    [SerializeField]
    private KeyCode blockMode = KeyCode.Q;
    [SerializeField]
    private int actionButton = 0;

    public Vector2 Move { get; private set; }
    public bool Grab { get; private set; }
    public bool GrabOS { get; private set; }
    public bool ShootMode { get; private set; }
    public bool ShootModeOS { get; private set; }
    public bool BlockMode { get; private set; }
    public bool BlockModeOS { get; private set; }
    public bool Action { get; private set; }
    public bool ActionOS { get; private set; }
    public bool ActionOffOS { get; private set; }
    public Vector2 MousePosition { get; private set; }

    private Camera mainCam;

    private void Start()
    {
        mainCam = Camera.main;
    }

    private void Update()
    {
        // Move
        Vector2 moveVec = Vector2.zero;
        if(Input.GetKey(up)) moveVec.y++;
        if(Input.GetKey(down)) moveVec.y--;
        if(Input.GetKey(right)) moveVec.x++;
        if(Input.GetKey(left)) moveVec.x--;
        Move = moveVec;

        // Grab
        Grab = Input.GetKey(grab);
        GrabOS = Input.GetKeyDown(grab);

        // Block Mode and Shoot Mode
        BlockMode = Input.GetKey(blockMode);
        BlockModeOS = Input.GetKeyDown(blockMode);

        ShootMode = Input.GetKey(shootMode) && !Input.GetKey(blockMode);
        ShootModeOS = Input.GetKeyDown(shootMode) && !Input.GetKey(blockMode);

        // Action
        Action = Input.GetMouseButton(actionButton);
        ActionOS = Input.GetMouseButtonDown(actionButton);
        ActionOffOS = Input.GetMouseButtonUp(actionButton);
       

        // Mouse Position
        MousePosition = mainCam.ScreenToWorldPoint(Input.mousePosition);

    }
}
