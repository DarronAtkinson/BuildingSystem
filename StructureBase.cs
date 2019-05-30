using UnityEngine;

namespace Structures
{
    /// <summary>
    /// Abstract class for a structure object.
    /// </summary>
    public abstract class StructureBase : MonoBehaviour
    {
        [Tooltip("Reference to a set of shared support points for a structure of this type.")]
        [SerializeField] protected StructureSupportPoints m_supportPoints;

        [Tooltip("Indicates if this structure is a foundation.")]
        [SerializeField] private bool m_isFoundation;

        [Tooltip("Defines the minimum number of required supports are needed to be considered supported")]
        [SerializeField] private int m_numberOfSupportsRequired = 0;

        [HideInInspector] // A reference to the build manager for this structure.
        [SerializeField] private IBuildManager m_buildManager;

        #region Public properties

        /// <summary>
        /// Returns the shared support points for this type of structure.
        /// </summary>
        public StructureSupportPoints supportPoints => m_supportPoints;

        /// <summary>
        /// Returns true is this structure is a foundation.
        /// </summary>
        public bool isFoundation => m_isFoundation;

        /// <summary>
        /// Returns the minimum number of required supports needed to be considered supported.
        /// </summary>
        public int numberOfSupportsRequired => m_numberOfSupportsRequired;

        /// <summary>
        /// The build manager this structure is subscribed to.
        /// </summary>
        public IBuildManager buildManager => m_buildManager;

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Called before the build manager is set.
        /// </summary>
        /// <param name="manager">The new build manager.</param>
        public abstract void OnBuildManagerChange(IBuildManager manager);

        /// <summary>
        /// Called when checking if required supports are available in the build manager.
        /// Will only be called is a valid support is present.
        /// </summary>
        /// <param name="support">The support at the position defined by the point.</param>
        /// <param name="point">The support point that relates to the support found in the build manager.</param>
        /// <returns></returns>
        public abstract bool OnRequiredSupportsAvailable(ISupport support, ISupportPoint point);

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets the build manager for this structure.
        /// </summary>
        /// <param name="manager">The build manager to set.</param>
        public void SetBuildManager(IBuildManager manager)
        {
            OnBuildManagerChange(manager);
            m_buildManager = manager;
        }

        /// <summary>
        /// Returns the world position of the support point closest to the given point.
        /// </summary>
        /// <param name="point">The point to check.</param>
        /// <returns>The world position of the closest support point.</returns>
        public Vector3 GetClosestSupportPointTo(Vector3 point)
        {
            // Get the local position of point.
            var position = transform.InverseTransformPoint(point);

            // Get the local position of the closest support point.
            var result = supportPoints.GetClosestSupportPointTo(position);

            // Transform the local position to world space.
            return transform.TransformPoint(result.position);
        }

        /// <summary>
        /// Returns true if the build manager contains 
        /// the required supports for this structure.
        /// </summary>
        /// <returns>True is the structure is supported.</returns>
        public bool RequiredSupportsAvailable()
        {
            // When no supports are required structure is always considered to be supported.
            if (numberOfSupportsRequired == 0)
                return true;

            int numberOfSupports = 0;

            for (int i = 0; i < supportPoints.count; i++)
            {
                var supportPoint = supportPoints.Get(i);

                // Only test supports that are required.
                if (supportPoint.required)
                {
                    // Transform the support point into world space.
                    var position = transform.TransformPoint(supportPoint.position);

                    // Check to see if the build manager contains a support at this position.
                    Support support;
                    if (buildManager.TryGetAtPosition(position, out support))
                    {
                        // Call the abstract method to allow subclasses 
                        // to define what is considered valid support.
                        if (OnRequiredSupportsAvailable(support, supportPoint))
                        {
                            numberOfSupports++;
                        }
                    }
                }
            }

            return numberOfSupportsRequired <= numberOfSupports;
        }

        #endregion

        #region Gizmos

        /// <summary>
        /// Draws a blue sphere at the position of each support point.
        /// </summary>
        private void OnDrawGizmos()
        {
            for (int i = 0; i < supportPoints.count; i++)
            {
                Gizmos.color = Color.blue;

                var point = supportPoints.Get(i);
                var position = transform.TransformPoint(point.position);
                Gizmos.DrawSphere(position, 0.1f);
            }
        }

        #endregion
    }
}
