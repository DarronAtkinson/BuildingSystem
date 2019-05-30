using UnityEngine;
using System;
using System.Collections.Generic;
using Collections.Octree;

namespace Structures
{
    /// <summary>
    /// A support is the representation of a support point within a build manager.
    /// Mulitple structures can register a support point to a single support.
    /// </summary>
    [Serializable]
    public class Support : ScriptableObject, ISupport
    {
        #region Static Methods

        /// <summary>
        /// Creates a new support at the position.
        /// </summary>
        /// <param name="position">The positino of the support.</param>
        /// <returns>The new support.</returns>
        public static Support Create(Vector3 position)
        {
            var support = CreateInstance<Support>();
            support.position = position;
            return support;
        }

        #endregion

        #region Events

        /// <summary>
        /// Fires when the support value has changed.
        /// </summary>
        public Action<Support> OnSupportChanged;

        /// <summary>
        /// Fires when the support is destroyed.
        /// </summary>
        public Action<Support> OnSupportDestroy;

        #endregion

        /// <summary>
        /// The world position of this support.
        /// </summary>
        public Vector3 position { get; private set; }

        /// <summary>
        /// A heap structure to store the structures registered to this support.
        /// The structure providing the highest support rises to the top of the heap.
        /// </summary>
        [SerializeField] private StructureHeap structures = new StructureHeap();

        /// <summary>
        /// Cached value of the support provided by the structure at the top of the heap.
        /// </summary>
        [SerializeField] private float m_supportValue;

        #region Public properties

        /// <summary>
        /// Returns the support value for this support.
        /// </summary>
        public float supportValue => m_supportValue;

        #endregion

        #region Public Methods

        /// <summary>
        /// Subscribes a support provider and related support point to this support.
        /// </summary>
        /// <param name="provider">The structure providing support.</param>
        /// <param name="support">The support point of the structure located at this position of this support.</param>
        public void Subscribe(ISupportProvider provider, ISupportPoint support)
        {
            // Push the provider to the heap.
            structures.Push(provider, support.outgoingSupport);

            // Recaluculate support value.
            SetSupportValue();
        }

        /// <summary>
        /// Unsubscribes a provider from this support.
        /// </summary>
        /// <param name="provider">The structure to remove from the support.</param>
        public void Unsubscribe(ISupportProvider provider)
        {
            // Remove the provider from the heap.
            structures.Remove(provider);

            // Recalculate the support value.
            SetSupportValue();

            if (supportValue == 0)
            {
                // This support is no longer supported.
                Destroy(this);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Recalculates the support value.
        /// </summary>
        private void SetSupportValue()
        {
            // Get the value from the heap if possible.
            var value = structures.Count > 0 ? structures.Peek().Value : 0;

            // Keep a record of the previous value to check for change.

            // Set the support value.
            var oldValue = m_supportValue;
            m_supportValue = value;

            // Check for change and alert subscribed providers.
            if (value != oldValue)
                OnSupportChanged?.Invoke(this);
        }

        #endregion

        #region Cleanup

        // Remove all references.
        private void OnDestroy()
        {
            OnSupportDestroy?.Invoke(this);
            OnSupportDestroy = null;
            OnSupportChanged = null;
            structures.Clear();
        }

        #endregion
    }
}


