using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Structures
{
    public interface ISupport
    {
        float supportValue { get; }
        void Subscribe(ISupportProvider provider, ISupportPoint support);
        void Unsubscribe(ISupportProvider provider);
    }
}
