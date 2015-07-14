using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
    
public class PlayerHealth : MonoBehaviour {

    public static void RestartLevel () {
        Application.LoadLevel (Application.loadedLevel);
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
    bool isDead;
	
    void Awake () {
        anim = GetComponent <Animator> ();
        playerAudio = GetComponent <AudioSource> ();
        playerMovement = GetComponent <PlayerMovement> ();
        playerShooting = GetComponentInChildren <PlayerShooting> ();
        currentHealth = startingHealth;
    }
	
	
    void Update () {
    }
	
	
    public void TakeDamage (int amount) {
        currentHealth -= amount;
        healthSlider.value = currentHealth;
        playerAudio.Play ();
        if(currentHealth <= 0 && !isDead) {
            Death ();
        }
        StartCoroutine(FlashDamage());
    }

    private IEnumerator FlashDamage() {
        damageImage.color = flashColor;
        yield return new WaitForSeconds(flashSpeed);
        damageImage.color = Color.clear;
    }
	
	
    void Death () {
        isDead = true;
        anim.SetTrigger ("Die");

        playerAudio.clip = deathClip;
        playerAudio.Play ();
		
        playerMovement.enabled = false;
        playerShooting.enabled = false;
    }
	
	
}
