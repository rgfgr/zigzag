using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piller : MonoBehaviour
{
    public float fallSpeed = 1.0f;

    public bool isDead = false;
    private bool isDeading = false;

    // Update is called once per frame
    void Update()
    {
        if (isDeading)
        {
            var pos = transform.position;
            pos.y -= fallSpeed * Time.deltaTime;
            transform.position = pos;
            if (pos.y <= -5)
            {
                isDeading = !(isDead = true);
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isDeading = true;
        }
    }
}
