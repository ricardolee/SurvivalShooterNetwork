using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using System.Collections;
using Toolkit;

[RequireComponent(typeof(StateManager))]
[RequireComponent(typeof(EventDispatcher))]
[RequireComponent(typeof(AudioSource))]
public class PlayerHealth : NetworkBehaviour
{

    public static string DamageEvent ="DamageEvent";

    public enum HealthState {
        Live, Death
    }


    public static void RestartLevel()
    {
        Application.LoadLevel(Application.loadedLevel);
    }

    int startingHealth = 100;

    Slider healthSlider;
    Image damageImage;
    public AudioClip playerHurt;
    public AudioClip deathClip;
    public float flashSpeed = 0.05f;
    public Color flashColor = new Color(1f, 0f, 0f, 0.1f);

    int currentHealth;

    Animator anim;
    AudioSource playerAudio;
    PlayerMovement playerMovement;
    PlayerShooting playerShooting;
    EventDispatcher events;
    
    [StateMachineInject]
    StateMachine<HealthState> healthSM;
    
    void Awake()
    {
        events = GetComponent<EventDispatcher>();
        anim = GetComponent<Animator>();
        playerAudio = GetComponent<AudioSource>();
        playerMovement = GetComponent<PlayerMovement>();
        playerShooting = GetComponentInChildren<PlayerShooting>();
        currentHealth = startingHealth;
    }

    void Start()
    {
        healthSlider = GameObject.Find("HealthSlider").GetComponent<Slider>();
        damageImage = GameObject.Find("DamageImage").GetComponent<Image>();
        damageListener = events.GenListener("TakeDamage", this);
        healthSM.Init(HealthState.Live);
    }

    
    private Listener damageListener = null;

    [StateListener(state = HealthState.Live, on = StateEvent.Enter)]
    void LiveEnter() {
        events.Register(DamageEvent, damageListener);
    }

    [StateListener(state = HealthState.Live, on = StateEvent.Exit)]
    void LiveExit() {
        events.Cancel(DamageEvent, damageListener);
    }

    public void TakeDamage(int amount, Action deathCallback)
    {
        currentHealth -= amount;
        if (isLocalPlayer)
        {
            healthSlider.value = currentHealth;
            StartCoroutine(FlashDamage());
        }
        playerAudio.Play();
        if (currentHealth <= 0)
        {
            deathCallback();
            healthSM.ChangeState(HealthState.Death);
        }
    }

    private IEnumerator FlashDamage()
    {
        damageImage.color = flashColor;
        yield return new WaitForSeconds(flashSpeed);
        damageImage.color = Color.clear;
    }

    [StateListener(state = HealthState.Death, on = StateEvent.Enter)]
    void Death()
    {
        anim.SetTrigger("Die");
        playerAudio.clip = deathClip;
        playerAudio.Play();
        playerMovement.enabled = false;
        playerShooting.enabled = false;
    }
}
