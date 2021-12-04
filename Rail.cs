using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pixelplacement;

public class Rail : MonoBehaviour
{
    [SerializeField] public Rigidbody2D rb;
    [SerializeField] public BoxCollider2D bc;
    [SerializeField] public LayerMask railLayer;
    [SerializeField] public Vector2 perpendicular;
    [SerializeField] public ParticleSystem particle;

    [SerializeField] public bool canGrind = true;
    [SerializeField] public bool isGrinding = false;


    // Start is called before the first frame update
    void Start()
    {
        particle.Stop();
    }

    // Update is called once per frame
    void Update()
    {
        if(!isGrinding)
        {
            particle.Stop();
        }

        RaycastHit2D hit = Physics2D.Raycast(bc.transform.position, Vector2.down, 1f, railLayer);

        StartCoroutine(movePlayer(hit));
        StartCoroutine(stopPlayer(hit));
        railGrind(hit);
        stopGrind(hit);
    } 

    public void railGrind(RaycastHit2D hit)
    {

        if (bc.IsTouchingLayers(railLayer) && canGrind && rb.velocity.y < 0f && hit) {

            canGrind = false;
            isGrinding = true;
        } 
        else if (canGrind)
        {
            isGrinding = false;
            canGrind = true;
        }
    }

    IEnumerator movePlayer(RaycastHit2D hit)
    {
        while(isGrinding)
        {
            canGrind = false;
            hit.collider.isTrigger = false;
            this.GetComponent<PlayerController>().canMove = false;
            particle.Play();
            rb.gravityScale = 0f;
            rb.velocity = new Vector2(hit.normal.y, hit.normal.x) * new Vector2(2f, 2f);
            //yield return new WaitForSeconds(0.1f);
            yield return null;
        }

        yield return null;
    }

    IEnumerator stopPlayer(RaycastHit2D hit)
    {
        while (!isGrinding)
        {
            canGrind = true;
            hit.collider.isTrigger = true;
            this.GetComponent<PlayerController>().canMove = true;
            particle.Stop();
            if(this.GetComponent<PlayerController>().isWallRunning == false)
            {
                rb.gravityScale = 1f; 
            }
            yield return null;
        }
        yield return null;
    }

    IEnumerator waitForGrind()
    {
        particle.Stop();
        canGrind = true;

        yield return null;
    }

    public void stopGrind(RaycastHit2D hit)
    {
        if (isGrinding && Input.GetKey("s"))
        {
            isGrinding = false;
            hit.collider.isTrigger = true;
            particle.Stop();

            this.GetComponent<PlayerController>().speed = this.GetComponent<PlayerController>().speed + 1f;
            rb.AddForce(new Vector2(0f, -this.GetComponent<PlayerController>().jumpHeight/50f), ForceMode2D.Impulse);
            canGrind = true;
        }
        if (isGrinding && Input.GetKeyDown("w"))
        {
            isGrinding = false;
            hit.collider.isTrigger = true;
            particle.Stop();

            this.GetComponent<PlayerController>().speed = this.GetComponent<PlayerController>().speed + 1f;
            rb.AddForce(new Vector2(0f, this.GetComponent<PlayerController>().jumpHeight), ForceMode2D.Impulse);
            canGrind = true;
        }
        if (!isGrinding || !bc.IsTouchingLayers(railLayer)) 
        {
            isGrinding = false;

            hit.collider.isTrigger = true;
            particle.Stop();
        }

    }
}
