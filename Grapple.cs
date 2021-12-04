using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grapple : MonoBehaviour
{
    public Camera cam;
    public Rigidbody2D rb;
    public LayerMask grappleableSurface;
    public Transform grapplePoint;
    public Vector2 lookDirection;
    public Material ropeMat;

    SpringJoint2D grappleRope;
    SpringJoint2D connectedRope;

    LineRenderer grappleLine;

    public float grappleDistance = Mathf.Infinity;

    public RaycastHit2D firstHit;
    public RaycastHit2D secondHit;

    public bool isGrappling = false;
    public bool isGrappledToDynamic = false;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        getLookDirection();
        startGrapple();
        updateLine();
    }

    void getLookDirection()
    {
        lookDirection = cam.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        Debug.DrawLine(grapplePoint.position, cam.ScreenToWorldPoint(Input.mousePosition));
    }

    void startGrapple()
    {
        //If the player clicks and they're not already grappling
        if (Input.GetMouseButtonDown(0) && isGrappling == false)
        {
            print("Attempting to Grapple!");

            //Raycast from the player's position to the mouse position
            firstHit = Physics2D.Raycast(transform.position, lookDirection, grappleDistance, grappleableSurface);

            //If trying to grapple to a rigid surface
            if (firstHit.collider != null && firstHit.collider.attachedRigidbody == null)
            {
                print("Grappling to rigid surface!");

                isGrappling = true;
                isGrappledToDynamic = false;

                setRope(firstHit);
            }
            //If trying to grapple to a dynamic surface
            else if (firstHit.collider != null && firstHit.collider.attachedRigidbody != null)
            {
                print("Grappling to dynamic surface!");

                isGrappling = true;
                isGrappledToDynamic = true;

                setRope(firstHit);
            }
        }
        //If the player left clicks and they are grappling
        else if (Input.GetMouseButtonDown(0) && isGrappling == true) 
        {
            removeRope();
            isGrappling = false;
            isGrappledToDynamic = false;
            rb.AddForce(new Vector2(5, 1), ForceMode2D.Impulse);
            this.GetComponent<PlayerController>().speed += 0.5f;
        }
    }

    void setRope(RaycastHit2D firstHit)
    {
        grappleRope = gameObject.AddComponent<SpringJoint2D>();
        grappleLine = gameObject.AddComponent<LineRenderer>();

        ropeSettings();
    }

    void updateLine()
    {
        if(grappleLine != null && isGrappling == true)
        {
            grappleLine.SetPosition(0, grapplePoint.position);

            if(firstHit.collider.attachedRigidbody == null)
            {
                grappleLine.SetPosition(1, firstHit.point);
            } 

            else if (firstHit.collider.attachedRigidbody != null)
            {
                grappleLine.SetPosition(1, firstHit.collider.attachedRigidbody.gameObject.transform.position);
            }
        }
    }

    void removeRope()
    {
        Destroy(grappleRope);
        Destroy(grappleLine);
    }

    void ropeSettings()
    {
        
        if(firstHit.collider.attachedRigidbody == null)
        {
            grappleRope.connectedAnchor = firstHit.point;
            grappleRope.connectedBody = null;
        } 
        else if(firstHit.collider.attachedRigidbody != null)
        {
            //grappleRope.connectedAnchor = firstHit.point;
            grappleRope.connectedBody = firstHit.collider.attachedRigidbody;
        }

        grappleRope.dampingRatio = 0.5f;
        grappleRope.frequency = 1.5f;
        grappleRope.autoConfigureDistance = false;
        grappleRope.distance = 0.25f;
        grappleRope.enabled = true;
        grappleRope.enableCollision = true;

        grappleLine.numCapVertices = 10;
        grappleLine.startWidth = 0.03f;
        grappleLine.endWidth = 0.01f;
        grappleLine.material = ropeMat;
        grappleLine.startColor = Color.black;
        grappleLine.endColor = Color.black;
    }
}
