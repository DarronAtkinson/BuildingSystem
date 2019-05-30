using UnityEngine;
using System;

namespace Structures
{
    public class StructurePreview : StructureBase
    {
        /// <summary>
        /// Defines an int grid.
        /// </summary>
        public struct Grid
        {
            // The matrix of the object which this grid snaps to.
            private Matrix4x4 localToWorldMatrix;

            /// <summary>
            /// Returns the rotation from the transformation matrix.
            /// </summary>
            public Quaternion rotation => localToWorldMatrix.rotation;

            /// <summary>
            /// Creates a new grid at the given position with the given rotation.
            /// </summary>
            /// <param name="position">World space position.</param>
            /// <param name="rotation">world space rotation.</param>
            public Grid(Vector3 position, Quaternion rotation)
            {
                localToWorldMatrix = Matrix4x4.TRS(position, rotation, Vector3.one);
            }

            /// <summary>
            /// Converts the given point into grid space.
            /// </summary>
            /// <param name="point">A world space point.</param>
            /// <returns>The point in grid space.</returns>
            public Vector3 Convert(Vector3 point)
            {
                // Get the local position of the point.
                var localPoint = localToWorldMatrix.inverse.MultiplyPoint3x4(point);

                // Convert the local point to Vector3Int and return this in world space.
                return localToWorldMatrix.MultiplyPoint3x4(Vector3Int.RoundToInt(localPoint));
            }
        }

        #region Static Methods

        /// <summary>
        /// Instantiates the given prefab object and returns the attached StructurePreview component.
        /// </summary>
        /// <param name="prefab">A prefab obejct with a StructurePreview component</param>
        /// <returns>The StrcutrePreview component attached to the prefab object.</returns>
        public static StructurePreview Create(GameObject prefab)
        {
            if (prefab.GetComponent<StructurePreview>() == null)
                throw new MissingComponentException(prefab + " missing StructurePreview component");

            var gameObject = Instantiate(prefab);
            return gameObject.GetComponent<StructurePreview>();
        }

        #endregion

        [Tooltip("The prefab of the actual structure object to be used in game.")] 
        [SerializeField] private Structure prefab;

        [Tooltip("Reference to the shared structures settings.")]
        [SerializeField] private StructureSettings settings;

        [Tooltip("Defines if this structure can snap to a grid.")]
        [SerializeField] private bool snapsToGrid;

        // The previewer object that created this structure preview object.
        // Usually a player character.
        private IBuildPreviewer m_previewer;

        // The grid definition to be used for snapping.
        private Grid grid;

        // Defines if the object should snap to the Grid.
        private bool useGrid;

        // Defines if the structure has support and can their be placed.
        private bool hasSupport;

        #region Public Properties

        /// <summary>
        /// Getter and setter for the previewer object.
        /// Subscriptions to events in handled.
        /// </summary>
        public IBuildPreviewer previewer
        {
            get { return m_previewer; }
            set
            {
                UnsubscribeFromEvents();
                m_previewer = value;
                SubscribeToEvents();
            }
        }

        #endregion

        #region Initialisation and Cleanup

        private void OnEnable()
        {
            SetInitialPosition();
        }

        /// <summary>
        /// Sets the initial position of the object based on the settings.
        /// </summary>
        private void SetInitialPosition()
        {
            var ray = Camera.main.ViewportCenterRay();
            if (Physics.Raycast(ray, out var hit, settings.maxPlacementDistance, settings.structurePreviewLayer))
            {
                transform.position = hit.point;
            }
            else
            {
                transform.position = ray.origin + settings.defaultPlacementDistance * ray.direction;
            }
        }

        private void OnDisable()
        {
            // Destroy on editor reload.
            // Is this the correct process for doing this??
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            previewer = null;
        }

        #endregion

        #region Update

