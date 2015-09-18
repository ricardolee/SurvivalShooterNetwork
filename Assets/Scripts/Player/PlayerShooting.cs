using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using Toolkit;

[RequireComponent(typeof(StateManager))]
public class PlayerShooting : NetworkBehaviour
{
    public enum ShootingState
    {
        Idle, Fire
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

    [StateMachineInject]
    StateMachine<ShootingState> shootingSM;

    void Start()
    {
        shootableMask = LayerMask.GetMask("Shootable");
        gunBarrelEnd = transform.FindChild("GunBarrelEnd").gameObject;
        gunParticles = gunBarrelEnd.GetComponent<ParticleSystem>();
        gunLine = gunBarrelEnd.GetComponent<LineRenderer>();
        gunAudio = gunBarrelEnd.GetComponent<AudioSource>();
        gunLight = gunBarrelEnd.GetComponent<Light>();
        shootingSM.Init(ShootingState.Idle);
    }

    void Update()
    {
        if (isLocalPlayer)
        {
            if (Input.GetButton("Fire1"))
            {
                shootingSM.ChangeState(ShootingState.Fire);
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
         shootingSM.ChangeState(ShootingState.Fire);
    }

    [StateListener(state = ShootingState.Fire, on = StateEvent.Enter)]
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
            EnemyHealth enemyHealth = shootHit.collider.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                shootHit.collider.GetComponent<EventDispatcher>().Trigger(EnemyHealth.DamageEvent, damagePerShot, shootHit.point);
            }
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
        shootingSM.ChangeState(ShootingState.Idle);
    }
}
