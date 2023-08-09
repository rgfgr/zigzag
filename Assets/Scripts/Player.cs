using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float moveSpeed = 10;

    private bool left = true;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
            left = !left;
        transform.position += moveSpeed * Time.deltaTime * (left ? transform.forward : transform.right);

        if (transform.position.y <= -3)
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            rb.velocity = Vector3.zero;
            rb.useGravity = false;
            GameObject.Find("GameMaster").GetComponent<GameMaster>().EndGame();
            GetComponent<Renderer>().enabled = false;
            GetComponent<Player>().enabled = false;
        }
    }
}
