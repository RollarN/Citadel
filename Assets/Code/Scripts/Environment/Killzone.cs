using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Killzone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.gameObject.TryGetComponent<Character>(out Character character))
            character.TakeDamage(2000);
    }
}
