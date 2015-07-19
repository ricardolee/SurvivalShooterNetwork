using UnityEngine;
using UnityEngine.Networking;
using FSM;

public class PlayerMovement : NetworkStateBehaviour
{
    public enum States {
        Idle,
        Walking
    }

    
    public float speed = 6f;

    Vector3 movement;
    Animator anim;
    Rigidbody playerRigidbody;
    int floorMask;
    float camRayLenght = 100f;

    
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
        InitState(States.Idle);
    }

    void FixedUpdate()
    {
        if (isLocalPlayer) {
            Movement();
        }
    }

    private void Movement()
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
        States newWalkingStatus = h != 0f || v != 0f ? States.Walking : States.Idle;
        if (newWalkingStatus != (States) GetState()) {
            ChangeState(newWalkingStatus);
            CmdChangeWalkingStatus(newWalkingStatus);
        }
    }

    [Command]
    void CmdChangeWalkingStatus(States state) {
        RpcChangeWalkingStatue(state);
    }

    [ClientRpc]
    void RpcChangeWalkingStatue(States state) {
        ChangeState(state);
    }

    [StateBehaviour(state = "Idle", on = "Enter")]
    void IdleEnter() {
        anim.SetBool("IsWalking", false);
    }

    [StateBehaviour(state = "Walking", on = "Enter")]
    void WalkingEnter() {
        anim.SetBool("IsWalking", true);
    }
}
