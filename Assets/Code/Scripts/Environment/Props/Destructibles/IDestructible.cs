using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDestructible
{
    void Explode(float explosionForce, Vector3 explosionPosition, float explosionRadius, float upwardsModifier = 0f);
}
