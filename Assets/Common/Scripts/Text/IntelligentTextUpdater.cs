using UnityEngine;

namespace Common.Text
{
    [RequireComponent(typeof(IntelligentText))]
    public class IntelligentTextUpdater : MonoBehaviour
    {
        private IntelligentText m_IntelligentText;
        private float m_TimeAccumulator = 0;

        public float UpdateTimer = 0;
        public bool ScaledTime = false;

        private void Awake()
        {
            m_IntelligentText = GetComponent<IntelligentText>();
            if (m_IntelligentText == null)
            {
                enabled = false;
                Debug.LogWarningFormat("IntelligentTextUpdater with no IntelligentText on GameObject: {0}", gameObject.name);
            }
        }

        private void Update()
        {
            m_TimeAccumulator += ScaledTime ? Time.deltaTime : Time.unscaledDeltaTime;
            if(m_TimeAccumulator >= UpdateTimer)
            {
                m_TimeAccumulator -= UpdateTimer;
                m_IntelligentText.UpdateText();
            }
        }
    }
}
