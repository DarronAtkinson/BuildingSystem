using UnityEngine;

namespace Structures
{
    [CreateAssetMenu(menuName = "Structures/Settings")]
    public class StructureSettings : ScriptableObject
    {
        [Header("Preview")]
        [SerializeField] LayerMask m_ignoreRaycastLayer;
        [SerializeField] LayerMask m_structurePreviewLayer;
        [SerializeField] float m_defaultPlacementDistance = 5;
        [SerializeField] float m_maxPlacementDistance = 5;

        #region Public Properties

        public LayerMask ignoreRaycastLayer => m_ignoreRaycastLayer;
        public LayerMask structurePreviewLayer => m_structurePreviewLayer;
        public float defaultPlacementDistance => m_defaultPlacementDistance;
        public float maxPlacementDistance => m_maxPlacementDistance;

        #endregion

        public static StructureSettings CreateDefault()
        {
            var settings = CreateInstance<StructureSettings>();
            settings.m_structurePreviewLayer = LayerMask.GetMask("Ground", "Structure");
            settings.m_ignoreRaycastLayer = LayerMask.NameToLayer("Ignore Raycast");
            return settings;
        }
    }
}
