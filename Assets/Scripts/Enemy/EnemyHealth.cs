using UnityEngine;
using Toolkit;
using System.Collections;

[RequireComponent(typeof(StateManager))]
public class EnemyHealth : MonoBehaviour {

    public const string DamageEvent = "DamageEvent";
    
    public enum HealthState
    {
        Live, Die, Sink 
    }
    
    public int startingHealth = 100;
    public int currentHealth;
    public float sinkSpeed = 2.5f;
    public int scoreValue = 10;
    public AudioClip deathClip;

    Animator anim;
    AudioSource enemyAudio;
    ParticleSystem hitParticles;
    SphereCollider sphereCollider;

    EventDispatcher events;

    [StateMachineInject]
    StateMachine<HealthState> healthSM;
    Listener damageListene;
    
    void Awake () {
        anim = GetComponent <Animator> ();
        enemyAudio = GetComponent <AudioSource> ();
        hitParticles = GetComponentInChildren <ParticleSystem> ();
        sphereCollider = GetComponent<SphereCollider> ();
        
        currentHealth = startingHealth;
        events = GetComponent<EventDispatcher>();
        damageListene = events.GenListener("TakeDamage", this);
    }

    void Start() {
        healthSM.Init(HealthState.Live);
    }


    
    void Update () {
        healthSM.Update();
    }


    void TakeDamage (int amount, Vector3 hitPoint) {
        enemyAudio.Play ();
        currentHealth -= amount;
        hitParticles.transform.position = hitPoint;
        hitParticles.Play();
        if(currentHealth <= 0) {
            healthSM.ChangeState(HealthState.Die);
        }
    }

    [StateListener(state = HealthState.Live, on = StateEvent.Enter)]
    void LiveEnter() {
        events.Register(DamageEvent, damageListene);
    }

    [StateListener(state = HealthState.Live, on = StateEvent.Exit)]
    void LiveExit() {
        events.Cancel(DamageEvent, damageListene);
    }

    
    [StateListener(state = HealthState.Die, on = StateEvent.Enter)]
    IEnumerator Die () {
        anim.SetTrigger ("Die");
        enemyAudio.clip = deathClip;
        enemyAudio.Play ();
        sphereCollider.isTrigger = true;
        yield return new WaitForSeconds(2);
        healthSM.ChangeState(HealthState.Sink);
    }

    [StateListener(state = HealthState.Sink, on = StateEvent.Enter)]
    void StartSinking () {
        GetComponent <NavMeshAgent> ().enabled = false;
        GetComponent <Rigidbody> ().isKinematic = true;
        ScoreManager.score += scoreValue;
        Destroy (gameObject, 2f);
    }
    
    [StateListener(state = HealthState.Sink, on = StateEvent.Update)]
    void SinkUpdate () {
        transform.Translate (-Vector3.up * sinkSpeed * Time.deltaTime);
    }



}
