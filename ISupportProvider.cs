using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Structures
{
    public interface ISupportProvider
    {
        ISupportPoint[] GetSupportPoints();
    }
}