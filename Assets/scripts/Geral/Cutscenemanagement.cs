using Analog;
using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Cutscenemanagement : MonoBehaviour
{
    //  //  // DECLARA��ES //  //  //
    CinemachineVirtualCamera VCam;
    CinemachineFramingTransposer transp;
    GameObject d_box;                 // dialogue box
    TMP_Text d_text;               // dialogue text
    [SerializeField]GameObject[] hud;
    //[SerializeField]joystick joy;                   //
    Vector2 t_default;              // Transpose default
    Vector2 t_current;              // Transpose current
    public bool locked = false;     // Camera locked
    public bool running = false;    // cutscene
    [SerializeField] float time = .1f;  // Dialogue time
    bool touched = false;       // Skip?
    bool run = false;       // dialogue
    int joy_stage = 0;
    [SerializeField] GameObject end;

    Transform deathObj;
    static Vector3 vel = Vector3.zero;

    //
    private movimento movimento;
    private Pause pause;
    public bool movement_locked = false;

    private void Awake()
    {
        pause = GameObject.FindGameObjectWithTag("Pause Menu").GetComponent<Pause>();
    }

    void Start()
    {
        deathObj = GameObject.FindGameObjectWithTag("deathHud").transform;
        VCam = GameObject.FindGameObjectWithTag("VCam").GetComponent<CinemachineVirtualCamera>();
        transp = VCam.GetCinemachineComponent<CinemachineFramingTransposer>();
        t_default = new Vector2(transp.m_ScreenX, transp.m_ScreenY);
        t_current = new Vector2(t_default.x, t_default.y);
        d_box = GameObject.FindGameObjectWithTag("Dialogue");
        d_text = d_box.transform.GetChild(0).GetComponent<TMP_Text>();
        d_box.SetActive(false);
        hud = GameObject.FindGameObjectsWithTag("Hud controls");
        end.SetActive(false);

        movimento = GetComponent<movimento>();
        //pause = GameObject.FindGameObjectWithTag("Pause Menu").GetComponent<Pause>();

        for (int i = 0; i < hud.Length; i++)
        {
            if (hud[i].name == "joystick")
            {
                //joy = hud[i].transform.GetChild(1).GetComponent<joystick>();
            }
        }
    }


    void Update()
    {
        transp.m_ScreenX = t_current.x;
        transp.m_ScreenY = t_current.y;
        /*
        if(Input.touchCount > 0 && Input.GetTouch(0).position.y < Screen.height / 2 &&  PlayerPrefs.GetInt("pause") == 0)
        {
            Touch t = Input.GetTouch(0);

            switch (t.phase)
            {
                case TouchPhase.Began:
                    touched = true;
                    break;
                case TouchPhase.Ended:
                    //touched = false;
                    break;
            }
        }*/

        
        if(movimento.t_camera != joy_stage)
        {
            if (!locked)
            {
                Look(movimento.t_camera);
            }
            joy_stage = movimento.t_camera;
        }        
    }


    public void GetCutscene(string s, int l = 2)
    {
        if (l == 1)
        {
            locked = true;
        }
        else if(l == 0)
        {
            locked = false;
        }


        switch (s)
        {
            case "LookDown":
                Look(1);
                break;

            case "LookDownL":
                locked = true;
                Look(1);
                break;

            case "LookDownJoy":
                if (!locked)
                    Look(1);
                break;

            case "LookUp":
                Look(2);
                break;

            case "LookUpL":
                locked = true;
                Look(2);
                break;

            case "LookUpJoy":
                if(!locked)
                    Look(2);
                break;

            case "LookDefault":
                locked = false;
                Look();
                break;

            case "LookDefaultJoy":
                if (!locked)
                    Look();
                break;

            case "c1":
                StartCoroutine(Scene(1));
                break;

            case "c2":
                StartCoroutine(Scene(2));
                break;

            case "c3":
                StartCoroutine(Scene(3));
                break;

            case "c3.5":
                StartCoroutine(Scene(35));
                break;

            case "c4":
                StartCoroutine(Scene(4));
                break;

            case "c5":
                StartCoroutine(Scene(5));
                break;
        }
    }

    void Look(int i = 0, GameObject target = null)
    {
        switch (i)
        {
            case 0:
                t_current.y = t_default.y;
                break;
            case 1:
                t_current.y = .1f;
                break;
            case 2:
                t_current.y = .9f;
                break;
        }

        if(target != null)
        {
            VCam.Follow = target.transform;
        }
        else
        {
            VCam.Follow = gameObject.transform;
        }
    }

    void HudDisable(bool set)
    {
        for (int i = 0; i < hud.Length; i++)
        {
            if (set == false)
            {
                //Debug.Log("0");
                //joy.Stop(true);
            }
            else
            {
                //Debug.Log("2.5");
                //joy.Stop(false);
            }

            if (hud[i].name != "joystick")
            hud[i].SetActive(set);
        }
    }


    IEnumerator Scene(int scene)
    {
        running = true;
        touched = false;
        locked = true;
        movement_locked = true;
        d_box.SetActive(true);
        HudDisable(false);
        switch (scene)
        {
            case 1:
                if (!PlayerPrefs.HasKey("c1"))
                {
                    Look(0,GameObject.FindGameObjectWithTag("Anhaga"));
                    StartCoroutine(RunDialogue("Ah, Tainara! Finalmente chegou. Pronta para começar? Quero ver o quanto você evoluiu esses dias."));

                    yield return new WaitUntil(() => !run); yield return new WaitUntil(() => touched); touched = false;

                    Look();
                    StartCoroutine(RunDialogue("Olha, eu já pedi desculpas por aquele acidente com o jacaré. Não precisava ter trocado o lugar de treino só por isso. Eu consigo dar conta."));

                    yield return new WaitUntil(() => !run); yield return new WaitUntil(() => touched); touched = false;

                    Look(0, GameObject.FindGameObjectWithTag("Anhaga"));
                    StartCoroutine(RunDialogue("Você tem que prestar mais atenção da próxima vez para não confundir eles com um tronco de novo. Deixe desse medo bobo."));

                    yield return new WaitUntil(() => !run); yield return new WaitUntil(() => touched); touched = false;

                    Look();
                    StartCoroutine(RunDialogue("Mas você fala isso por que não viu os dentes dele! Parece um lagarto gigante. Urgh!!"));

                    yield return new WaitUntil(() => !run); yield return new WaitUntil(() => touched); touched = false;

                    StartCoroutine(RunDialogue("E ainda acho que eu consigo dessa vez."));

                    yield return new WaitUntil(() => !run); yield return new WaitUntil(() => touched); touched = false;

                    Look(0, GameObject.FindGameObjectWithTag("Anhaga"));
                    StartCoroutine(RunDialogue("Relaxe criança. Você precisa praticar em outro ambiente também. E tenho meus motivos para estar aqui."));

                    yield return new WaitUntil(() => !run); yield return new WaitUntil(() => touched); touched = false;

                    Look();
                    StartCoroutine(RunDialogue("Que motivos? É melhor que não tenham a ver com jacarés e troncos."));

                    yield return new WaitUntil(() => !run); yield return new WaitUntil(() => touched); touched = false;

                    Look(0, GameObject.FindGameObjectWithTag("Anhaga"));
                    StartCoroutine(RunDialogue("Se você conseguir chegar no final podemos conversar."));

                    yield return new WaitUntil(() => !run); yield return new WaitUntil(() => touched); touched = false;

                    Look();
                    StartCoroutine(RunDialogue("Ha! Não se preocupe. Eu te espero lá."));

                    yield return new WaitUntil(() => !run); yield return new WaitUntil(() => touched); touched = false;

                    PlayerPrefs.SetString("c1", "c1");
                }
                break;
            case 2:
                if(!PlayerPrefs.HasKey("c2"))
                {
                    Look();
                    StartCoroutine(RunDialogue("Tainara! Aqui em cima!"));
                    
                    yield return new WaitUntil(() => !run); yield return new WaitUntil(() => touched); touched = false;

                    Look(1, GameObject.FindGameObjectWithTag("Anhaga"));
                    StartCoroutine(RunDialogue("Está indo muito bem. Antes de continuar quero te avisar algo."));

                    yield return new WaitUntil(() => !run); yield return new WaitUntil(() => touched); touched = false;
                    Look();
                    StartCoroutine(RunDialogue("Tudo bem... Mas como você já está aqui? Você ainda tem que me explicar como você chega assim tão rápido nos lugares."));

                    yield return new WaitUntil(() => !run); yield return new WaitUntil(() => touched); touched = false;

                    Look(1, GameObject.FindGameObjectWithTag("Anhaga"));
                    StartCoroutine(RunDialogue("Bem... Digamos que eu não tenho animação para ficar andando, então venho de outra forma."));

                    yield return new WaitUntil(() => !run); yield return new WaitUntil(() => touched); touched = false;

                    Look();
                    StartCoroutine(RunDialogue("Mas o que isso quer dizer?"));

                    yield return new WaitUntil(() => !run); yield return new WaitUntil(() => touched); touched = false;

                    Look(1, GameObject.FindGameObjectWithTag("Anhaga"));
                    StartCoroutine(RunDialogue("Não importa. O que eu queria dizer é que fiz algo para você. Descendo aqui embaixo terão umas flores. Elas vão salvar seu progresso e você vai voltar nelas se cair."));

                    yield return new WaitUntil(() => !run); yield return new WaitUntil(() => touched); touched = false;

                    Look(1);
                    StartCoroutine(RunDialogue("(Se mova para baixo e pule para descer de plataformas.)"));

                    yield return new WaitUntil(() => !run); yield return new WaitUntil(() => touched); touched = false;

                    PlayerPrefs.SetString("c2", "c2");
                }
                break;

            case 3:
                if (!PlayerPrefs.HasKey("c3"))
                {
                    Look(0, GameObject.FindGameObjectWithTag("Anhaga"));
                    StartCoroutine(RunDialogue("O caminho foi bloqueado por estes espinhos. Não vai ter como passar por aqui."));

                    yield return new WaitUntil(() => !run); yield return new WaitUntil(() => touched); touched = false;

                    Look();
                    StartCoroutine(RunDialogue("Estes espinhos não estava sempre aí?"));

                    yield return new WaitUntil(() => !run); yield return new WaitUntil(() => touched); touched = false;

                    Look(0, GameObject.FindGameObjectWithTag("Anhaga"));
                    StartCoroutine(RunDialogue("Não. Já usei este caminho antes. Algo fez estas vinhas crescerem mais do que deveriam."));

                    yield return new WaitUntil(() => !run); yield return new WaitUntil(() => touched); touched = false;

                    Look();
                    StartCoroutine(RunDialogue("Eu... Devia me preocupar? Seu tom de voz me diz que voc� sabe de algo."));

                    yield return new WaitUntil(() => !run); yield return new WaitUntil(() => touched); touched = false;

                    Look(0, GameObject.FindGameObjectWithTag("Anhaga"));
                    StartCoroutine(RunDialogue("Você tem que se concentrar em terminar o seu treino. Use essas paredes e pule de uma para outra para subir como eu te ensinei. Te encontro lá em cima"));

                    yield return new WaitUntil(() => !run); yield return new WaitUntil(() => touched); touched = false;

                    Look();
                    StartCoroutine(RunDialogue("Mas..."));

                    yield return new WaitUntil(() => !run); yield return new WaitUntil(() => touched); touched = false;

                    StartCoroutine(RunDialogue("Certo. Tudo bem."));

                    yield return new WaitUntil(() => !run); yield return new WaitUntil(() => touched); touched = false;

                    Look(2);
                    StartCoroutine(RunDialogue("(Salte na parede e pule mais uma vez antes de cair no chão para se propulsionar para alturas maiores. Você pode repetir isso até chegar no topo!)"));

                    yield return new WaitUntil(() => !run); yield return new WaitUntil(() => touched); touched = false;

                    PlayerPrefs.SetString("c3", "c3");
                }
                break;

            case 35:
                if (!PlayerPrefs.HasKey("c3.5"))
                {
                    Look(0, GameObject.FindGameObjectWithTag("Anhaga"));
                    StartCoroutine(RunDialogue("Demorou mais do que eu imaginava. Ainda está viva?"));

                    yield return new WaitUntil(() => !run); yield return new WaitUntil(() => touched); touched = false;

                    Look();
                    StartCoroutine(RunDialogue("Haha. Não foi tão difícil. Eu só estava... Er... Me aquecendo."));

                    yield return new WaitUntil(() => !run); yield return new WaitUntil(() => touched); touched = false;

                    Look(0, GameObject.FindGameObjectWithTag("Anhaga"));
                    StartCoroutine(RunDialogue("Bem, já que está tão pronta, vamos para um desafio de verdade com obstáculos vivos. O que acha?"));

                    yield return new WaitUntil(() => !run); yield return new WaitUntil(() => touched); touched = false;

                    Look();
                    StartCoroutine(RunDialogue("Obstáculos vivos? Você quer dizer que eu vou ter que lutar contra animais?"));

                    yield return new WaitUntil(() => !run); yield return new WaitUntil(() => touched); touched = false;

                    Look(0, GameObject.FindGameObjectWithTag("Anhaga"));
                    StartCoroutine(RunDialogue("Sim. O caminho a frente est� cheio deles. Mas lembre-se que eles são vidas como você. Segure os seus golpes e apenas nocauteie eles."));

                    yield return new WaitUntil(() => !run); yield return new WaitUntil(() => touched); touched = false;

                    Look();
                    StartCoroutine(RunDialogue("Pode deixar! Eu odiaria ferir um animal de verdade. "));

                    yield return new WaitUntil(() => !run); yield return new WaitUntil(() => touched); touched = false;

                    Look();
                    StartCoroutine(RunDialogue("(Use o botão de ataque para nocautear inimigos.)"));

                    yield return new WaitUntil(() => !run); yield return new WaitUntil(() => touched); touched = false;

                    PlayerPrefs.SetString("c3.5", "c3.5");
                }
                break;

            case 4:
                if (!PlayerPrefs.HasKey("c4"))
                {
                    //Look(0, GameObject.FindGameObjectWithTag("Anhaga"));
                    StartCoroutine(RunDialogue("Tainara!! Venha rápido! Eu não tenho muito tempo!"));

                    yield return new WaitUntil(() => !run); yield return new WaitUntil(() => touched); touched = false;

                    Look();
                    StartCoroutine(RunDialogue("Anhagá?! O que houve?!"));

                    yield return new WaitUntil(() => !run); yield return new WaitUntil(() => touched); touched = false;

                    Look(0, GameObject.FindGameObjectWithTag("Anhaga"));
                    StartCoroutine(RunDialogue("Tem alguma coisa de errada com a floresta. Eu estou sentindo minha ligação com este plano se quebrando."));

                    yield return new WaitUntil(() => !run); yield return new WaitUntil(() => touched); touched = false;

                    Look();
                    StartCoroutine(RunDialogue("Mas o quê? Por quê? O que foi que aconteceu?! Você não pode sumir assim!! Tem que haver uma forma de evitar isso!!!"));

                    yield return new WaitUntil(() => !run); yield return new WaitUntil(() => touched); touched = false;

                    Look(0, GameObject.FindGameObjectWithTag("Anhaga"));
                    StartCoroutine(RunDialogue("Sinto muito, mas não tem nada o que fazer. Apenas volte para a cabana!"));

                    yield return new WaitUntil(() => !run); yield return new WaitUntil(() => touched); touched = false;

                    Look();
                    StartCoroutine(RunDialogue("O quê?! Claro que não!! Eu irei descobrir o que aconteceu!! Não vou deixar voc� sumir assim."));

                    yield return new WaitUntil(() => !run); yield return new WaitUntil(() => touched); touched = false;

                    Look(0, GameObject.FindGameObjectWithTag("Anhaga"));
                    StartCoroutine(RunDialogue("Não!! É muito perigoso. Os animais dessa região foram corrompidos. Eles se transformaram em bestas agressivas. Não é seguro. Volte agora!"));

                    yield return new WaitUntil(() => !run); yield return new WaitUntil(() => touched); touched = false;

                    Look();
                    StartCoroutine(RunDialogue("Eu..."));

                    yield return new WaitUntil(() => !run); yield return new WaitUntil(() => touched); touched = false;

                    StartCoroutine(RunDialogue("..."));

                    yield return new WaitUntil(() => !run); yield return new WaitUntil(() => touched); touched = false;

                    StartCoroutine(RunDialogue("Eu vou descobrir o que está acontecendo. Não posso deixar você sumir. E esse também é minha floresta."));

                    yield return new WaitUntil(() => !run); yield return new WaitUntil(() => touched); touched = false;

                    Look(0, GameObject.FindGameObjectWithTag("Anhaga"));
                    StartCoroutine(RunDialogue("Tainara..."));

                    yield return new WaitUntil(() => !run);
                    PlayerPrefs.SetString("c4Aux", "c4Aux");

                    yield return new WaitUntil(() => touched); touched = false;

                    StartCoroutine(RunDialogue("..."));

                    yield return new WaitUntil(() => !run); yield return new WaitUntil(() => touched); touched = false;

                    StartCoroutine(RunDialogue("Eu vou descobrir..."));

                    yield return new WaitUntil(() => !run); yield return new WaitUntil(() => touched); touched = false;

                    PlayerPrefs.SetString("c4", "c4");
                }
                break;
            case 5:
                if (!PlayerPrefs.HasKey("c5"))
                {
                    StartCoroutine(RunDialogue("Parece que as criaturas corrompidas vieram daqui. Se eu seguir por essa trilha talvez eu ache alguma resposta...?"));

                    yield return new WaitUntil(() => !run); yield return new WaitUntil(() => touched); touched = false;

                    StartCoroutine(RunDialogue("Bem só tem uma forma de descobrir."));

                    yield return new WaitUntil(() => !run); yield return new WaitUntil(() => touched); touched = false;

                    PlayerPrefs.SetString("end", "end");

                    PlayerPrefs.SetString("c5", "c5");
                }
                break;
        }
        Look(0, null);
        d_box.SetActive(false);
        if (!PlayerPrefs.HasKey("end"))
        {
            HudDisable(true);
            locked = false;
            running = false;
            movement_locked = false;
        }
        else
        {
            StartCoroutine(transicao());
        }
    }

    IEnumerator RunDialogue(string text, float speed = 0)
    {
        run = true;
        d_text.text = "";
        float t;
        if(speed == 0)
        {
            t = time;
        }
        else
        {
            t = speed;
        }

        for (int i = 0; i < text.Length; i++)
        {
            d_text.text += text[i];
            yield return new WaitForSeconds(t);
            if(touched)
            {
                d_text.text = text;
                touched = false;
                i = text.Length;
            }
        }
        run = false;
    }

    IEnumerator transicao()
    {
        Time.timeScale = .8f;

        float t = Time.time + .73f;

        deathObj.position = deathObj.position + Vector3.right * 150;

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        while (Time.time < t)
        {
            deathObj.position = Vector3.SmoothDamp(deathObj.position, player.transform.position, ref vel, .27f);
            yield return null;
        }

        end.SetActive(true);
        deathObj.gameObject.SetActive(false);
    }

    public void Quit()
    {
        PlayerPrefs.Save();
        SceneManager.LoadScene(0);
    }

    public void OnInteract(InputAction.CallbackContext _callbackContext)
    {
        if(_callbackContext.started && PlayerPrefs.GetInt("pause") == 0)
            touched = true;
    }

    public void OnPause(InputAction.CallbackContext _callbackContext)
    {
        if (_callbackContext.started)
        {
            pause.TogglePause();
        }
    }
}
