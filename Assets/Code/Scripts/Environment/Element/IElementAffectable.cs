using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IElementAffectable {

    void SetElement(ElementState targetElement);
    void UpdateElement(ElementState element);
}
