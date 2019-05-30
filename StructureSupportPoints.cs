using UnityEngine;
using System.Collections.Generic;

namespace Structures
{
    /// <summary>
    /// A shared collection of support points for a structure object.
    /// 
    /// The points are defined in local space relative to the structure they will represent.
    /// 
    /// The same StructureSupportPoints object can be used across multiple
    /// varieties of the same structure.
    /// 
    /// For example a two wall structure may have identical support points but
    /// provided different functionality.
    /// </summary>
    [CreateAssetMenu(menuName = "Structures/SupportPoints")]
    public class StructureSupportPoints : ScriptableObject
    {
        [Tooltip("A collection of support points on a structure.")]
        [SerializeField] private List<SupportPoint> supportPoints = new List<SupportPoint>();

        [Tooltip("A structure without any support points can still snap to a surface.")]
        [SerializeField] private bool m_snapsToSurface = false;

        #region Public Properties

        /// <summary>
        /// Returns the number of support points.
        /// </summary>
        public int count => supportPoints.Count;

        /// <summary>
        /// Returns true if the object can snap to a surface.
        /// </summary>
        public bool snapsToSurface => m_snapsToSurface;

        /// <summary>
        /// Returns an array of all the support points.
        /// </summary>
        public ISupportPoint[] array
        {
            get
            {
                var results = new ISupportPoint[supportPoints.Count];
                for (int i = 0; i < supportPoints.Count; i++)
                {
                    results[i] = supportPoints[i];
                }
                return results;
            }
        }

        /// <summary>
        /// Returns an array of all the required support points.
        /// </summary>
        public ISupportPoint[] required
        {
            get
            {
                var results = new List<ISupportPoint>();
                for (int i = 0; i < supportPoints.Count; i++)
                {
                    if (supportPoints[i].required)
                        results.Add(supportPoints[i]);
                }
                return results.ToArray();
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Get the support point at the given index.
        /// The index must be valid.
        /// </summary>
        /// <param name="index">The index to retrieve.</param>
        /// <returns>The support point at the index given.</returns>
        public ISupportPoint Get(int index)
        {
            return supportPoints[index];
        }

        /// <summary>
        /// Finds the support point closest to the given position.
        /// </summary>
        /// <param name="position">The position to use as the origin for the search.</param>
        /// <returns>The closest support point.</returns>
        public ISupportPoint GetClosestSupportPointTo(Vector3 position)
        {
            if (count == 0) return null;

            var result = supportPoints[0];

            var closestDistance = float.PositiveInfinity;
            for (int i = 0; i < supportPoints.Count; i++)
            {
                var distance = Vector3.Distance(supportPoints[i].position, position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    result = supportPoints[i];
                }
            }

            return result;
        }

        #endregion
    }
}