        private void Update()
        {
            hasSupport = GetSupport();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns true if the support is capable to supporting this structure.
        /// </summary>
        /// <param name="support">The support from the build manager.</param>
        /// <param name="point">The support points subscribed to the support.</param>
        /// <returns></returns>
        public override bool OnRequiredSupportsAvailable(ISupport support, ISupportPoint point)
        {
            return support.supportValue >= point.requiredSupport;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="manager"></param>
        public override void OnBuildManagerChange(IBuildManager manager)
        {

        }

        #endregion;

        #region Private Methods

        /// <summary>
        /// Positions the structure based on the structures within the world.
        /// </summary>
        /// <returns>Returns true if the position is valid.</returns>
        private bool GetSupport()
        {
            var ray = Camera.main.ViewportCenterRay();

            if (Physics.Raycast(ray, out var hit, settings.maxPlacementDistance, settings.structurePreviewLayer))
            {
                // Snaps to the object hit if the object has support points.
                if (SetPositionFromHit(hit))
                {
                    // Non foundations must be checked for valid supports.
                    if (!isFoundation)
                        return RequiredSupportsAvailable();
                }
                else
                {
                    // The structure could not be snapped to the hit object
                    // so the structure is placed at the hit point.
                    SetTransform(hit.point, ray.direction);
                }
            }
            else
            {
                // Position the structure in the default position if no hit occurs.
                SetTransform(ray.origin + settings.defaultPlacementDistance * ray.direction, ray.direction);
            }

            return false;
        }

        /// <summary>
        /// Snaps the structure to the hit object if the hit object has support points.
        /// If the preview is using grid snapping then the structure will snap to the grid
        /// based on the position of the hit point.
        /// </summary>
        /// <param name="hit">The raycast hit from the viewport center.</param>
        /// <returns>True if the structure has snapped to a position.</returns>
        private bool SetPositionFromHit(RaycastHit hit)
        {
            // Snap to grid if needed.
            if (useGrid)
            {
                SnapToGrid(hit);
                return true;
            }

            // Try to get the structure connected to the hit.
            var structure = hit.transform.GetComponent<Structure>();
            if (structure != null)
            {
                // TODO - Refine structure snapping.

                // Structure found.
                // Prevent placement normal from pointing up.
                var normal = hit.normal == Vector3.up ? structure.transform.forward : hit.normal;

                // Snap the structure to the surface if required.
                if (supportPoints.snapsToSurface)
                {
                    SetTransform(hit.point, normal);
                    return true;
                }

                // Get the closest snap point to hit point.
                var closestPoint = structure.GetClosestSupportPointTo(hit.point);

                // Snap the first required support point to the closest point.
                var requiredSupports = supportPoints.required;
                if (requiredSupports.Length > 0)
                {
                    var offset = transform.TransformVector(requiredSupports[0].position);
                    closestPoint += offset;
                }

                // Snap the preview transform to the structure.
                SetTransform(closestPoint, normal);
                return true;
            }

            // The structure was unable to snap into position.
            return false;              
        }

        /// <summary>
        /// Snaps the structure to the grid based on the given hit.
        /// </summary>
        /// <param name="hit">A raycast hit representing the raw position.</param>
        private void SnapToGrid(RaycastHit hit)
        {
            // Snap the hit point to the grid.
            var gridPoint = grid.Convert(hit.point);

            // Reset the y value to the raw value.
            transform.position = new Vector3(gridPoint.x, hit.point.y, gridPoint.z);

            // Set the rotation to the rotation around the y axis.
            transform.rotation = Utility.EulerY(grid.rotation);
        }

        /// <summary>
        /// Set the transform to the given position.
        /// The rotation is set to the rotation of the 
        /// forward direction around the y axis.
        /// </summary>
        /// <param name="position">The desired position.</param>
        /// <param name="forward">The desired raw forward direction.</param>
        private void SetTransform(Vector3 position, Vector3 forward)
        {
            transform.position = position;
            transform.rotation = Utility.EulerY(forward);
        }

        /// <summary>
        /// Subscribes this object to the events on the previewer.
        /// </summary>
        private void SubscribeToEvents()
        {
            if (previewer == null) return;

            previewer.OnConfirm += OnConfirm;
            previewer.OnCancel += OnCancel;

            if (snapsToGrid)
                previewer.OnSnapToGrid += OnSnapToGrid;
        }

        /// <summary>
        /// Unsubscribes this object from the events on the previewer.
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            if (previewer == null) return;

            previewer.OnConfirm -= OnConfirm;
            previewer.OnCancel -= OnCancel;

            if (snapsToGrid)
                previewer.OnSnapToGrid -= OnSnapToGrid;
        }

        #endregion

        #region Event Callbacks

        /// <summary>
        /// Creates a new grid based on the object found via raycast from the viewport center.
        /// </summary>
        private void OnSnapToGrid()
        {
            if (useGrid)
                useGrid = false;
            else
            {
                var ray = Camera.main.ViewportCenterRay();
                if (Physics.Raycast(ray, out var hit, settings.maxPlacementDistance, settings.structurePreviewLayer))
                {
                    grid = new Grid(hit.transform.position, hit.transform.rotation);
                    useGrid = true;
                }
            }
        }

        /// <summary>
        /// Adds the structure to the build manager if the structure has valid support.
        /// This will instantiate the concrete in game version of the structure.
        /// </summary>
        private void OnConfirm()
        {
            if (hasSupport || isFoundation)
            {
                buildManager.AddStructure(prefab, transform.position, transform.rotation);
                // Removed to allow preview to be reused.
                // Destroy(gameObject);
            }
        }

        /// <summary>
        /// Cancelling will destroy the preview object.
        /// </summary>
        private void OnCancel()
        {
            Destroy(gameObject);
        }

        #endregion
    }
}
