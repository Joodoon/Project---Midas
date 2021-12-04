using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Respawn : MonoBehaviour
{
    [SerializeField] public LayerMask deathLayer;
    [SerializeField] public LayerMask checkpointLayer;
    [SerializeField] public CapsuleCollider2D cc;
    [SerializeField] public Rigidbody2D rb;
    [SerializeField] public Vector2 respawnLocation;
    [SerializeField] public Vector2 startLocation;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        checkpoint();
        respawn();
    }

    private void checkpoint()
    {
        if (cc.IsTouchingLayers(checkpointLayer))
        {
            respawnLocation = rb.transform.position;
        }
    }

   private void respawn()
    {
        if(cc.IsTouchingLayers(deathLayer))
        {
            rb.transform.position = respawnLocation;
            rb.velocity = new Vector2(0f, 0f);

            this.GetComponent<PlayerController>().horizontalDirection = 1f;

            this.GetComponent<Rail>().particle.Stop();

            print("i should be dead");
        }
    }
}
