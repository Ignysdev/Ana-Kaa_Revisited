using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Analog;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class movimento : MonoBehaviour
{
   
    // ANOTA��ES
    /* 
        Todos os c�digos considerados como importantes e/ou reutiliz�veis
    foram comentados e guardados no fim do c�digo para caso haja necessidade
    de usar eles depois.

        Existem marca��es no c�digo indicando onde estavam estes c�digos.
    - Iggy
    */
    
    //  //  // DECLARA��ES //  //  //
    #region 
    // INSPECTOR
    [Header("Movimento geral")]
    [SerializeField, Tooltip("Velocidade do movimento"), Range(0.01f, 7f)] float speed = 1.7f;
    [SerializeField, Tooltip("suaviza��o do movimento"), Range(0, 0.3f)] float smooth = .1f;
    [SerializeField, Tooltip("Layer que representa o ch�o")] LayerMask mask;
    [SerializeField, Tooltip("Objeto vazio que detecta o ch�o")] public Transform groundCheck;
    [SerializeField, Tooltip("Tamanho do detector do ch�o")] float detectorChao = .2f;

    [Header("Pulo")]
    [SerializeField, Tooltip("For�a do movimento")] float jumpForce = 16;
    [SerializeField, Tooltip("Suaviza��o no ar (porcentagem em rela��o � suaviza��o padr�o)")] float airSmooth = 3;
    [SerializeField, Tooltip("For�a do corte de pulo (soltar bot�o)"), Range(0, 1)] float jumpCutHeight = .87f;
    [SerializeField, Tooltip("Tempo em segundos de imprecis�o permitido"), Range(0, .7f)] float toleranciaPulo = .2f;

    [Header("Dash")]
    [SerializeField, Tooltip("Velocidade do dash"), Range(1, 5)] float forcaDash = 1.37f;
    [SerializeField, Tooltip("Dist�ncia percorrida pelo dash"), Range(0, 10)] float distanciaDash = 4f;
    [SerializeField, Tooltip("Tempo de recarga do dash em segundos"), Range(0, 10)] float cooldownDash = 1f;

    [Header("Tiro")]
    [SerializeField, Tooltip("Tempo do cooldown"), Range(10, 20)] int tiro_limite ;
    float tiro_time;
    

    // OBJETOS EXTERNOS
    Collider2D attack;          // Colisor do ataque
    Rigidbody2D rb;             // RigidBody deste objeto
    Animator animin;// Componente de anima��o
    AudioSource audioSource;
    //Collider2D trig;            // Trigger para atravesar o ch�o
    BoxCollider2D col;      // Colisor do objeto
    Transform deathObj;
    Cutscenemanagement cutscene;

    // VARI�VEIS AUXILIARES
        // movimento
    bool ladoDir = true;    // Virado para a direita?
    bool ativajoy = true;
        // pulo
    bool liberaChao = false;    // Est� em contato com o ch�o?
    bool jumpCut = false;       // Bot�o pulo pressionado ainda?
    float prePulo, puloPos;     // Tempo de imprecis�o do pulo
    float puloPosParede;        // Tempo de imprecis�o
    public bool trig = false;   // Trigger para atravessar o ch�o

        // dash
    public bool dash = false;   // Dash liberado?
    bool dashing = false;       // Est� fazendo dash?
    Vector3 dashStartPos;       // Posi��o de in�cio do dash
    Vector2 dir;                // Dire��o do dash
    float dd;                   // Dist�ncia � percorrer
    bool dashTrig;              // Auxiliar de finaliza��o

        // outros
    static Vector2 respawn;                 // Posi��o de respawn
    private Vector3 vel = Vector3.zero;     // Burocracia de c�digo
    private Vector3 vel2 = Vector3.zero;
    bool alive = true;
    bool dying = false;
    bool anima_andar;

        // Revisit
    private Vector2 movementInput;
    public int t_camera = 0;
    private bool cameraCoold = false;
    private bool cc = false;
    #endregion

    //  //  // FUN��ES PRINCIPAIS //  //  //
    #region

    void Start()
    {
        //  Coleta todos os componentes necess�rios.
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = Resources.Load<AudioClip>("Efeitos sonoros/Tainara/Correndo grama");
        rb = GetComponent<Rigidbody2D>();
        animin = GetComponent<Animator>();
        attack = GameObject.Find("melee zone").GetComponent<Collider2D>();
        deathObj = GameObject.FindGameObjectWithTag("deathHud").GetComponent<Transform>();
        //trig = groundCheck.GetComponent<Collider2D>();
        col = GetComponent<BoxCollider2D>();
        cutscene = GetComponent<Cutscenemanagement>();

        PlayerPrefs.SetInt("fase", SceneManager.GetActiveScene().buildIndex);
        if(PlayerPrefs.HasKey("x"))
        {
            respawn = new Vector2(PlayerPrefs.GetFloat("x"), PlayerPrefs.GetFloat("y"));
        }
        else
        {
            respawn = transform.position;
        }

        //  Move para o spawnpoint e desativa colisor de combate.
        transform.position = respawn;
        attack.enabled = false;

        // Anima��o do come�o
        StartCoroutine(Death(true));
    }

    void FixedUpdate()
    {
        
       
        animin.SetFloat("Yvel", rb.linearVelocity.y);
        float tiro_fim = Mathf.FloorToInt(tiro_time);
        
        //GameObject.Find("tirobt").GetComponent<Image>().fillAmount = tiro_fim /  tiro_limite;
        // Cron�metros
        prePulo -= Time.fixedDeltaTime; puloPos -= Time.fixedDeltaTime; puloPosParede -= Time.fixedDeltaTime;

        // COLISOR CH�O
        if(Physics2D.OverlapCircle(groundCheck.position, detectorChao, mask))   //  Cria um c�rculo de colis�o com base nos par�metros predefinidos.
        {
            //  Se no ch�o seta a vari�vel e ativa a toler�ncia de pulo.
            if(liberaChao != true)
            {
                audioSource.clip = Resources.Load<AudioClip>("Efeitos_sonoros/Tainara/Queda_grama");
                audioSource.loop = false;

                audioSource.Play();
            }
            liberaChao = true;
            
            if (rb.linearVelocity.y < 0.3f)
            {
                animin.SetBool("jumping", false);
            }
            puloPos = toleranciaPulo;
            
        }
        else
        {

            //  Se n�o, seta vari�vel.
            liberaChao = false;
        }

        // COLISOR PAREDE
        if (Physics2D.OverlapCircle(attack.transform.position, detectorChao, mask))   //  Cria um c�rculo de colis�o com base nos par�metros predefinidos.
        {
            //  Se na parede ativa a toler�ncia de pulo.
            puloPosParede = toleranciaPulo;
        } 

        // PULO
        if (prePulo > 0 && puloPos > 0 && alive) 
        {
            animin.SetBool("jumping", true);
            animin.SetBool("walking", false);
            animin.SetBool("attacking", false);
            audioSource.clip = Resources.Load<AudioClip>("Efeitos_sonoros/Tainara/Pulo");
            audioSource.loop = false;
            audioSource.Play();
            //  Caso tenha apertado em pulo e estado no ch�o em um tempo menor que a tolerancia de pulo, pule e ajuste as vari�veis.
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            puloPos = 0;
            prePulo = 0;
            liberaChao = false;
        }

        // PULO NA PAREDE
        if (prePulo > 0 && puloPosParede > 0 && puloPos <= 0 && alive)
        {
            //  Caso tenha apertado em pulo e estado na parede em um tempo menor que a tolerancia de pulo, pule na dire��o oposta da parede e ajuste as vari�veis.
            if (ladoDir)
            {
                rb.linearVelocity = new Vector2(-jumpForce, jumpForce  /1.3f);
            }
            else
            {
                rb.linearVelocity = new Vector2(jumpForce, jumpForce / 1.3f);
            }
            Flip();
            //StartCoroutine(movimentoParede());
            puloPosParede = 0;
            prePulo = 0;
        }

            // Suaviza��o no ar
        float s = smooth;   //  Vari�vel de suaviza��o usada no c�digo.
        if (!liberaChao)
        {
            // Se estiver no ar, atualize suaviza��o.
            s *= airSmooth;
        }

        // MOVIMENTO
        if(movementInput.x > 0.2 && alive || movementInput.x < -0.2 && alive)
        {
            int sign = -1;
            if(movementInput.x > 0)
            {
                sign = 1;
            }
            if(liberaChao == true)
            {
                anima_andar = true;
                audioSource.clip = Resources.Load<AudioClip>("Efeitos_sonoros/Tainara/Correndo_grama");
                audioSource.loop = true;
                if(audioSource.isPlaying == false)
                {
                    audioSource.Play();
                }
                
            }
           

            Vector3 velocidade = Vector3.zero;

            if (Mathf.Abs(movementInput.x) > 0.75f)
            {
                velocidade = new Vector2(5 * sign * speed, rb.linearVelocity.y);   //  Vetor da velocidade.
            }
            else
            {
                velocidade = new Vector2(1.5f * sign * speed, rb.linearVelocity.y);   //  Vetor da velocidade.
            }
            rb.linearVelocity = Vector3.SmoothDamp(rb.linearVelocity, velocidade, ref vel, s);  //  Aplica��o da suaviza��o.
        }
        else
        {
            if(alive)
            rb.linearVelocity = Vector3.SmoothDamp(rb.linearVelocity, new Vector3(0, rb.linearVelocity.y), ref vel, s);  //  Aplica��o da suaviza��o.
        }
        if(movementInput.x == 0 && anima_andar == true || liberaChao == false && anima_andar == true)
        {
            audioSource.Stop();
            audioSource.loop = false;
            anima_andar = false;
        }
        

            // Corte de pulo
        if (jumpCut && rb.linearVelocity.y > 0 && alive)
        {
            // Se parou de clicar em pular e subindo, diminua velocidade.
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutHeight);
        }

        // LIMITE QUEDA
        if(rb.linearVelocity.y < -20)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -20);
        }

        //�1 

        // DASH
        Dash(true);
    }

    private void Update()
    {
        // ELIMINAR EXPLOIT
        if (!animin.GetBool("attacking"))
        {
            attack.enabled = false;
        }


        if (tiro_time < tiro_limite)
        {
            tiro_time += Time.deltaTime;
        }
        // VIRAR PERSONAGEM
        if (movementInput.x > 0.2 /*&& ativajoy == true*/)
        {
            //  Se estiver andando para a direita rode a anima��o.
            animin.SetBool("walking", true);
            if (!ladoDir)
            {
                //  Se estiver virado para o lado errado, vire.
                Flip();
            }
        }
        else if (movementInput.x < -0.2 /*&& ativajoy == true*/)
        {
            //  Se estiver andando para a esquerda rode a anima��o.
            animin.SetBool("walking", true);
            if (ladoDir)
            {
                //  Se estiver virado para o lado errado, vire.
                Flip();
            }
        }
        else
        {
            //  Se estiver parado, pare a anima��o de andar.
            animin.SetBool("walking", false);
        }

        if (cutscene.movement_locked)
            movementInput = Vector2.zero;

        if (PlayerPrefs.GetInt("pause") == 1)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }

    }

    public void OnMove(InputAction.CallbackContext _callbackContext)
    {
        if (!cutscene.movement_locked && PlayerPrefs.GetInt("pause") == 0)
        {
            movementInput = _callbackContext.ReadValue<Vector2>();
            CameraMovement();
        }
        else 
        {
            movementInput = Vector2.zero;
        }
    }

    public void OnJump(InputAction.CallbackContext _callbackContext)
    {
        if (!cutscene.movement_locked && PlayerPrefs.GetInt("pause") == 0)
        {
            if(_callbackContext.started)
                Pulo(true);
            if (_callbackContext.canceled)
                Pulo(false);
        }
    }

    public void OnAttack(InputAction.CallbackContext _callbackContext)
    {
        if (_callbackContext.started && !cutscene.movement_locked && PlayerPrefs.GetInt("pause") == 0)
            Ataque();
    }
    #endregion

    //  //  // PROCEDIMENTOS //  //  //
    #region
    private void Flip()
    {
        //  Mude o lado na vari�vel.
        ladoDir = !ladoDir;

        //  Muda o lado do objeto usando escala.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    public void Pulo(bool on)
    {
        if (on)
        {
            //  Se apertado, atualiza vari�vel de apertando.
            jumpCut = false;
            if (movementInput.y > -0.75f)
            {
                //  Se n�o estiver olhando para baixo pule antes da toler�ncia acabar.
                prePulo = toleranciaPulo;
            }
            else
            {
                //  Se olhando para baixo, lige colisor que desativa ch�o.
                if (GameObject.FindGameObjectWithTag("chao_atravessavel"))
                {
                    GameObject.FindGameObjectWithTag("chao_atravessavel").GetComponent<Collider2D>().usedByEffector = false;
                    GameObject.FindGameObjectWithTag("chao_atravessavel").layer = 8;
                }
                //trig = true;
                liberaChao = false;
                StartCoroutine(LigarChao(false));
            }
        }
        else
        {
            //  Se soltado, interrompa pulo.
            jumpCut = true;
        }
        //�2
    }

    public void Ataque()
    {
        //  Se bot�o de ataque apertado, rode anima��o e ligue colisos de ataque.
        attack.enabled = true;
        animin.SetBool("attacking", true);
        animin.SetBool("jumping", false);

    }

    public void Ataque_som1()
    {
        audioSource.clip = Resources.Load<AudioClip>("Efeitos_sonoros/Tainara/ataque_leve");
        audioSource.loop = false;
        
            audioSource.Play();
        
    }

    public void Ataque_som2()
    {
        audioSource.clip = Resources.Load<AudioClip>("Efeitos_sonoros/Tainara/ataque_pesado");
        audioSource.loop = false;

        audioSource.Play();

    }

    public void Tiro()
    {
        if (tiro_time >= tiro_limite)
        {
            Instantiate(Resources.Load("prefabio/tiro"), transform.position, Quaternion.identity);
            tiro_time = 0;
        }
        
    }

    void AtaqueFim(string sonio)
    {
        //  Quando ataque terminado, desligue colisor e anima��o.
        animin.SetBool("attacking", false);
        attack.enabled = false;
    }

    public void Dash(bool mode)
    {
        if (!mode && dash)
        {
            //  Se em in�cio, ligue dashing, desligue vari�vel interferente, ative cooldown e lembre a posi��o inicial.
            dash = false;
            dashing = true;
            jumpCut = false;
            dashStartPos = transform.position;
            Debug.Log("d0");
            StartCoroutine(DashCooldown());
            RaycastHit2D hit;

            if (Mathf.Abs(joystick.horizontal) + Mathf.Abs(joystick.vertical) > 2)
            {
                //  Se joystick estiver apontando, pegue dire��o e guarde se na dist�ncia do dash tem obst�culos.
                dir = Vector2.ClampMagnitude(new Vector2(joystick.horizontal, joystick.vertical), 1);
                //*1*
                //hit = Physics2D.CapsuleCast(new Vector2(transform.position.x, transform.position.y) + col.offset, col.size, col.direction, 0, new Vector2(joystick.horizontal, joystick.vertical), distanciaDash + 1.3f);
                hit = Physics2D.BoxCast(new Vector2(transform.position.x, transform.position.y) + col.offset, col.size, 0, new Vector2(joystick.horizontal, joystick.vertical), distanciaDash + 1.3f);
            }
            else if (ladoDir)
            {
                //  Se n�o e estiver virado para a direita, use dire��o direita e guarde se na dist�ncia do dash tem obst�culos.
                dir = Vector2.right;
                //*2*
                //hit = Physics2D.CapsuleCast(new Vector2(transform.position.x, transform.position.y) + col.offset, col.size, col.direction, 0, Vector2.right, distanciaDash + 1.3f);
                hit = Physics2D.BoxCast(new Vector2(transform.position.x, transform.position.y) + col.offset, col.size, 0, Vector2.right, distanciaDash + 1.3f);
            }
            else
            {
                //  Se n�o, use dire��o esquerda e guarde se na dist�ncia do dash tem obst�culos.
                dir = Vector2.left;
                //*3*
                //hit = Physics2D.CapsuleCast(new Vector2(transform.position.x, transform.position.y) + col.offset, col.size, col.direction, 0, Vector2.left, distanciaDash + 1.3f);
                hit = Physics2D.BoxCast(new Vector2(transform.position.x, transform.position.y) + col.offset, col.size, 0, Vector2.left, distanciaDash + 1.3f);
            }

            if (hit)
            {
                //  Se tiver obst�culos, dash at� o obst�culo.
                dd = hit.distance - 1.3f;
            }
            else
            {
                //  Se sem obst�culos, dash at� dist�ncia m�xima.
                dd = distanciaDash;
            }

            if (joystick.vertical < -3 || !liberaChao)
            {
                //  Se apontado para o ch�o ou n�o estiver no ch�o, permitir atravessar.
                //trig = true;
                if (liberaChao)
                {
                    //  Se no ch�o, regular for�a e atualizar vari�vel.
                    dd += 1.3f;
                    liberaChao = false;
                }
            }
            else if (joystick.vertical < 0 && liberaChao)
            {
                //  Se no ch�o e apontando para o ch�o, ajusta dire��o para puramente horizontal.
                dir = Vector2.ClampMagnitude(new Vector2(joystick.horizontal * 5, 0), 1);
            }
        }
        else // ---------------------------------------------------------------------------------------------------------------------------------------
        {
            //  Ap�s in�cio
            if (dashing && Vector2.Distance(transform.position, dashStartPos) < dd)
            {
                //  Se n�o alcan�ou o destino do dash, continue.
                rb.linearVelocity = Vector2.zero;
                rb.linearVelocity = new Vector2(dir.x * jumpForce * forcaDash, dir.y * jumpForce * forcaDash);
                dashTrig = true;
            }
            else
            {
                if (dashTrig && Vector2.Distance(transform.position, dashStartPos) > dd - .2f)
                {
                    //  Se dash acabou e passou 0.2 do alvo, desligue atravesar o ch�o e desacelere.
                    //StartCoroutine(LigarChao(false));
                    dashTrig = false;
                    jumpCut = true;
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x * .73f, rb.linearVelocity.y * .87f);
                }
                //  Atualize status do dash
                dashing = false;
            }
        }
    }

    void CameraMovement()
    {
        if (movementInput.y > 0.75f || movementInput.y < -0.75f)
        {
            if (cameraCoold)
            {
                if (movementInput.y > 0)
                {
                    if (t_camera != 2)
                    {
                        //cutscene.GetCutscene("LookUpJoy", 2);
                        t_camera = 2;
                    }
                }
                else
                {
                    if (t_camera != 1)
                    {
                        //cutscene.GetCutscene("LookDownJoy", 2);
                        t_camera = 1;
                    }
                }
            }
            else
            {
                if (!cc)
                {
                    StartCoroutine(CameraCooldown());
                }
            }
        }
        else
        {
            cc = false;
            cameraCoold = false;
            if (t_camera != 0)
            {
                t_camera = 0;
            }
        }
    }

    IEnumerator CameraCooldown()
    {
        cc = true;
        yield return new WaitForSeconds(1.3f);
        cameraCoold = true;

    }

    //�3


    //�4
    #endregion

    //  //  // PROCEDIMENTOS DO UNITY //  //  //
    #region

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("dano") && !dashing)
        {
            // Se atingido fora do dash, respawne.
            animin.SetTrigger("death");
            rb.gravityScale = 0;
            rb.linearVelocity = ((-collision.transform.position + transform.position).normalized*6);
            StartCoroutine(Death(false));
        }
        if (collision.gameObject.CompareTag("music_fade_trigger"))
        {
            StartCoroutine(Music_Controller.fade_out());
            
        }
        if (collision.gameObject.CompareTag("musica_boss_trigger"))
        {
            Music_Controller.OnBossEnter();

        }
        if (collision.gameObject.CompareTag("Respawn"))
        {
            // Se encostou em checkpoint, d� o checkpoint.
            StartCoroutine(Checkpoint());
        }

        if(collision.gameObject.layer == 9)
        {
            cutscene.GetCutscene(collision.gameObject.tag);
        }
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("dano") && !dashing)
        {
            // Se atingido fora do dash, respawne.
            animin.SetTrigger("death");
            rb.gravityScale = 0;
            rb.linearVelocity = ((-collision.transform.position + transform.position).normalized*6);
            StartCoroutine(Death(false));
        }
    }


    private void OnTriggerExit2D(Collider2D collision)
    {
        //  Ap�s passar do ch�o, ligue ele de novo.
        //StartCoroutine(LigarChao(true));
    }
    #endregion

    //  //  // COROUTINES //  //  //
    #region
    /*IEnumerator movimentoParede()
    {
        if ((rb.velocity.x > 0 && joystick.horizontal < 0) || (rb.velocity.x < 0 && joystick.horizontal > 0))
        {
            bool dir = ladoDir;
            ativajoy = false;
            speed = speed * -1;
            yield return new WaitUntil(() => liberaChao == true || joystick.horizontal == 0 || dir != ladoDir); //(rb.velocity.x <= -2.3f && joystick.horizontal < 0) || (rb.velocity.x >= 2.3f && joystick.horizontal > 0));
            speed = speed * -1;
            ativajoy = true;
        }
        yield return null;
    }
    */
    

    IEnumerator Checkpoint()
    {
        //  Espere at� estar no ch�o e ative o checkpoint.
        yield return new WaitUntil(()=> liberaChao);
        respawn = transform.position;
        PlayerPrefs.SetFloat("x", respawn.x);
        PlayerPrefs.SetFloat("y", respawn.y);
        PlayerPrefs.Save();

        yield return null;
    }
  
    
    public void LC()
    {
        if (GameObject.FindGameObjectWithTag("chao_atravessavel"))
        {
            GameObject.FindGameObjectWithTag("chao_atravessavel").GetComponent<Collider2D>().usedByEffector = false;
            GameObject.FindGameObjectWithTag("chao_atravessavel").layer = 8;
        }
        //trig = true;
        liberaChao = false;
        StartCoroutine(LigarChao(false));
    }


    IEnumerator LigarChao(bool instantaneo)
    {
        if (!instantaneo)
        {
            //  Se n�o for instant�neo, espere e desligue.
            yield return new WaitForSeconds(0.5f);
            if (GameObject.FindGameObjectWithTag("chao_atravessavel"))
            {
                GameObject.FindGameObjectWithTag("chao_atravessavel").GetComponent<Collider2D>().usedByEffector = true;
                GameObject.FindGameObjectWithTag("chao_atravessavel").layer = 7;
            }
            //trig = false;
        }
        else
        {
            //  Se for intant�neo, desligue.
            if (GameObject.FindGameObjectWithTag("chao_atravessavel"))
            {
                GameObject.FindGameObjectWithTag("chao_atravessavel").GetComponent<Collider2D>().usedByEffector = true;
                GameObject.FindGameObjectWithTag("chao_atravessavel").layer = 7;
            }
            //trig = false;
        }
        yield return null;
    }


    IEnumerator DashCooldown()
    {
        //  Desliga o dash, espera e liga de novo.
        dash = false;
        Debug.Log("d1");
        yield return new WaitForSeconds(cooldownDash);
        Debug.Log("d2");
        yield return new WaitUntil(() => liberaChao && !dash);
        Debug.Log("d3");
        dash = true;
    }


    IEnumerator Death(bool start)
    {
        if (!dying)
        {
            float t = Time.time + .73f;

            Vector3 d;
            if (start)
            {
                deathObj.localPosition = Vector3.zero;
                d = new Vector3(respawn.x - 60f, respawn.y);
            }
            else
            {
                dying = true;
                alive = false;
                deathObj.position = deathObj.position + Vector3.right * 150;
                d = transform.position;
            }

            while (Time.time < t)
            {
                rb.linearVelocity = Vector3.SmoothDamp(rb.linearVelocity, new Vector3(0, rb.linearVelocity.y), ref vel, .05f);
                deathObj.position = Vector3.SmoothDamp(deathObj.position, d, ref vel2, .27f);
                yield return null;
            }

            if (!start)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }
    
    
    #endregion

    //  //  // TESTE DE C�DIGOS //  //  //
    #region
    

    #endregion
}

