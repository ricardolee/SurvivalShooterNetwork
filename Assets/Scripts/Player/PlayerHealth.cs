using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using System.Collections;
using Toolkit;

[RequireComponent(typeof(FiniteStateMachine))]
public class PlayerHealth : NetworkBehaviour
{

    public static string DamageEvent ="DamageEvent";
    
    public static void RestartLevel()
    {
        Application.LoadLevel(Application.loadedLevel);
    }

    public static class Health {
        public const string name = "Health";
        public const string Live = "Live";
        public const string Death = "Death";
    }

    public int startingHealth = 100;
    public int currentHealth;
    public Slider healthSlider;
    public Image damageImage;
    public AudioClip deathClip;
    public float flashSpeed = 0.05f;
    public Color flashColor = new Color(1f, 0f, 0f, 0.1f);

    Animator anim;
    AudioSource playerAudio;
    PlayerMovement playerMovement;
    PlayerShooting playerShooting;

    bool isDead()
    {
        return fsm.GetCurrentState(Health.name) == Health.Death;
    }

    FiniteStateMachine fsm;
    EventDispatcher events;
    
    void Awake()
    {
        fsm = GetComponent<FiniteStateMachine>();
        events = fsm.events;
        anim = GetComponent<Animator>();
        playerAudio = GetComponent<AudioSource>();
        playerMovement = GetComponent<PlayerMovement>();
        playerShooting = GetComponentInChildren<PlayerShooting>();
        currentHealth = startingHealth;
    }

    void Start()
    {
        fsm.ChangeState(Health.name, Health.Live);
        damageListener = events.GenListener("TakeDamage", this);
    }

    
    private Listener damageListener = null;

    [StateListener(state = Health.name, when = Health.Live, on = "Enter")]
    void LiveEnter() {
        events.Register(DamageEvent, damageListener);
    }

    [StateListener(state = Health.name, when = Health.Live, on = "Exit")]
    void LiveExit() {
        events.Cancel(DamageEvent, damageListener);
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        healthSlider.value = currentHealth;
        playerAudio.Play();
        if (currentHealth <= 0)
        {
            fsm.ChangeState(Health.name, Health.Death);
        }
        StartCoroutine(FlashDamage());
    }

    private IEnumerator FlashDamage()
    {
        damageImage.color = flashColor;
        yield return new WaitForSeconds(flashSpeed);
        damageImage.color = Color.clear;
    }

    [StateListener(state = Health.name, when = Health.Death, on = "Enter")]
    void Death()
    {
        anim.SetTrigger("Die");
        playerAudio.clip = deathClip;
        playerAudio.Play();
        playerMovement.enabled = false;
        playerShooting.enabled = false;
    }


}
