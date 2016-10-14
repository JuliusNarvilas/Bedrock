#if DEBUG_LOGS_OFF
#undef DEBUG_LOGS
#elif UNITY_EDITOR || DEBUG
#define DEBUG_LOGS
#endif

#if PRODUCTION_LOGS_OFF
#undef PRODUCTION_LOGS
#else
#define PRODUCTION_LOGS
#endif

using System.Diagnostics;

namespace Common
{
    public class Logger
    {
        public delegate void LoggerOutputTargetFunc(string i_Message, params object[] i_Args);

        private LoggerOutputTargetFunc m_Log;
        private LoggerOutputTargetFunc m_Warning;
        private LoggerOutputTargetFunc m_Error;
        
        public Logger(LoggerOutputTargetFunc i_Log, LoggerOutputTargetFunc i_Warning, LoggerOutputTargetFunc i_Error)
        {
            m_Log = i_Log;
            m_Warning = i_Warning;
            m_Error = i_Error;
        }

        [Conditional("DEBUG_LOGS")]
        public static void DebugLog(string i_Message, params object[] i_Args)
        {
            Debug.m_Log(i_Message, i_Args);
        }
        [Conditional("DEBUG_LOGS")]
        public static void DebugLogIf(bool i_Condition, string i_Message, params object[] i_Args)
        {
            if (i_Condition)
            {
                DebugLog(i_Message, i_Args);
            }
        }

        [Conditional("DEBUG_LOGS")]
        public static void DebugLogWarning(string i_Message, params object[] i_Args)
        {
            Debug.m_Warning(i_Message, i_Args);
        }
        [Conditional("DEBUG_LOGS")]
        public static void DebugLogWarningIf(bool i_Condition, string i_Message, params object[] i_Args)
        {
            if(i_Condition)
            {
                DebugLogWarning(i_Message, i_Args);
            }
        }

        [Conditional("DEBUG_LOGS")]
        public static void DebugLogError(string i_Message, params object[] i_Args)
        {
            Debug.m_Error(i_Message, i_Args);
        }
        [Conditional("DEBUG_LOGS")]
        public static void DebugLogErrorIf(bool i_Condition, string i_Message, params object[] i_Args)
        {
            if (i_Condition)
            {
                DebugLogError(i_Message, i_Args);
            }
        }


        [Conditional("PRODUCTION_LOGS")]
        public static void ProductionLog(string i_Message, params object[] i_Args)
        {
            Production.m_Log(i_Message, i_Args);
        }
        [Conditional("PRODUCTION_LOGS")]
        public static void ProductionLogIf(bool i_Condition, string i_Message, params object[] i_Args)
        {
            if (i_Condition)
            {
                ProductionLog(i_Message, i_Args);
            }
        }

        [Conditional("PRODUCTION_LOGS")]
        public static void ProductionLogWarning(string i_Message, params object[] i_Args)
        {
            Production.m_Warning(i_Message, i_Args);
        }
        [Conditional("PRODUCTION_LOGS")]
        public static void ProductionLogWarningIf(bool i_Condition, string i_Message, params object[] i_Args)
        {
            if (i_Condition)
            {
                ProductionLogWarning(i_Message, i_Args);
            }
        }

        [Conditional("PRODUCTION_LOGS")]
        public static void ProductionLogError(string i_Message, params object[] i_Args)
        {
            Production.m_Error(i_Message, i_Args);
        }
        [Conditional("PRODUCTION_LOGS")]
        public static void ProductionLogErrorIf(bool i_Condition, string i_Message, params object[] i_Args)
        {
            if (i_Condition)
            {
                ProductionLogError(i_Message, i_Args);
            }
        }

        public static Logger Default = new Logger(UnityEngine.Debug.LogFormat, UnityEngine.Debug.LogWarningFormat, UnityEngine.Debug.LogErrorFormat);

        public static Logger Debug = Default;
        public static Logger Production = Default;
    }
}