//  //  // C�DIGO MORTO //  //  //
#region
//�1
/*/transform.Translate(new Vector2(joystick.horizontal*direcao * Time.deltaTime,0));
//Debug.Log(joystick.horizontal);
 if (Input.touchCount == 1)
 {
     Touch touch1 = Input.GetTouch(0);

     switch (touch1.phase)
     {
         case TouchPhase.Began:
             contagem++;

             break;
     }
 }
 else if (Input.touchCount == 2)
 {
     Touch touch1 = Input.GetTouch(1);

     switch (touch1.phase)
     {
         case TouchPhase.Began:
             contagem++;

             break;
     }
 }
 if (tempo > 0 && contagem == 1)
 {
     tempo -= Time.deltaTime;
 }
 else if (contagem == 2 && liberaChao == true)
 {
     rb.AddForce(new Vector2(0, 500));

     contagem = 0;
     tempo = 0.5f;
     liberaChao = false;
 }
 else if (contagem != 0)
 {
     contagem = 0;
     tempo = 0.5f;
 }
*/

//�2
/*Instantiate(tiroObj, transform.position, Quaternion.identity); 
 contagem = 0;
 tempo = 0.5f;
*/

//�3
/*private void OnCollisionEnter2D(Collision2D collision)
{
    if (collision.gameObject.CompareTag("chao"))
    {
        liberaChao = true;

    }
} //*/

