using UnityEngine;

public class Laser : MonoBehaviour
{
    [SerializeField] private LaserSO _laserSO;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            //TODO - Damage the player
        }
    }
}
