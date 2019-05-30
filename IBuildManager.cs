using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Structures
{
    public interface IBuildManager
    {
        Structure AddStructure(Structure prefab, Vector3 position, Quaternion rotation);
        Support CreateSupport(Vector3 position);
        void RemoveSupport(Support support);
        bool TryGetAtPosition(Vector3 position, out Support support);
    }
}
