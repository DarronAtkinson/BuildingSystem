using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Structures
{
    /// <summary>
    /// A point on a structure that required or provides support.
    /// Support is a float value between 0 and 1.
    /// </summary>
    [System.Serializable]
    public struct SupportPoint : ISupportPoint
    {
        [Tooltip("The local position of the support.")]
        [SerializeField] private Vector3 m_position;

        [Tooltip("Defines if the related structure requires this support.")]
        [SerializeField] private bool m_required;

        [Range(0, 1)]
        [Tooltip("The support value needed for this point to be supported.")]
        [SerializeField] private float m_requiredSupport;

        [Tooltip("Defines if this point provides a support value to others.")]
        [SerializeField] private bool m_outgoing;

        [Range(0, 1)]
        [Tooltip("The support value provided by this point.")]
        [SerializeField] private float m_outgoingSupport;

        #region Public Properties

        /// <summary>
        /// Returns the position of the point.
        /// </summary>
        public Vector3 position => m_position;

        /// <summary>
        /// Returns true if the point is required.
        /// </summary>
        public bool required => m_required;

        /// <summary>
        /// Returns the required support value needed for this point.
        /// </summary>
        public float requiredSupport => m_requiredSupport;

        /// <summary>
        /// Returns true if this point provides outgoing support.
        /// </summary>
        public bool outgoing => m_outgoing;

        /// <summary>
        /// Returns the support value provided by this point.
        /// </summary>
        public float outgoingSupport => m_outgoingSupport;

        #endregion
    }
}
