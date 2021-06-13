using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using Panda;

//Faz derivar da Interface IAction
public class AI : MonoBehaviour, IAction
{
    //Posição do Player
    public Transform player;
    //Posição de onde as balas vão Spawnar
    public Transform bulletSpawn;
    //Slider da Barra de Vida dos Inimigos
    public Slider healthBar;
    //Prefab da bala
    public GameObject bulletPrefab, bulletExPrefab;


    //NavMeshAgent
    NavMeshAgent agent;
    //Destino aonde o inimigo irá se movimentar
    public Vector3 destination;
    //Posição do alvo
    public Vector3 target;
    //Quantia de vida
    float health = 100.0f;
    //Velocidade de rotação
    float rotSpeed = 5.0f;

    //Alcance em que pode ver o Player
    float visibleRange = 80.0f;
    //Alcance em que pode atirar no Player
    float shotRange = 0f;
    //Variavel para saber se é o BOT 4
    [SerializeField] private bool bot4;

    //Robô 2
    //Layer que o Bot que explode irá dar dano ao Exploder
    [SerializeField] private LayerMask playerLayer;
    //Robô 3
    //Transform da area vermelha que o BOT 3 faz aparecer
    [SerializeField] private Transform area;
    //Layers que o BOT 3 influencia
    [SerializeField] private LayerMask layersEffect;
    //Robô 4
    //Vezes que pode dar TP
    private int tpCount;
    //Pontos para dar o TP
    [SerializeField] private Transform[] fleePoints;

    void Start()
    {
        //BOT 4
        //Seta para poder dar 1 TP antes de ser morto completamente
        tpCount = 1;

        //Pega o componente do NavMeshAgent
        agent = this.GetComponent<NavMeshAgent>();
        //Inimigo para quando o Player está a uma distância do shotRange - 5 unidades
        agent.stoppingDistance = shotRange - 5;
        //Atualiza a barra de vida a cada 5 segundos
        InvokeRepeating("UpdateHealth", 5, 0.5f);
    }

    void Update()
    {
        //Posição da barra de Vida
        Vector3 healthBarPos = Camera.main.WorldToScreenPoint(this.transform.position);
        //Atribui o valor da vida na barra de Vida
        healthBar.value = (int)health;
        //Posiciona a barra de vida acima
        healthBar.transform.position = healthBarPos + new Vector3(0, 60, 0);

        //Se não for o bot 4 e tiver com mais de 100 de vida, seta para 100
        if (!bot4)
        {
            if (health > 100)
                health = 100;
        }
        else
        {
            return;
        }
    }

    void UpdateHealth()
    {
        //Se vida menor que 100, incrementa
        if (health < 100)
            health++;
    }

    [Task]
    public void PickRandomDestination()
    {
        //Pega uma posição aleatória ao redor do Player numa distância de 100 pixels
        Vector3 dest = new Vector3(Random.Range(-100, 100), 0, Random.Range(-100, 100));
        //Faz o inimigo ir até la
        agent.SetDestination(dest);
        //Termina a task e diz ao script do Panda
        Task.current.Succeed();
    }

    [Task]
    public void MoveToDestination()
    {
        if (Task.isInspected)
            Task.current.debugInfo = string.Format("t={0:0.00}", Time.time);

        if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
        {
            //Termina a task e diz ao script do Panda
            Task.current.Succeed();
        }
    }

    [Task]
    public void PickDestination(int x, int z)
    {
        //Seta o destino
        Vector3 dest = new Vector3(x, 0, z);
        //Move até o destino
        agent.SetDestination(dest);
        //Termina a task e diz ao script do Panda
        Task.current.Succeed();
    }

    [Task]
    public void TargetPlayer()
    {
        //Seta o player como o Alvo
        target = player.transform.position;
        //Termina a task e diz ao script do Panda
        Task.current.Succeed();
    }

    [Task]
    public void Fire()
    {
        //Se não tiver dado TP atira a bala normal
        if (tpCount > 0)
        {
            //Instancia o Prefab da bala como um GameObject na posição do bulletSpawn e com a rotação do mesmo
            GameObject bullet = GameObject.Instantiate(bulletPrefab,
                bulletSpawn.transform.position, bulletSpawn.transform.rotation);

            //Adiciona força na bala para ela ir pra frente
            bullet.GetComponent<Rigidbody>().AddForce(bullet.transform.forward * 3200);

            Task.current.Succeed();
        }
        //Atira a bala maior e que da mais dano
        else
        {
            GameObject bullet = GameObject.Instantiate(bulletExPrefab,
                                bulletSpawn.transform.position, bulletSpawn.transform.rotation);

            //Adiciona força na bala para ela ir pra frente
            bullet.GetComponent<Rigidbody>().AddForce(bullet.transform.forward * 3200);

            Task.current.Succeed();
        }


    }

    [Task]
    public void LookAtTarget()
    {
        //Cria um vetor direção do ponto A (posição do inimigo) e do Alvo (Player)
        Vector3 direction = target - this.transform.position;

        //Rotaciona o inimigo para olhar para o Alvo
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation,
            Quaternion.LookRotation(direction), Time.deltaTime * rotSpeed);

