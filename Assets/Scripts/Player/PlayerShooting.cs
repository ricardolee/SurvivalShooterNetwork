using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;

public class PlayerShooting : NetworkBehaviour
{
    public int damagePerShot = 20;
    public float fireRate = 0.15f;
    public float range = 100f;

    
    private Ray shootRay;
    private RaycastHit shootHit;
    private int shootableMask;
    private ParticleSystem gunParticles;
    private LineRenderer gunLine;
    private AudioSource gunAudio;
    private Light gunLight;
    private float effectsDisplayTime = 0.02f;
    private GameObject gunBarrelEnd;
    private float nextFire = 0;
    
    void Start()
    {
        shootableMask = LayerMask.GetMask("Shootable");
        gunBarrelEnd = transform.FindChild("GunBarrelEnd").gameObject;    
        gunParticles = gunBarrelEnd.GetComponent<ParticleSystem>();
        gunLine = gunBarrelEnd.GetComponent <LineRenderer>();
        gunAudio = gunBarrelEnd.GetComponent<AudioSource>();
        gunLight = gunBarrelEnd.GetComponent<Light>();
    }

    void Update()
    {
        if (isLocalPlayer)
        {
            nextFire += Time.deltaTime;
            if (Input.GetButton("Fire1") && Time.time > nextFire)
            {
                Shoot();
            }            
        }
    }

    
    void Shoot()
    {
        if (isLocalPlayer)
        {
            nextFire = Time.time + fireRate;
        }
        gunAudio.Play();
        gunParticles.Stop();
        gunParticles.Play();
        shootRay.origin = gunBarrelEnd.transform.position;
        shootRay.direction = gunBarrelEnd.transform.forward;
        gunLine.SetPosition(0, gunBarrelEnd.transform.position);
        if (Physics.Raycast(shootRay, out shootHit, range, shootableMask))
        {
            EnemyHealth enemyHealth = shootHit.collider.GetComponent <EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damagePerShot, shootHit.point);
            }
            gunLine.SetPosition(1, shootHit.point);
        }
        else
        {
            gunLine.SetPosition(1, shootRay.origin + shootRay.direction * range);
        }
        StartCoroutine(BlinkShootLine());
    }

    private IEnumerator BlinkShootLine()
    {
        gunLight.enabled = true;
        gunLine.enabled = true;
        yield return new WaitForSeconds(effectsDisplayTime);
        gunLine.enabled = false;
        gunLight.enabled = false;
    }
}
