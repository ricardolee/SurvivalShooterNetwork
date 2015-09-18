using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using Toolkit;

[RequireComponent(typeof(StateManager))]
public class EnemyAttack : NetworkBehaviour {

    public enum AttackState
    {
        Idle, Attack
    }

    public float timeBetweenAttacks = 0.5f;
    public int attackDamage = 10;

    
    GameObject targetPlayer;

    [StateMachineInject]
    StateMachine<AttackState> attachSM;
    Animator anim;
    AudioSource audioSource;
    
    void Awake () {
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        attachSM.Init(AttackState.Idle);
    }
    

    void OnTriggerEnter (Collider other) {
        if(other.gameObject.tag == "Player" && attachSM.CurrentState == AttackState.Idle) {
            targetPlayer = other.gameObject;
            attachSM.ChangeState(AttackState.Attack);
        }
    }
    
    void OnTriggerExit (Collider other) {
        if (other.gameObject == targetPlayer)
            attachSM.ChangeState(AttackState.Idle);
    }

    [ClientRpc]
    void RpcChangeState(AttackState state)
    {
        attachSM.ChangeState(state);
    }


    private Coroutine attackCoroutine;
    
    [StateListener(state = AttackState.Attack, on = StateEvent.Enter)]
    void AttackEnter() {
        attackCoroutine = StartCoroutine(AttackEnumerator());
    }

    [StateListener(state = AttackState.Attack, on = StateEvent.Exit)]
    void AttackExit() {
        if (attackCoroutine != null)
            StopCoroutine(attackCoroutine);
    }


    IEnumerator AttackEnumerator() {
        EventDispatcher events = targetPlayer.GetComponent<EventDispatcher>();
        while (true && isActiveAndEnabled)
        {
            Action playDeathCallback = () => {
                    attachSM.ChangeState(AttackState.Idle);
            };
            audioSource.Play();
            events.Trigger(PlayerHealth.DamageEvent, attackDamage, playDeathCallback); 
            yield return new WaitForSeconds(timeBetweenAttacks);
        }
    }
}
