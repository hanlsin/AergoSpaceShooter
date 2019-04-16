using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Boundary
{
    public float xMin, xMax, zMin, zMax;
}

public class PlayerController : MonoBehaviour
{
    public float speed;
    public float tilt;
    public Boundary boundary;

    public GameObject shot;
    public Transform shotSpawn;

    public float fireRate = 0.5f;
    private float nextFire = 0.0f;

    private void FixedUpdate()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
        Rigidbody rigidBody = GetComponent<Rigidbody>();
        rigidBody.velocity = movement * speed;

        var x = Mathf.Clamp(rigidBody.position.x, boundary.xMin, boundary.xMax);
        var z = Mathf.Clamp(rigidBody.position.z, boundary.zMin, boundary.zMax);
        rigidBody.position = new Vector3(x, 0.0f, z);

        rigidBody.rotation = Quaternion.Euler(0.0f, 0.0f, rigidBody.velocity.x * tilt);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && Time.time > nextFire)
        {
            nextFire = Time.time + fireRate;
            Instantiate(shot, shotSpawn.position, shotSpawn.rotation);
            GetComponent<AudioSource>().Play();
        }
    }
}
