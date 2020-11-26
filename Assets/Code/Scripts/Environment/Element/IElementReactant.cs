using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IElementReactant<T1>
{
    void TryAffect(T1 element, out bool affected);
}
