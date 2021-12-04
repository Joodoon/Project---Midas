using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    //Reference to the Camera and the Player
    public Camera cam;
    public Transform subject;

    //Starting Position of the background sprite we wish to move
    Vector2 startPosition;
    float startZ;

    //Find the distance travelled by the camera with respect to the position of the background sprite, and store the value in a variable
    Vector2 travel => (Vector2)cam.transform.position - startPosition;
                             
    //Find the z distance from the background sprite to the player, and store the value in a variable
    float distanceFromSubject => transform.position.z - subject.position.z;

    //If the sprite is infront of the player, the value should be negative. If it is behind the player, the value should be positive
    float clippingPlane => (cam.transform.position.z + (distanceFromSubject > 0 ? cam.farClipPlane : cam.nearClipPlane));

    //Find the factor at which the background sprite should move based off of its distance from the player (can be negative or positive, depending on the clippingPlane variable above)
    float parallaxFactor => Mathf.Abs(distanceFromSubject) / clippingPlane;

    //When this script first runs, set the starting position of the background sprite to its current position
    public void Start()
    {
        startPosition = transform.position;
        startZ = transform.position.z;
    }

    //Every frame, set the position of the background sprite to newPos; a new position determined by the parallax factor and distance travelled by the camera
    public void Update()
    {
        Vector2 newPos = transform.position = startPosition + travel * parallaxFactor;
        transform.position = new Vector3(newPos.x, newPos.y, startZ);
    }

}
