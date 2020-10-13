using System;
using UnityEngine;

namespace Unity.MARS
{
    [Serializable]
    public abstract class ObjectCreationButtonData : ScriptableObject
    {
        public enum CreateInContext
        {
            Scene,
            SyntheticSimulation
        }

        [SerializeField]
        protected string m_ButtonName = "Name not set";

        [SerializeField]
        protected DarkLightIconPair m_Icon;

        [SerializeField]
        protected string m_Tooltip;

        [SerializeField]
        CreateInContext m_CreateInContext = CreateInContext.Scene;

        public string ButtonName => m_ButtonName;

        public CreateInContext CreateInContextSelection => m_CreateInContext;

        public abstract bool CreateGameObject();

        public GUIContent CreationButtonContent()
        {
            return new GUIContent(m_ButtonName, m_Icon.Icon, m_Tooltip);
        }
    }
}
