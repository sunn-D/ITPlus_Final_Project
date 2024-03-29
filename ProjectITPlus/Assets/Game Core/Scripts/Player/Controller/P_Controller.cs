﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Observer;

public class P_Controller : MonoBehaviour, IDamageable {
    #region Init, Config
    //Config
    [Header("Config")]
    [SerializeField] P_Data data;
    [SerializeField] GameObject aliveGO;
    [SerializeField] GameObject deadGO;
    [SerializeField] Transform bombSpawn;
    [SerializeField] GameObject bombBar;
    [SerializeField] Animator animator;
    [SerializeField] CapsuleCollider2D capsule;
    [SerializeField] Core core;
    [SerializeField] P_InputHandle inputHandle;

    //Getter
    public GameObject AliveGO { get => aliveGO; }
    public GameObject DeadGO { get => deadGO; }
    public Transform BombSpawn { get => bombSpawn; }
    public GameObject BombBar { get => bombBar; }
    public Animator Animator { get => animator; }
    public CapsuleCollider2D Capsule { get => capsule; }
    public Core Core { get => core; }
    public P_InputHandle InputHandle { get => inputHandle; }

    //Health System
    [Header("Health System")]
    [SerializeField] int maxHealth;
    [SerializeField] int minHealth;
    public int CurrentHealth { get; private set; }
    public bool IsDead { get; private set; }
    #endregion

    #region State Machine
    public P_StateMachine StateMachine { get; private set; }

    //States
    public P_IdleState IdleState { get; private set; }
    public P_RunState RunState { get; private set; }
    public P_JumpState JumpState { get; private set; }
    public P_InAirState InAirState { get; private set; }
    public P_GroundState GroundState { get; private set; }
    public P_ThrowState ThrowState { get; private set; }
    public P_HitState HitState { get; private set; }
    public P_DeadState DeadState { get; private set; }
    public P_DoorInState DoorInState { get; private set; }
    public P_DoorOutState DoorOutState { get; private set; }

    //Hash param animator
    private readonly int idleParam = Animator.StringToHash("idle");
    private readonly int runParam = Animator.StringToHash("run");
    private readonly int jumpParam = Animator.StringToHash("jump");
    private readonly int inAirParam = Animator.StringToHash("inAir");
    private readonly int groundParam = Animator.StringToHash("ground");
    private readonly int throwParam = Animator.StringToHash("throw");
    private readonly int hitParam = Animator.StringToHash("hit");
    private readonly int deadParam = Animator.StringToHash("dead");
    private readonly int doorOutParam = Animator.StringToHash("doorOut");
    private readonly int doorInParam = Animator.StringToHash("doorIn");

    private void Awake () {
        StateMachine = new P_StateMachine();

        IdleState = new P_IdleState(this, data, idleParam, false);
        RunState = new P_RunState(this, data, runParam, false);
        JumpState = new P_JumpState(this, data, jumpParam, true);
        InAirState = new P_InAirState(this, data, inAirParam, false);
        GroundState = new P_GroundState(this, data, groundParam, true);
        ThrowState = new P_ThrowState(this, data, throwParam, false);
        HitState = new P_HitState(this, data, hitParam, false);
        DeadState = new P_DeadState(this, data, deadParam, false);
        DoorInState = new P_DoorInState(this, data, doorInParam, true);
        DoorOutState = new P_DoorOutState(this, data, doorOutParam, true);
    }
    #endregion

    #region Start
    private void Start () {
        aliveGO.SetActive(true);
        deadGO.SetActive(false);
        Capsule.direction = CapsuleDirection2D.Vertical;
        bombBar.SetActive(false);
        CurrentHealth = maxHealth;
        IsDead = false;

        StateMachine.Initialize(DoorOutState);

        this.PostEvent(EventID.OnPlay);
    }
    #endregion

    #region Update
    private void Update () {
        StateMachine.CurrentState.LogicUpdate();
        Core.LogicUpdate();
    }

    private void FixedUpdate () {
        StateMachine.CurrentState.PhysicsUpdate();
    }
    #endregion

    #region Destroy
    public void Destroy () {
        Destroy(aliveGO);
        Destroy(this);
    }
    #endregion

    #region Damageable
    public void Damageable (int dame, float xForce, float yForce) {
        CurrentHealth -= dame;
        if (CurrentHealth <= minHealth) {
            IsDead = true;
        }
        Animator.SetInteger("health", CurrentHealth);
        Core.Movement.SetZeroVelocity();
        Core.Movement.AddForce(new Vector2(xForce, yForce), ForceMode2D.Impulse);
        StateMachine.ChangeState(HitState);

        this.PostEvent(EventID.PlayerTakeDamage);
    }

    public void OnDead () {
        this.PostEvent(EventID.PlayerDead);
    }
    #endregion

    #region Upgrade or Item collect
    public void UpgradeJump () {
        JumpState.IncreaseMaxAmountOfJump();
    }

    public void CollectHeart () {
        if (CurrentHealth < maxHealth) {
            CurrentHealth++;
            this.PostEvent(EventID.PlayerCollectHeart);
        }
    }
    #endregion
}
