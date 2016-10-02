using Common.IO;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Common.Text
{
    [ExecuteInEditMode]
    public class IntelligentTextSettingsManager : MonoBehaviour
    {
        public static readonly string LOCALIZATION_ID_GLOBAL = "global";
        public static readonly string LOCALIZATION_SAVE_ID = "currentLocalization";
        public delegate string CustomInsertProvider();
        private static IntelligentTextSettingsManager s_Instance = null;
        private static Dictionary<int, IntelligentText> s_PendingTextRegistry = new Dictionary<int, IntelligentText>();

        public static IntelligentTextSettingsManager Instance
        {
            get { return s_Instance; }
        }

        [SerializeField]
        private TextAsset m_Localisations = null;

        
        private Dictionary<string, IntelligentTextLocalizationRecord> m_LocalizationsRecords = new Dictionary<string, IntelligentTextLocalizationRecord>();
        private Dictionary<int, IntelligentText> m_ActiveTextRegistry = new Dictionary<int, IntelligentText>();
        private Dictionary<string, CustomInsertProvider> m_CustomInserts = new Dictionary<string, CustomInsertProvider>();

        private IntelligentTextLocalizationRecord m_CurrentLocalizationRecord;
        private IntelligentTextLocalization m_CurrentLocalization = IntelligentTextLocalization.Create();


        public static string CurrentLocalizationId
        {
            get { return s_Instance.m_CurrentLocalizationRecord.id; }
        }
        public static string CurrentLocalizationDisplayName
        {
            get { return s_Instance.m_CurrentLocalizationRecord.displayName; }
        }
        public static IEnumerable<IntelligentTextLocalizationRecord> LocalizationsRecords
        {
            get { return s_Instance.m_LocalizationsRecords.Values; }
        }

        public static void SetCustomInsert(string i_Id, CustomInsertProvider i_Response)
        {
#if UNITY_EDITOR
            if(s_Instance.m_CustomInserts.ContainsKey(i_Id))
            {
                Debug.LogWarningFormat("IntelligentText SetCustomInsert overwrites existing entry with id: {0}", i_Id);
            }
#endif
            s_Instance.m_CustomInserts[i_Id] = i_Response;
        }
        public static bool RemoveCustomInsert(string i_Id)
        {
            return s_Instance.m_CustomInserts.Remove(i_Id);
        }


        public static void RegisterText(IntelligentText i_Text)
        {
            if (s_Instance != null)
            {
                s_Instance.m_ActiveTextRegistry[i_Text.GetInstanceID()] = i_Text;
                i_Text.Refresh();
            }
            else
            {
                s_PendingTextRegistry[i_Text.GetInstanceID()] = i_Text;
            }
        }
        public static void UnregisterText(IntelligentText i_Text)
        {
            if (s_Instance != null)
            {
                s_Instance.m_ActiveTextRegistry.Remove(i_Text.GetInstanceID());
            }
            else
            {
                s_PendingTextRegistry.Remove(i_Text.GetInstanceID());
            }
        }

        public static string GetInsert(string i_Id)
        {
            CustomInsertProvider customInsertFunc;
            if(s_Instance.m_CustomInserts.TryGetValue(i_Id, out customInsertFunc))
            {
                return customInsertFunc();
            }
            string result;
            if(s_Instance.m_CurrentLocalization.Inserts.TryGetValue(i_Id, out result))
            {
                return result;
            }
            Debug.LogErrorFormat("IntelligentText Insert not found with id: {0}", i_Id);
            return string.Format("[{0}]", i_Id);
        }

        public static IntelligentTextStyle GetStyle(string i_Id)
        {
            IntelligentTextStyle result;
            if (s_Instance.m_CurrentLocalization.Styles.TryGetValue(i_Id, out result))
            {
                return result;
            }
            Debug.LogErrorFormat("IntelligentText Style not found with id: {0}", i_Id);
            return null;
        }

        public static void Refresh()
        {
            //regenerate text
            foreach(var intelligentText in s_Instance.m_ActiveTextRegistry.Values)
            {
                intelligentText.Refresh();
            }
        }

        public static void Reload()
        {
            s_Instance.m_CurrentLocalization.Clear();
            IntelligentTextLocalizationRecord record;
            if (s_Instance.m_LocalizationsRecords.TryGetValue(LOCALIZATION_ID_GLOBAL, out record))
            {
                var globalResource = ResourcesDB.GetByPath(record.path);
                if (globalResource != null)
                {
                    var currentGlobalSource = globalResource.Load<TextAsset>();
                    var globals = JsonUtility.FromJson<IntelligentTextLocalizationData>(currentGlobalSource.text);
                    s_Instance.m_CurrentLocalization.Append(globals);
                    globalResource.Unload();
                }
            }
            var resource = ResourcesDB.GetByPath(s_Instance.m_CurrentLocalizationRecord.path);
            var currentLocalizationSource = resource.Load<TextAsset>();
            var localization = JsonUtility.FromJson<IntelligentTextLocalizationData>(currentLocalizationSource.text);
            s_Instance.m_CurrentLocalization.Append(localization);
            resource.Unload();

            Refresh();
        }

        public static bool SetLocalization(string i_LocalizationId, bool i_Force = false)
        {
            if(s_Instance.m_CurrentLocalizationRecord.id != i_LocalizationId || i_Force)
            {
                IntelligentTextLocalizationRecord record;
                if (s_Instance.m_LocalizationsRecords.TryGetValue(i_LocalizationId, out record))
                {
                    s_Instance.m_CurrentLocalizationRecord = record;
                    Reload();
                    return true;
                }
            }
            return false;
        }

        void Awake()
        {
            if (s_Instance == null)
            {
                var localizationsContainer = JsonUtility.FromJson<IntelligentTextLocalizationsContainer>(m_Localisations.text);
                foreach(var localizationRecord in localizationsContainer.localizationList)
                {
                    m_LocalizationsRecords[localizationRecord.id] = localizationRecord;
                }
                s_Instance = this;
                Debug.Assert(m_LocalizationsRecords.Count > 0, "No LocalizationsRecords");

                string savedLocalization = PlayerPrefs.GetString(LOCALIZATION_SAVE_ID, string.Empty);
                if(string.IsNullOrEmpty(savedLocalization))
                {
                    savedLocalization = System.Globalization.CultureInfo.CurrentCulture.Name;
                }
                if(!SetLocalization(savedLocalization, true))
                {//fallback
                    if(savedLocalization.Length > 2)
                    {
                        var culture = new System.Globalization.CultureInfo(savedLocalization);
                        savedLocalization = culture.TwoLetterISOLanguageName;
                        SetLocalization(savedLocalization, true);
                    }
                    if (m_CurrentLocalizationRecord.id == null)
                    {
                        //pick first option
                        SetLocalization(m_LocalizationsRecords.Values.GetEnumerator().Current.id, true);
                    }
                }

                var pendingTexts = s_PendingTextRegistry.Values;
                foreach(var pendingText in pendingTexts)
                {
                    RegisterText(pendingText);
                }
                s_PendingTextRegistry.Clear();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        void OnDestroy()
        {
            if(s_Instance == this)
            {
                s_Instance = null;
            }
        }
    }
}