//�4
/*public void dash1()
    {
        //int layerMask = 0 << 3;                                                                             // Layer de colis�o

        //player.transform.GetChild(1).transform.localPosition = new Vector2(dash, 0);                      // Alvo do dash
        //Debug.DrawRay(player.transform.position, player.transform.right, Color.cyan);
        //Debug.DrawLine(player.transform.position, player.transform.GetChild(1).transform.position, Color.red);
        RaycastHit2D hit;
        Vector2 dashposition;
        hit = Physics2D.Raycast(transform.position, new Vector2(joystick.horizontal,joystick.vertical),3);      // testar se tem colis�o

        dashposition = hit.point;
        Vector2 dashoposicao = Vector2.ClampMagnitude(-(dashposition),1);
        Debug.Log(dashposition);
        if(hit == true)
        {
            transform.position = (dashposition + dashoposicao);
        }
        else
        {
            transform.Translate(Vector3.ClampMagnitude(new Vector3(joystick.horizontal, joystick.vertical, 0), 3));
        }
        
    } //*/

//*1*
//hit = Physics2D.Raycast(transform.position, new Vector2(joystick.horizontal, joystick.vertical), distanciaDash + 1.3f);

//*2*
//hit = Physics2D.Raycast(transform.position, Vector2.right, distanciaDash + 1.3f);

//*3*
//hit = Physics2D.Raycast(transform.position, Vector2.left, distanciaDash + 1.3f);
#endregion
