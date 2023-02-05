using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;


public class mCharacterMovements : MonoBehaviour
{
    public Animator anim;
    //BASEMOVEMENTS
    

    
    public Rigidbody2D _rb;
    public float _runSpeed = 5.0f;
    public bool _facingRight = true;
    Vector2 move;

   
    //COLLISIONNN
    //[SerializeField] LayerMask playerLayer;
    [SerializeField] Transform _groundCheckPoint;
    [SerializeField] LayerMask theGround;
    [SerializeField] LayerMask wallSlide;
    [SerializeField] LayerMask wallGrab;
    [SerializeField] float _groundCheckHit = 0.1f;
    //[SerializeField] bool isGrounded;
    public bool isGrounded;
    

    //JUMPPP

    [SerializeField] float _jumpForce = 5f;
    float _fallMultiplier = 2.5f;
    float _lowJumpMultiplier = 3f;
    [SerializeField] int _extraJumpsValue;
    public int _extraJumps;
     


    //GRAB && WALLSLIDING && WALLJUMP
    [SerializeField] float _grabRunIdle = 6;
    [SerializeField] float _grabTime = 0.7f;
    [SerializeField] float _grabTimeReset = 0.7f;
    [SerializeField] float _grabTimeOver = 0;
    [SerializeField] float _grabRunSpeed = 5.0f;

    [SerializeField] Transform _wallGrabPoint;
    [SerializeField] Transform _wallPendPoint;
    public bool _isPending;
    [SerializeField] bool _canGrab;
    public bool _isGrabbing;
    [SerializeField] float _slideSpeed = 1f;

    RaycastHit2D _canPend;
    [SerializeField] float _wallGrabCircleHit = 0.02f;
    float _wallPendHit = 0.1f;
    float _wallJumpTime = 0.2f;
    float _wallJumpCounter;
   
    
    
    //DASHHH

    [SerializeField] float _crossFade;
    [SerializeField] bool _canDash = true;
    [SerializeField] bool _isDashing = false;
    [SerializeField] float _dashingPower = 20f;
    [SerializeField] float _dashingTime = 0.17f;
    public float _dashingCooldown = 1.0f;
    [SerializeField] TrailRenderer _dashTr;

    //ATTACK 
    //[SerializeField] float _impulseForce = 1.0f;
    private  bool _isAttacking;
    public bool isAttacking
    {
        get { return _isAttacking; }
    set
    {
        _isAttacking = value;
        if (!_isAttacking)
        {
            _jumpForce = 12.0f;
        }
    }


    }
    public static mCharacterMovements instance;
    //___>> AttackHIT
    [SerializeField] Transform _attackPoint;
    [SerializeField] float _attackRange = 0.5f;
    [SerializeField] LayerMask enemyLayers;
    public int _attackDamage = 40;
    [SerializeField] ParticleSystem _hitEffect;
    //JUMPATTACK
    [SerializeField] bool _canJumpAttack;
    public bool _isJumpAttacking;
    [SerializeField] float _jumpAttackTime = 0.1f;
    [SerializeField] float _jumpAttackCD = 0.1f;
    [SerializeField] float _jumpAttackAnimation=1.0f;

    /// PROJECTILE CAST

    [SerializeField] Transform firePosition;
    [SerializeField] GameObject ballProjectile;
    public float _pushBack = 5.0f;


    [SerializeField] bool _canSmash = true;
    public bool _isSmashing;
    [SerializeField] float _smashingTime;
    [SerializeField] float _smashingCooldown;
    [SerializeField] float _fireRotationSpeed;

    Vector2 _smashDir;

    //BASHING
    
    [SerializeField] Transform _bashObj;
    [SerializeField] Transform _rayPosition;
    [SerializeField] float _radius;
    [SerializeField] GameObject _basheableObject;
    [SerializeField] bool _nearToBasheableObject;
    [SerializeField] bool _isChoosingDir;
    [SerializeField] bool _isBashing;
    [SerializeField] float _bashPower;
    [SerializeField] float _bashTime;
    [SerializeField] GameObject _arrow;// prefab mbola tsy ao anaty scene
    [SerializeField] GameObject _arrowClone;// object temporaire arrow
    Vector3 _bashDir;
    float _bashTimeReset;


