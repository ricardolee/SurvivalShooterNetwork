using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using Toolkit;

[RequireComponent(typeof(FiniteStateMachine))]
public class PlayerShooting : NetworkBehaviour
{
    public static class Shooting
    {
        // state name
        public const string name = "Shooting";
        // states
        public const string Idle = "Idle";
        public const string Fire = "Fire";
    }

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
    FiniteStateMachine fsm;
    void Awake()
    {
        fsm = GetComponent<FiniteStateMachine>();
    }
    void Start()
    {
        shootableMask = LayerMask.GetMask("Shootable");
        gunBarrelEnd = transform.FindChild("GunBarrelEnd").gameObject;
        gunParticles = gunBarrelEnd.GetComponent<ParticleSystem>();
        gunLine = gunBarrelEnd.GetComponent<LineRenderer>();
        gunAudio = gunBarrelEnd.GetComponent<AudioSource>();
        gunLight = gunBarrelEnd.GetComponent<Light>();
        fsm.ChangeState(Shooting.name, Shooting.Idle);
    }

    void Update()
    {
        if (isLocalPlayer)
        {
            if (Input.GetButton("Fire1"))
            {
                fsm.ChangeState(Shooting.name, Shooting.Fire);
                CmdFire();
            }
        }
    }

    [Command]
    void CmdFire()
    {
        RpcFire();
    }

    [ClientRpc]
    void RpcFire()
    {
        fsm.ChangeState(Shooting.name, Shooting.Fire);
    }

    [StateListener(state = Shooting.name, when = Shooting.Fire, on = "Enter")]
    void Shoot()
    {
        StartCoroutine(ShootEnumerator());
    }

    IEnumerator ShootEnumerator()
    {
        gunAudio.Play();
        gunParticles.Stop();
        gunParticles.Play();
        shootRay.origin = gunBarrelEnd.transform.position;
        shootRay.direction = gunBarrelEnd.transform.forward;
        gunLine.SetPosition(0, gunBarrelEnd.transform.position);
        if (Physics.Raycast(shootRay, out shootHit, range, shootableMask))
        {
            // EnemyHealth enemyHealth = shootHit.collider.GetComponent <EnemyHealth>();
            // if (enemyHealth != null)
            // {
            //     enemyHealth.TakeDamage(damagePerShot, shootHit.point);
            // }
            gunLine.SetPosition(1, shootHit.point);
        }
        else
        {
            gunLine.SetPosition(1, shootRay.origin + shootRay.direction * range);
        }
        gunLight.enabled = true;
        gunLine.enabled = true;
        yield return new WaitForSeconds(effectsDisplayTime);
        gunLine.enabled = false;
        gunLight.enabled = false;
        yield return new WaitForSeconds(fireRate - effectsDisplayTime);
        fsm.ChangeState(Shooting.name, Shooting.Idle);
    }
}
