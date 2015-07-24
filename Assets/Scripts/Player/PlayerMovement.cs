using UnityEngine;
using UnityEngine.Networking;
using Toolkit;

[RequireComponent(typeof(FiniteStateMachine))]
public class PlayerMovement : NetworkBehaviour
{

    public static class Movement {
        public const string name = "Movement";
        public const string Idle = "Idle";
        public const string Walking = "Walking";
    }


    public float speed = 6f;

    Vector3 movement;
    Animator anim;
    Rigidbody playerRigidbody;
    int floorMask;
    float camRayLenght = 100f;

    FiniteStateMachine fsm;
    
    void Awake()
    {
        fsm = GetComponent<FiniteStateMachine>();
        floorMask = LayerMask.GetMask("Floor");
        anim = GetComponent<Animator>();
        playerRigidbody = GetComponent<Rigidbody>();
    }
    
    void Start() {
        if (isLocalPlayer)
        {
            CameraFollow cf = Camera.main.GetComponent<CameraFollow>();
            cf.target = transform;
            cf.enabled = true;
        }
        fsm.ChangeState(Movement.name, Movement.Idle);
    }

    void FixedUpdate()
    {
        if (isLocalPlayer) {
            Move();
        }
    }

    private void Move()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Move(h, v);
        Animating(h, v);
        Turning();
    }

    private void Move(float h, float v)
    {
        movement.Set(h, 0f, v);
        movement = movement.normalized * speed * Time.deltaTime;
        playerRigidbody.MovePosition(transform.position + movement);
    }

    private void Turning()
    {
        Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit floorHit;
        if (Physics.Raycast(camRay, out floorHit, camRayLenght, floorMask))
        {
            Vector3 playerToMouse = floorHit.point - transform.position;
            playerToMouse.y = 0f;
            Quaternion newRotation = Quaternion.LookRotation(playerToMouse);
            playerRigidbody.MoveRotation(newRotation);
        }
    }


    void Animating(float h, float v) {
        string newWalkingStatus = h != 0f || v != 0f ? Movement.Walking : Movement.Idle;
        if (fsm.ChangeState(Movement.name, newWalkingStatus))
        {
            CmdChangeWalkingStatus(newWalkingStatus);
        }
    }

    [Command]
    void CmdChangeWalkingStatus(string state) {
        RpcChangeWalkingStatue(state);
    }

    [ClientRpc]
    void RpcChangeWalkingStatue(string state) {
        fsm.ChangeState(Movement.name, state);
    }

    [StateListener(state = Movement.name, when = Movement.Idle, on = "Enter")]
    void IdleEnter() {
        Debug.Log(("Idel"));
        anim.SetBool("IsWalking", false);
    }

    [StateListener(state = Movement.name, when = Movement.Walking, on = "Enter")]
    void WalkingEnter() {
        Debug.Log("Wakling");
        anim.SetBool("IsWalking", true);
    }
}