    //EFFECT
    [SerializeField] ParticleSystem _burstSmash;

    //HEAVYATTACK && HOLD ATTACK
    
    //public float _holdFrame = 0.5f;
    //[SerializeField] float _holdDuration = 2.0f;
    //[SerializeField] AnimationCurve _slowDownCurve;

    //float _holdTimer = 0.0f;
    //bool _isHolding = false;

    
    //HEALHSYSTEM
    [SerializeField] int _playerMaxHealth =100;
    [SerializeField] int _playerCurrentHealth;

    [SerializeField] float _immuneTime = 0.4f;
    [SerializeField] float _immuneOver = 0.0f;
    [SerializeField] float _playerHittedDelay = 0.4f;
    public float _hitPushBack = 2;
    
    //ONHIT
    //[SerializeField] bool _isOnHit = false;

    //ENEMYSCRIPTS && TRAP
    [SerializeField] int _trapDamage01 = 20;
    
    

    private void Awake()
    {
        instance = this;

        _playerControlInput = new PlayerControllerInput();
    }
    void Start()
    {
        _bashTimeReset = _bashTime;
        _rb = GetComponent<Rigidbody2D>();
        anim = gameObject.GetComponentInChildren<Animator>();
        _playerCurrentHealth = _playerMaxHealth;
        _extraJumps = _extraJumpsValue;
        _arrowClone = Instantiate(_arrow, _bashObj);
        _arrowClone.SetActive(false);
        
        mCharacterFX._instanceFx._HealFx.Stop();
    }

    Vector2 _axisControl = new Vector2();

