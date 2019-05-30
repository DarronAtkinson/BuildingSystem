using UnityEngine;
using Collections.Octree;

namespace Structures
{
    /// <summary>
    /// Represents the support points in the world for a particular player.
    /// </summary>
    [CreateAssetMenu(menuName = "Structures/PlayerBuildManger")]
    public class PlayerBuildManager : ScriptableObject, IBuildManager
    {
        [Tooltip("The radius to use where searching for a support within the manager.")]
        [SerializeField] private float supportSearchRadius = 0.1f;

        // The support points in the world.
        private Octree<Support> supportTree = new Octree<Support>();

        // A buffer array used for non alloc search operations.
        private Support[] searchBuffer = new Support[4];

        #region Public Methods

        /// <summary>
        /// Create a new structure from the provided prefab at the position with the given rotation.
        /// The structure is registered with this build manager.
        /// </summary>
        /// <param name="prefab">The structure prefab to create.</param>
        /// <param name="position">The position to place the structure in the world.</param>
        /// <param name="rotation">The rotation of the structure.</param>
        /// <returns>The created structure.</returns>
        public Structure AddStructure(Structure prefab, Vector3 position, Quaternion rotation)
        {
            // Guard statement to prevent the creation of a non structure.
            if (!prefab.GetComponent<Structure>())
                throw new MissingComponentException(prefab + " is missing a " + typeof(Structure));

            // Create the structure in the world.
            var go = Instantiate(prefab, position, rotation);

            // Get the structure component.
            var structure = go.GetComponent<Structure>();

            // Subscribe the structure to this build manager.
            structure.SetBuildManager(this);

            return structure;
        }

        /// <summary>
        /// Creates a new support object and adds it to this build manager.
        /// </summary>
        /// <param name="position">The world position of the support.</param>
        /// <returns>The created support.</returns>
        public Support CreateSupport(Vector3 position)
        {
            // Create the support.
            var support = Support.Create(position);

            // Register to be notified when the support is destroyed.
            support.OnSupportDestroy += RemoveSupport;

            // Add the support to the tree.
            supportTree.Add(position, support);

            return support;
        }

        /// <summary>
        /// Removes a support from this build manager.
        /// </summary>
        /// <param name="support"></param>
        public void RemoveSupport(Support support)
        {
            // Unregister from being notified when the support has been destroyed.
            support.OnSupportDestroy -= RemoveSupport;

            // Remove the support from the tree.
            supportTree.Remove(support.position);
        }

        /// <summary>
        /// Return true if a support if found at the given position.
        /// </summary>
        /// <param name="position">The position to query.</param>
        /// <param name="support">The support found at the postion.</param>
        /// <returns>True if a support is found.</returns>
        public bool TryGetAtPosition(Vector3 position, out Support support)
        {
            // Assign the support to a default value.
            support = null;

            // Query the tree.
            var found = supportTree.GetNearbyNonAlloc(position, supportSearchRadius, searchBuffer);

            if (found == 0)
            {
                // No support was found at the position.
                return false;
            }
            else
            {
                // A support was found.
                // Find the closest support to the position.
                var closestDistance = float.PositiveInfinity;
                for (int i = 0; i < found; i++)
                {
                    var distance = Vector3.Distance(searchBuffer[i].position, position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        support = searchBuffer[i];
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// Draws a representation of the tree.
        /// Must be called in OnDrawGizmos().
        /// </summary>
        public void DrawGizmos()
        {
            supportTree.DrawBounds();
            supportTree.DrawObjects();
        }

        #endregion

        #region Cleanup

        private void OnDestroy()
        {
            supportTree = null;
        }

        #endregion
    }
}

