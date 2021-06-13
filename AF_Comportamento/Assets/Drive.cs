using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Faz derivar da Interface IAction
public class Drive : MonoBehaviour, IAction
{
    //Velocidade do Player
	public float speed = 30f;
    //Velocidade de Rotação
    float rotationSpeed = 120.0f;
    //Prefab da Bala
    public GameObject bulletPrefab;
    //Local de Spawn da Bala
    public Transform bulletSpawn;
    //Quantia de Vida
    [SerializeField] private float health;
    //Slider da Barra de Vida
    [SerializeField] private Slider healthBar;
    //Ponto de Respawn  ao Morrer
    [SerializeField] private Transform respawnPoint;
    void Update() {
        //Pega o Input Vertical (w e S)
        float translation = Input.GetAxis("Vertical") * speed;
        //Pega o Input Horizontal (A e D)
        float rotation = Input.GetAxis("Horizontal") * rotationSpeed;
        //Multiplica por DeltaTime para ser Constante
        translation *= Time.deltaTime;
        rotation *= Time.deltaTime;
        //Move para Frente
        transform.Translate(0, 0, translation);
        //Rotaciona
        transform.Rotate(0, rotation, 0);

        //Input para atirar Balas
        if(Input.GetKeyDown("space"))
        {
            //Instancia a bala como GameObject
            GameObject bullet = GameObject.Instantiate(bulletPrefab, bulletSpawn.transform.position, bulletSpawn.transform.rotation);
            //Aplica força para frente
            bullet.GetComponent<Rigidbody>().AddForce(bullet.transform.forward*3200);
        }

        //Posição da barra de Vida
        Vector3 healthBarPos = Camera.main.WorldToScreenPoint(this.transform.position);
        //Atribui o valor da vida na barra de Vida
        healthBar.value = (int)health;
        //Posiciona a barra de vida acima
        healthBar.transform.position = healthBarPos + new Vector3(0, 60, 0);
    }

    public void TakeDamage()
    {
        //Leva 30 de Dano
        health -= 30f;
    }

    void OnCollisionEnter(Collision col)
    {
        //Se for atingido por uma bala do Player perde 10 de vida
        if (col.gameObject.tag == "bullet")
        {
            health -= 10;
        }
        //Se for atingido pela bala maior do robô grande perde 30 de vida
        if (col.collider.CompareTag("bulletEx"))
        {
            health -= 30;
        }
    }

    //Função da Interface IAction pra realizar a mesma ação em Objetos diferentes
    public void ReceiveActionOnTrigger(float value)
    {
        health -= value;
    }

    private void Respawn()
    {
        //Se tiver 0 ou menos de vida
        if (health <= 0)
        {
            //Respawna no ponto especifico
            transform.position = respawnPoint.position;
            //Volta a vida para 100
            health = 100f;
        }
    }

}
