using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITaggable<TSender>
{
    event Action OnTag;
    void Tag(TSender sender);
}
