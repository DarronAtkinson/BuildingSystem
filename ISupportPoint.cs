using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Structures
{
    public interface ISupportPoint
    {
        Vector3 position { get; }
        bool required { get; }
        float requiredSupport { get; }
        bool outgoing { get; }
        float outgoingSupport { get; }
    }
}
