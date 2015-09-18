using UnityEngine;
using UnityEngine.Networking;
using Toolkit;

[RequireComponent(typeof(StateManager))]
public class PlayerMovement : NetworkBehaviour
{

    public float speed = 6f;

    Vector3 movement;
    Animator anim;
    Rigidbody playerRigidbody;
    int floorMask;
    float camRayLenght = 100f;
    public enum MovementState {
        Idle, Walk
    }

    [StateMachineInject]
    StateMachine<MovementState> movementSM;
    
    void Awake()
    {
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
        movementSM.Init(MovementState.Idle);
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
        MovementState newWalkingStatus = h != 0f || v != 0f ? MovementState.Walk : MovementState.Idle;
        if (movementSM.ChangeState(newWalkingStatus))
        {
            CmdChangeWalkingStatus(newWalkingStatus);
        }
    }

    [Command]
    void CmdChangeWalkingStatus(MovementState state) {
        RpcChangeWalkingStatue(state);
    }

    [ClientRpc]
    void RpcChangeWalkingStatue(MovementState state) {
        movementSM.ChangeState(state);
    }

    [StateListener(state = MovementState.Idle, on = StateEvent.Enter)]
    void IdleEnter() {
        anim.SetBool("IsWalking", false);
    }

    [StateListener(state = MovementState.Walk, on = StateEvent.Enter)]
    void WalkingEnter() {
        anim.SetBool("IsWalking", true);
    }
}