    // Update is called once per frame
    void Update()
    {
        
        if(_wallJumpCounter <=0)
        {

            if (_isDashing)
            {
                return;
            }

            if (_isSmashing)
            {
                return;
            }

            //get axis value from Input system
            _axisControl = _playerControlInput.Movement.Move.ReadValue<Vector2>();

            //------RUNNING
            //float x = isAttacking ? 0 : Input.GetAxisRaw("Horizontal");

            //disable horizontal movement if attacking
            //_axisControl.x = isAttacking ? 0 : _axisControl.x;

            //disable vertical movement if pending
            _axisControl.y = _isPending ? 0 : _axisControl.y;

            if (_isPending)
            {
                anim.SetBool("isExtraJump", false);
                _canDash = false;
                if (_facingRight)
                {
                    _axisControl.x = _axisControl.x < 0 ? 0 : _axisControl.x;
                    
                }
                else
                {
                    _axisControl.x = _axisControl.x > 0 ? 0 : _axisControl.x;
                }
            }
            float y = _isPending ? 0 : Input.GetAxisRaw("Vertical");

            //move = new Vector2(x, y);
            move = _axisControl;


            //if (_isBashing == false)
            _rb.velocity = (new Vector2(move.x * _runSpeed, _rb.velocity.y));


            //------FLIP_DIRECTION
            
            if ( !anim.GetCurrentAnimatorStateInfo(0).IsName("counterAttackAir") && !anim.GetCurrentAnimatorStateInfo(0).IsName("counterAttackGround") && !anim.GetCurrentAnimatorStateInfo(0).IsName("mCharacterJumpAttack01") && !anim.GetCurrentAnimatorStateInfo(0).IsName("HeroKnight_Hurt") && !anim.GetCurrentAnimatorStateInfo(0).IsName("healG") && 
                !anim.GetCurrentAnimatorStateInfo(0).IsName("transitionAttack01") && !anim.GetCurrentAnimatorStateInfo(0).IsName("transitionAttack02") && !anim.GetCurrentAnimatorStateInfo(0).IsName("transitionAttack03") && !anim.GetCurrentAnimatorStateInfo(0).IsName("HeroKnight_Attack1") && !anim.GetCurrentAnimatorStateInfo(0).IsName("HeroKnight_Attack2") && 
            !anim.GetCurrentAnimatorStateInfo(0).IsName("HeroKnight_Attack3") && !anim.GetCurrentAnimatorStateInfo(0).IsName("HeavyAttack") && _axisControl.x > 0 && !_facingRight)
            {
                Flip();

            }

            if (!anim.GetCurrentAnimatorStateInfo(0).IsName("counterAttackAir") && !anim.GetCurrentAnimatorStateInfo(0).IsName("counterAttackGround") && !anim.GetCurrentAnimatorStateInfo(0).IsName("mCharacterJumpAttack01") && !anim.GetCurrentAnimatorStateInfo(0).IsName("HeroKnight_Hurt") && !anim.GetCurrentAnimatorStateInfo(0).IsName("healG") && 
                !anim.GetCurrentAnimatorStateInfo(0).IsName("transitionAttack01") && !anim.GetCurrentAnimatorStateInfo(0).IsName("transitionAttack02") && !anim.GetCurrentAnimatorStateInfo(0).IsName("transitionAttack03") && !anim.GetCurrentAnimatorStateInfo(0).IsName("HeroKnight_Attack1") && !anim.GetCurrentAnimatorStateInfo(0).IsName("HeroKnight_Attack2") && 
            !anim.GetCurrentAnimatorStateInfo(0).IsName("HeroKnight_Attack3") &&  !anim.GetCurrentAnimatorStateInfo(0).IsName("HeavyAttack") && _axisControl.x < 0 && _facingRight)
            {
                Flip();
            }

            CheckSlidingWall();


            ///----->PENDING


            _canPend = Physics2D.Raycast(_wallPendPoint.position, new Vector2(0, _wallPendHit), _wallPendHit, wallGrab);

            Debug.DrawRay(_wallPendPoint.position, new Vector2(0, _wallPendHit), Color.yellow);

            _isPending = false;

            if (_canPend && !isGrounded)
            {

                if ((_facingRight && move.x > 0) || (!_facingRight && move.x < 0))
                {

                    _isPending = true;
                    _isGrabbing = false;
                    _canGrab = false;

                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        
                        _wallJumpCounter = _wallJumpTime;
                        _rb.velocity = new Vector2(0, _jumpForce);
                        _isPending = false;
                        _canDash = true;
                        _rb.gravityScale = 4;
                        _extraJumps = 0;

                    }
                }

                if (_isPending && move.x == 0)
                {
                    _isPending = false;
                }
                if (_isPending)
                {
                    _rb.gravityScale = 0;
                    _rb.velocity = Vector2.zero;

                }

                else
                {
                    _rb.gravityScale = 4;
                }
            }
            




            //------JUMPING

            // isGrounded = Physics2D.OverlapCircle(_groundCheckPoint.position, _groundCheckHit, theGround);

            if (_rb.velocity.y < 0)
            {
                _rb.velocity += Vector2.up * Physics2D.gravity.y * (_fallMultiplier - 1) * Time.deltaTime;
            }
            else if (_rb.velocity.y > 0 && !Input.GetKey(KeyCode.Space))
            {
                _rb.velocity += Vector2.up * Physics2D.gravity.y * (_lowJumpMultiplier - 1) * Time.deltaTime;
            }

           

            


            //PLAYERONHIT

            // PLAYERBASH
            Bash();


            ///SETANIMATOR
            anim.SetBool("isPending", _isPending);
            anim.SetBool("isGrabbing", _isGrabbing);
            //anim.SetFloat("yVelocity", _rb.velocity.y);
            anim.SetBool("isJumping", !isGrounded);
            
            anim.SetBool("isRunning", move.x != 0);
            //anim.SetBool("isDashing", _isDashing);


            if(anim.GetCurrentAnimatorStateInfo(0).IsName("HeavyAttack"))
            {
                _jumpForce =0.0f;
            }

                if (isGrounded )
            {
                anim.SetBool("isHeavyAirAttack", false);
                mCharacterFX._instanceFx._Slash04.Stop();
                anim.SetBool("isJumpAttacking", false);
                anim.SetBool("isExtraJump", false);
                anim.SetBool("isCounterAir", false);
                //mCharacterFX._instanceFx._Slash04.Stop();

            }
        }

        else
        {
            _wallJumpCounter -= Time.deltaTime;
        }

