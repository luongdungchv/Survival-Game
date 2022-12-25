using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(InputReceiver), typeof(Rigidbody))]
public class NetworkMovement : MonoBehaviour
{
    [SerializeField] private float speed;
    private InputReceiver inputReceiver;
    private Client client => Client.ins;
    private Rigidbody rb;
    private Vector2 movementInputVector => inputReceiver.movementInputVector;
    private void Awake()
    {
        inputReceiver = GetComponent<InputReceiver>();
        rb = GetComponent<Rigidbody>();
    }
    private void Update()
    {
        Move();
    }
    public void Move()
    {
        if (movementInputVector != Vector2.zero)
        {
            Debug.Log(movementInputVector);
            rb.velocity = movementInputVector.normalized * speed;
        }
        else
        {
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
        }
    }

}
