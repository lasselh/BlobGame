using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed;
    public float maxSpeed;


    private Rigidbody2D rb2d;

    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }
    void FixedUpdate()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector2 movement = new Vector2(moveHorizontal, moveVertical);
        
        rb2d.AddForce(movement * speed);

        if(rb2d.velocity.magnitude > maxSpeed)
        {
            rb2d.velocity = Vector3.ClampMagnitude(rb2d.velocity, maxSpeed);
        }
    }
}
