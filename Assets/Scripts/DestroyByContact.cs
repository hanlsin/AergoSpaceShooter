using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyByContact : MonoBehaviour
{
    public GameObject explosion;
    public GameObject playerExplosion;
    public int scoreValue;
    private GameController gameController;

    private void Start()
    {
        GameObject gameControllerObj = GameObject.FindWithTag("GameController");
        if (gameControllerObj != null)
        {
            gameController = gameControllerObj.GetComponent<GameController>();
        }
        if (gameController == null)
        {
            Debug.LogError("Cannot find 'GameContoller' scipr!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if ("Boundary" == other.tag)
        {
            return;
        }

        if ("Player" == other.tag)
        {
            Instantiate(playerExplosion, transform.position, transform.rotation);
            gameController.GameOver();
        }
        else
        {
            Instantiate(explosion, transform.position, transform.rotation);
            gameController.AddScore(scoreValue);
        }

        Destroy(other.gameObject);
        Destroy(gameObject);
    }
}