        //Se o angulo for menor do que 0.5 (o que significa que está olhando para o alvo)
        if (Vector3.Angle(this.transform.forward, direction) < 0.4f)
        {
            //Termina a task e diz ao script do Panda
            Task.current.Succeed();
        }
        //Se não tiver no angulo ainda, refaz a função (foi pra resolver alguns problemas no BOT 1)
        else
            LookAtTarget();
    }

    [Task]
    bool SeePlayer()
    {
        //Distancia do inimigo para o Player
        Vector3 distance = player.transform.position - this.transform.position;
        //Raycast
        RaycastHit hit;
        //Bool para saber se está olhando para uma parede
        bool seeWall = false;
        //Desenha o raio do Raycast na cena
        Debug.DrawRay(this.transform.position, distance, Color.red);
        //Se o raio colidir com uma parede, seta o seeWall como TRUE
        if (Physics.Raycast(this.transform.position, distance, out hit))
        {
            if (hit.collider.gameObject.tag == "wall")
            {
                seeWall = true;
            }
        }

        if (Task.isInspected)
        {
            Task.current.debugInfo = string.Format("wall={0}", seeWall);
        }

        if (distance.magnitude < visibleRange && !seeWall)
            return true;
        else
            return false;
    }

    [Task]
    bool Turn(float angle)
    {
        //Não entendi muito bem pra falar a verdade
        var p = this.transform.position + Quaternion.AngleAxis(angle, Vector3.up) * this.transform.forward;
        target = p;
        return true;
    }

    [Task]
    public void SelfDestruct()
    {
        //Faz um Raycast em formato de Esfera e checa por todos os objetos com a Layer 'Player'
        Collider[] player = Physics.OverlapSphere(transform.position, 30f, playerLayer);

        //Para cada um da Layer 'Player'
        foreach (Collider target in player)
        {
            //Pega o Script do Drive e chama a função de dar Dano
            var drive = target.GetComponent<Drive>();
            drive.TakeDamage();
        }
        //Destroy o BOT
        Destroy(gameObject);
    }

    [Task]
    public void DamageOrHeal()
    {
        //Faz a mesma coisa que o Self Destruct, só que também checa pela Layer 'Enemies'
        Collider[] robots = Physics.OverlapSphere(transform.position, 50f, layersEffect);

        //Faz aparecer uma área para demonstrar visualmente
        StartCoroutine(ShowArea());
        foreach (Collider targets in robots)
        {
            //Pega o elemento da Interface e aplica a função de acordo com o elemento
            var action = targets.GetComponent<IAction>();
            action.ReceiveActionOnTrigger(30f);
        }
        Task.current.Succeed();
    }

    [Task]
    public void Die()
    {
        //Destroy o BOT e a barra de Vida
        Destroy(healthBar.gameObject);
        Destroy(gameObject);
    }

    [Task]
    public bool CheckDistance(float distance)
    {
        //Se a distância entre esse objeto e o player for menor que distância retorna true
        if (Vector3.Distance(transform.position, player.position) < distance)
            return true;
        //Se não retorna false
        else
            return false;
    }

    [Task]
    public void Chase()
    {
        //Seta o destino para a posição do Player para perseguir
        agent.SetDestination(player.position);
    }

    [Task]
    public void FuryMode()
    {
        //Aumenta o tamanho do BOT
        transform.localScale = new Vector3(2.2f, 2.2f, 2.2f);
        //Seta a stopping distance pra 20
        agent.stoppingDistance = 20f;
    }

    [Task]
    public bool CanTeleport()
    {
        //Se TP count maior que 0 retorna true
        if (tpCount > 0)
            return true;
        //Se não retorna false
        else
            return false;
    }

    [Task]
    public bool CheckHealth(float lessThan)
    {
        //Se a vida for menor que o lessThan retorna true
        if (health <= lessThan)
            return true;
        //Se não retorna false
        else
            return false;
    }

    [Task]
    public void GoToSafeArea()
    {
        //Vai para uma das areas do FleePoints e diminui o tpCount para indicar que realizou o TP
        if (tpCount > 0)
        {
            //Aumenta a vida para 200
            health = 200;
            //Atualiza a HealthBar para 200
            healthBar.maxValue = 200;
            //Teleporta
            transform.position = fleePoints[Random.Range(0, fleePoints.Length)].position;
            tpCount--;
            Task.current.Succeed();
        }
    }

    void OnCollisionEnter(Collision col)
    {
        //Se for atingido por uma bala do Player perde 10 de vida
        if (col.gameObject.tag == "bullet")
        {
            health -= 10;
        }
    }

    public void ReceiveActionOnTrigger(float value)
    {
        //Script derivado da Interface para os BOTS (recupera a vida se estiver na área)
        health += value;
    }

    //Faz aparecer a Área vermelha e sumir em 0.1 segundos
    private IEnumerator ShowArea()
    {
        area.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        area.gameObject.SetActive(false);
    }

    [Task]
    public void Fugir()
    {
        //Calcula a distância entre o bot e o player
        float distance = Vector3.Distance(transform.position, player.transform.position);

        //Se for menor ou igual a 30
        if (distance >= 30f)
        {
            //Vetor distância do BOT para o Player
            Vector3 playerDir = player.transform.position - transform.position;

            //Faz o vetor ficar o oposto do que era, fazendo ser a direção oposta da que o player está
            Vector3 fleeDir = transform.position - playerDir;

            //Seta o destino
            agent.SetDestination(fleeDir);
            //Aumenta a velocidade
            agent.speed = 33f;
        }

        //Se a distância for maior que 30 diz que a Task foi concluida para não ficar travado nela
        if (distance > 30f)
            Task.current.Succeed();
    }
}