        //COUNTER SYSTEM
        Counter();

        //HEAL SYSTEM

        Heal();

        //HEAVY ATTACK
        HeavyAttack();

    }

    
    /// <summary>
    /// WALLJUMPING and SLIDING
    /// GRABBING
    /// </summary>
    private void CheckSlidingWall()
    {
        // var oldGrab = _canGrab;
        _canGrab = Physics2D.OverlapCircle(_wallGrabPoint.position, _wallGrabCircleHit, wallSlide);
        // if(!oldGrab){
        //     if(_canGrab){
        //         _grabTime = _grabTimeReset;
        //     }
        // }

        //_canDash = true;
        
        _isGrabbing = false;

        if (_canGrab && !_isPending)
        {
            _canDash = true;

            if ((_facingRight && move.x > 0) || (!_facingRight && move.x < 0) || move.x == 0)
            {
                _isGrabbing = true;
                _isPending = false;
                _canDash = false;



                if (_isGrabbing && move.x == 0)
                {
                    _grabTime -= Time.deltaTime;


                    if (_grabTime <= _grabTimeOver)
                    {
                        _isGrabbing = false;
                    }

                }
                else
                {
                    _grabTime = _grabTimeReset;
                }


                if (Input.GetKeyDown(KeyCode.Space))
                {
                    _wallJumpCounter = _wallJumpTime;

                    if (move.x == 0)
                    {
                        anim.SetBool("isJumping", true);
                        anim.Play("jumpFinal", -1, 0.3f);
                        print("jumpFinal");
                        if (_facingRight)
                        {
                            _rb.velocity = new Vector2(-_grabRunIdle, _jumpForce);
                            
                        }
                        else if (!_facingRight)
                        {
                            _rb.velocity = new Vector2(_grabRunIdle, _jumpForce);
                            
                        }

                    }

                    else
                    {
                        _rb.velocity = new Vector2(-move.x * _grabRunSpeed, _jumpForce);
                        

                    }

                    // anim.SetBool("isJumping", true);
                    _isGrabbing = false;
                    _canDash = true;
                    _rb.gravityScale = 4;
                    //_extraJumps = 0;
                    Flip();

                }

            }



        }


        //SLIDE DOWN SLOWLY IF ON BLUE WALL
        if (_isGrabbing)
        {
            _rb.velocity = new Vector2(_rb.velocity.x, -_slideSpeed);
            //_canDash = false;
            if (!grabbed)
            {
                grabbed = true;
                ungrabbed = false;
                _canDash = false;
                //_canDash = true;
                anim.SetBool("isExtraJump", false);

            }
        }
        else
        {
            if (!ungrabbed)
            {
                grabbed = false;
                ungrabbed = true;
                _canDash = true;
                anim.SetBool("isExtraJump", false);
            }
        }
    }
    [SerializeField] bool grabbed = false;
    [SerializeField] bool ungrabbed = false;


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(_wallGrabPoint.position, _wallGrabCircleHit);

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(_groundCheckPoint.position, _groundCheckHit);

        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(_attackPoint.position, _attackRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(_rayPosition.position, _radius);
    }
    private void Flip()
    {
        _facingRight = !_facingRight;
        transform.Rotate(0f, 180f, 0f);
        mCharacterFX._instanceFx._Slash01.Stop();
        mCharacterFX._instanceFx._Slash02.Stop();
        mCharacterFX._instanceFx._Slash03.Stop();
        mCharacterFX._instanceFx._Slash04.Stop();
    }


    



    public IEnumerator JumpAttack()
    {
        
        _canJumpAttack = false;
        _isJumpAttacking = true;
        
        anim.SetBool("isJumpAttacking", true);
        if(!isGrounded)
        {
            mCharacterFX._instanceFx._Slash04.Play();
        }
        else
        {
            mCharacterFX._instanceFx._Slash04.Stop();
        }
        anim.SetBool("isExtraJump", false);
        
        yield return new WaitForSeconds(_jumpAttackTime);
        _isJumpAttacking = false;

        anim.SetBool("isJumpAttacking", false);
        if(!isGrounded)
        {
            anim.Play("jumpFinal", 0, _jumpAttackAnimation);
        }
        
       

        anim.SetBool("isRunning", true);
        
        
        yield return new WaitForSeconds(_jumpAttackCD);
      

        _canJumpAttack = true;
        

    }

    public IEnumerator SmashBall()
    {
        _canSmash = false;
        _isSmashing = true;
        _rb.gravityScale = 0;
        //if(_facingRight)
        //{
        //    _rb.velocity = new Vector2(transform.localScale.x * -_pushBack, 0f);
        //}

        //if (!_facingRight)
        //{
        //    _rb.velocity = new Vector2(transform.localScale.x * _pushBack, 0f);
        //}

        
        anim.SetTrigger("isSmacking");
        
        yield return new WaitForSeconds(_smashingTime);
        _rb.gravityScale = 4;
        _isSmashing = false;

        yield return new WaitForSeconds(_smashingCooldown);
        _canSmash = true;

    }
    public void Cast()
    {

        _burstSmash.Play();
        Instantiate(ballProjectile, firePosition.position, firePosition.rotation);

    }

    public void HitEnem()
    {
        Collider2D[] _hitEnemies = Physics2D.OverlapCircleAll(_attackPoint.position, _attackRange, enemyLayers);

        
        foreach (Collider2D enemy in _hitEnemies)
            {
            _nearToBasheableObject = false;        
            enemy.GetComponent<enemyBehaviours>().TakeDamage(_attackDamage);
            _hitEffect.Play();
            mCharacterCamera._instanceCam.ShakeCamera(2f, 0.1f);
            
            }
      

    }


    void Bash()
    {
        
        RaycastHit2D[] _rays = Physics2D.CircleCastAll(_rayPosition.position, _radius, Vector3.forward);
        foreach (RaycastHit2D _ray in _rays)
        {

            _nearToBasheableObject = false;

            if (_ray.collider.tag == "basheable")
            {

                _nearToBasheableObject = true;
                _basheableObject = _ray.collider.transform.gameObject;
                break;
            }
        }
        if (_nearToBasheableObject)
        {
            _basheableObject.GetComponent<SpriteRenderer>().color = Color.yellow;
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                Time.timeScale = 0.05f;
                _arrowClone.SetActive(true);
                _arrowClone.transform.position = _basheableObject.transform.position;
                _isChoosingDir = true;

            }

            else if (_isChoosingDir && Input.GetKeyUp(KeyCode.Mouse1))
            {
                Time.timeScale = 1f;
                _isChoosingDir = false;
                _isBashing = true;
                _rb.velocity = Vector2.zero;
                transform.position = _basheableObject.transform.transform.position;
               
                _bashDir = Camera.main.ScreenToWorldPoint (Input.mousePosition)  - transform.position;
                

                _bashDir.z = 0;
                if (_bashDir.x < 0 && _facingRight)
                {
                    Flip();
                }
                else if (_bashDir.x > 0 && !_facingRight)
                {
                    Flip();

                }

                _bashDir = _bashDir.normalized;
                _basheableObject.GetComponent<Rigidbody2D>().AddForce(-_bashDir * 0.5f, ForceMode2D.Impulse);
                _arrowClone.SetActive(false);
                //anim.SetBool("isJumping", true);

                anim.SetBool("isJumping", true);
                anim.Play("jumpFinal",-1,0.3f);
                print("jumpFinalBash");
            }

        }
        else if (_basheableObject != null)
        {
            _basheableObject.GetComponent<SpriteRenderer>().color = Color.white;
            Time.timeScale = 1f;
            _arrowClone.SetActive(false);
        }

        //PerformBash

        if (_isBashing)
        {
            if (_bashTime > 0)
            {
                _bashTime -= Time.deltaTime;
                _rb.velocity = _bashDir * _bashPower;
            }
            else
            {
                _isBashing = false;
                _bashTime = _bashTimeReset;
                _rb.velocity = new Vector2(-_rb.velocity.x, 0);

            }
        }

    }


        public void PlayerHited(int Hited)
    {
                
        _playerCurrentHealth -= Hited;
        //anim.SetTrigger("isHitted");

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("HeroKnight_Hurt") && _immuneTime > 0)
        {
            _immuneTime -= Time.deltaTime;
            
            
            Physics2D.IgnoreLayerCollision(11, 9, true);
            Physics2D.IgnoreLayerCollision(11, 15, true);
            
            
            if (_immuneTime == _immuneOver)
            {
                
                Physics2D.IgnoreLayerCollision(11, 9, false);
                Physics2D.IgnoreLayerCollision(11, 15, false);
                
            }
        }
        if (_playerCurrentHealth <= 0)
        {
            PlayerDie();
        }
    }

 
    public void PlayerDie()
    {
        //anim.CrossFadeInFixedTime("playerDeath", 0.1f);
        anim.SetBool("isDead", true);
        _rb.velocity = Vector2.zero;
        this.enabled = false;
        Physics2D.IgnoreLayerCollision(11,9,true);
        Destroy(gameObject, 2);
        GameManager._gameManager.StartPlayerSpawning();
    }

    private void OnCollisionEnter2D(Collision2D _coll2D)
    {
        if(_coll2D.gameObject.tag == "DeathPit")
        {
            PlayerDie();
            Physics2D.IgnoreLayerCollision(11, 9, true);
            Physics2D.IgnoreLayerCollision(11, 14, true);
            
        }

       
    }

    private void OnTriggerEnter2D(Collider2D _trigger2D)
    {
        if (_trigger2D.gameObject.tag == "Trap")
        {

            

            _playerCurrentHealth -= _trapDamage01;
            
            anim.SetTrigger("isHitted");
            
            _runSpeed =0.0f;
            _extraJumps = 0;
           

            if (_facingRight)
            {
                transform.Translate(Vector2.left * _hitPushBack * Time.deltaTime);
                
                
            }
            else if (!_facingRight)
            {
                transform.Translate(Vector2.right * -_hitPushBack * Time.deltaTime);
            }



            if (_playerCurrentHealth <= 0)
            {
                Physics2D.IgnoreLayerCollision(11, 15, true);
                PlayerDie();
            }
        }
    }

   
    public IEnumerator PlayerOnStepHit()
    {
        _playerCurrentHealth -= 5;
        anim.SetTrigger("isHitted");
        Physics2D.IgnoreLayerCollision(11, 9, true);
        yield return new WaitForSeconds(_playerHittedDelay);
        Physics2D.IgnoreLayerCollision(11, 9, false);
        anim.ResetTrigger("isHitted");
    }


    public void ResetExtraJump()
    {
        ////CHECK IF ON THE GROUND THEN GET ONE JUMP AVAILABLE
        _extraJumps = _extraJumpsValue;
    }

    
    /* NEW INPUT SYSTEM */
    // CONTROLLER
    PlayerControllerInput _playerControlInput;

    private void OnEnable()
    {
        _playerControlInput.Actions.Enable();

        //Jump
        _playerControlInput.Actions.Jump.Enable();
        _playerControlInput.Actions.Jump.performed += HandleJump;

        //Dash
        _playerControlInput.Actions.Dash.Enable();
        _playerControlInput.Actions.Dash.performed += HandleDash;

        //Attack
        _playerControlInput.Actions.Attack.Enable();
        _playerControlInput.Actions.Attack.performed += HandleAttack;

        //Smash
        _playerControlInput.Actions.Smash.Enable();
        _playerControlInput.Actions.Smash.performed += HandleSmash;

        //Movement
        _playerControlInput.Movement.Enable();
        _playerControlInput.Movement.Move.Enable();
    }

    

    private void OnDisable()
    {
        _playerControlInput.Actions.Disable();

        //Jump
        _playerControlInput.Actions.Jump.Disable();
        _playerControlInput.Actions.Jump.performed -= HandleJump;

        //Dash
        _playerControlInput.Actions.Dash.Disable();
        _playerControlInput.Actions.Dash.performed -= HandleDash;

        //Attack
        _playerControlInput.Actions.Attack.Disable();
        _playerControlInput.Actions.Attack.performed -= HandleAttack;

        //Smash
        _playerControlInput.Actions.Smash.Disable();
        _playerControlInput.Actions.Smash.performed -= HandleSmash;

        //Movement
        _playerControlInput.Movement.Disable();
        _playerControlInput.Movement.Move.Disable();
    }

    private void HandleJump(InputAction.CallbackContext obj)
    { 

        //if grounded we can always jump
        if (isGrounded)
        {
            _rb.velocity = Vector2.up * _jumpForce;
        }


        else if (_extraJumps > 0 && !_isPending && !_isGrabbing)
        {
            _rb.velocity = Vector2.up * _jumpForce;
            
            _extraJumps--;
            anim.SetBool("isJumping", false);
            anim.SetBool("isExtraJump", true);
            anim.CrossFadeInFixedTime("jumpFinal", 0.1f);

            mCharacterFX._instanceFx.JumpFx();
            
            
        }


    }

    private void HandleDash(InputAction.CallbackContext obj)
    {
        if (_canDash)
        {
            StartCoroutine(Dash());
        }
    }


    private IEnumerator Dash()
    {
        int _jumpStock = _extraJumps;
        _canDash = false;
        _isDashing = true;
        _rb.gravityScale = 0.0f;
        _extraJumps = 0;
        anim.SetBool("isDashing", true);
        anim.SetBool("isExtraJump",false);
        anim.SetBool("isJumping", false);
        _isJumpAttacking = false;
        

        if (_facingRight)
        {
            _rb.velocity = new Vector2(transform.localScale.x * _dashingPower, 0f);
        }
        
        if(!_facingRight)
        {
            _rb.velocity = new Vector2(transform.localScale.x * -_dashingPower, 0f);

        }

       
        Physics2D.IgnoreLayerCollision(11, 9, true);



        yield return new WaitForSeconds(_dashingTime);

        _isJumpAttacking = true;
        _extraJumps = _jumpStock;
        _rb.gravityScale = 4;
        _isDashing = false;
        anim.SetBool("isDashing",false);
        
        anim.SetBool("isExtraJump",false);
        
        
        Physics2D.IgnoreLayerCollision(11, 9, false);
        yield return new WaitForSeconds(_dashingCooldown);
        _canDash = true;
        
        
    }
    private void HandleAttack(InputAction.CallbackContext obj)
    {
        if (!isAttacking)
        {
            if (isGrounded)
            {
                _jumpForce = 0.0f;
                isAttacking = true;
                _isGrabbing = false;
                _isPending = false;
            }

            else if (_canJumpAttack && !_isPending && !_isGrabbing)
            {
                
                StartCoroutine(JumpAttack());
            }
        }
    }

    private void HandleSmash(InputAction.CallbackContext obj)
    {
        if (_canSmash)
        {
            _smashDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

            firePosition.transform.Rotate(_smashDir);
            StartCoroutine(SmashBall());
        }
    }

    void Counter()
    {
        if(Input.GetKeyDown(KeyCode.C) && isGrounded)
        {
            anim.SetTrigger("isCounterGround");
            _runSpeed = 0.0f;
            //im.SetBool("isCounterAir", false);
        }

        if(Input.GetKeyDown(KeyCode.C) && !isGrounded)
        {
            anim.SetBool("isCounterAir", true);
        }
    }


    void Heal()
    {
        if(Input.GetKeyDown(KeyCode.H) && isGrounded)
        {
            anim.SetTrigger("isHeal");
            _runSpeed =0.0f;
            mCharacterFX._instanceFx._HealFx.Play();
        }
    }

    void HeavyAttack()
    {
        if(Input.GetKeyDown(KeyCode.A) && isGrounded && !anim.GetCurrentAnimatorStateInfo(0).IsName("HeroKnight_Attack1") && !anim.GetCurrentAnimatorStateInfo(0).IsName("HeroKnight_Attack2") && !anim.GetCurrentAnimatorStateInfo(0).IsName("HeroKnight_Attack3"))
        {
            anim.SetTrigger("isHeavyGroundAttack");
            
            _runSpeed = 0.0f;
            mCharacterFX._instanceFx._heavySlash.Play();
            
        }


    }

    

}
