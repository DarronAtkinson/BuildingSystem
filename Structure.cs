using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

namespace Structures
{
    /// <summary>
    /// A concrete class to represent a structure object placed in the world.
    /// </summary>
    public class Structure : StructureBase, ISupportProvider
    {
        /// <summary>
        /// A dictionary of support point indices, keyed by the support they are registered to.
        /// </summary>
        private Dictionary<Support, int> supportLookup = new Dictionary<Support, int>();

        #region Public Properties

        /// <summary>
        /// Implementation of ISupportProvider.
        /// </summary>
        /// <returns>An array of this structures support points.</returns>
        public ISupportPoint[] GetSupportPoints()
        {
            return supportPoints.array;
        }

        #endregion

        #region Cleanup

        private void OnDestroy()
        {
            // Clear the build manager reference.
            SetBuildManager(null);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Ensure the structure unsubscribes from the current manager.
        /// Subscribes to the new manager if required.
        /// </summary>
        /// <param name="manager">The new build manager.</param>
        public override void OnBuildManagerChange(IBuildManager manager)
        {
            UnsubscribeFromSupports();
            if (manager != null)
                SubscribeSupportsToManager(manager);
        }

        /// <summary>
        /// Returns true if the support provides a suitable support value for the point.
        /// </summary>
        /// <param name="support">The support from the build manager.</param>
        /// <param name="point">The support point subscribed to the support.</param>
        /// <returns></returns>
        public override bool OnRequiredSupportsAvailable(ISupport support, ISupportPoint point)
        {
            // A support cannot provide a support value for itself.
            return point.requiredSupport < support.supportValue - point.outgoingSupport;
        }

        /// <summary>
        /// Determines if the change to the support will cause the structure to be destroyed.
        /// </summary>
        /// <param name="support">The support from the build manager.</param>
        /// <param name="point">The support point subscribed to the support.</param>
        public virtual void OnSupportChanged(ISupport support, ISupportPoint point)
        {
            // Only a change to required supports can trigger destruction.
            if (point.required && support.supportValue - point.outgoingSupport < point.requiredSupport)
            {
                // The support point is no longer supported so we check 
                // to ensure the structure still has enough support.
                if (!RequiredSupportsAvailable())
                {
                    // The structure will be destroyed so unsubscribe from the supports.
                    UnsubscribeFromSupports();

                    var destruction = GetComponent<Destruction>();
                    if (destruction != null)
                    {
                        destruction.AddRigid();
                    }
                    else
                    {
                        Destroy(gameObject);
                    }
                } 
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="support"></param>
        public virtual void OnSupportDestroy(Support support)
        {

        }

        #endregion

        #region Private Methods

        protected void SubscribeSupportsToManager(IBuildManager manager)
        {
            supportLookup.Clear();

            for (int i = 0; i < supportPoints.count; i++)
            {
                var supportPoint = supportPoints.Get(i);

                var position = transform.TransformPoint(supportPoint.position);

                Support support;
                if (!manager.TryGetAtPosition(position, out support))
                {
                    support = manager.CreateSupport(position);
                }

                support.Subscribe(this, supportPoint);
                support.OnSupportChanged += InternalOnSupportChanged;
                support.OnSupportDestroy += InternalOnSupportDestroy;

                supportLookup.Add(support, i);
            }
        }

        protected void UnsubscribeFromSupports()
        {
            foreach (var support in supportLookup.Keys)
            {
                support.OnSupportChanged -= InternalOnSupportChanged;
                support.OnSupportDestroy -= InternalOnSupportDestroy;
                support.Unsubscribe(this);
            }

            supportLookup.Clear();
        }

        private void InternalOnSupportChanged(Support support)
        {
            if (supportLookup.TryGetValue(support, out var index))
                OnSupportChanged(support, supportPoints.Get(index));
        }

        private void InternalOnSupportDestroy(Support support)
        {
            supportLookup.Remove(support);
            OnSupportDestroy(support);
        }

        #endregion
    }
}
