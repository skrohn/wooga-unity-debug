using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityDebug = UnityEngine.Debug;
using System.Threading;
using System.Diagnostics;

public static class WoogaDebug
{
	private const string NULL = "<NULL>";
	private const string EMPTY = "<EMPTY>";
	private const string NOTHING = "<NOTHING_TO_LOG>";

	[MethodImpl(MethodImplOptions.NoInlining)]
	public static string GetCurrentMethodName ()
	{
		return new StackTrace().GetFrame(1).GetMethod().Name;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public static string GetCurrentClassAndMethod ()
	{
		return ClassAndMethodName();
	}

	public static void Trace(params object[] logObjects)
	{
		if (LogIt(LogLevels.Trace))
		{
			UnityDebug.Log(ClassAndMethodName() + LogString(logObjects));
		}
	}

	public static void Log(params object[] logObjects)
	{
		if (LogIt(LogLevels.Info))
		{
			UnityDebug.Log(ClassAndMethodName() + LogString(logObjects));
		}
	}

	public static void LogWarning(params object[] logObjects)
	{
		if (LogIt(LogLevels.Warning))
		{
			UnityDebug.LogWarning(ClassAndMethodName() + LogString(logObjects));
		}
	}

	public static void LogError(params object[] logObjects)
	{
		if (LogIt(LogLevels.Error))
		{
			UnityDebug.LogError(ClassAndMethodName() + LogString(logObjects));
		}
	}

	public static void TraceFormatted(string format, params object[] logObjects)
	{
		if (LogIt(LogLevels.Trace))
		{
			UnityDebug.Log(ClassAndMethodName() + LogStringFormatted(format, logObjects));
		}
	}

	public static void LogFormatted(string format, params object[] logObjects)
	{
		if (LogIt(LogLevels.Info))
		{
			UnityDebug.Log(ClassAndMethodName() + LogStringFormatted(format, logObjects));
		}
	}

	public static void LogWarningFormatted(string format, params object[] logObjects)
	{
		if (LogIt(LogLevels.Warning))
		{
			UnityDebug.LogWarning(ClassAndMethodName() + LogStringFormatted(format, logObjects));
		}
	}

	public static void LogErrorFormatted(string format, params object[] logObjects)
	{
		if (LogIt(LogLevels.Error))
		{
			UnityDebug.LogError(ClassAndMethodName() + LogStringFormatted(format, logObjects));
		}
	}

	private static string ClassAndMethodName()
	{
		var trace = new StackTrace();
		StackFrame sf = null;
		if (trace.FrameCount > 2)
		{
			sf = trace.GetFrame(2);
		}
		else if (trace.FrameCount > 1)
		{
			sf = trace.GetFrame(1);
		}
		
		if (sf != null)
		{
			return sf.GetMethod().DeclaringType.Name + "." + sf.GetMethod().Name + ": ";
		}
		else
		{
			return "";
		}
	}

	private static string LogStringFormatted(string format, object[] logObjects)
	{
		return string.Format(format, logObjects.Select(logObj => ObjectToString(logObj)).ToArray());
	}

	private static string LogString(object[] logObjects)
	{

		string log;
		if (logObjects.Length > 0)
		{
			StringBuilder sb = new StringBuilder ();
			foreach(object obj in logObjects)
			{
				sb.Append(ObjectToString(obj) + " | ");
			}
			sb.Remove(sb.Length - 3, 3);
			log = sb.ToString();
		}
		else
		{
			log = NOTHING;
		}
		
		return log;
	}

	public static string EnumerableToString(IEnumerable list)
	{
		if (!list.IsNullOrEmptyEnumerable())
		{
			StringBuilder sb = new StringBuilder ();
			foreach (object obj in list)
			{
				sb.Append(ObjectToString(obj) + ", ");
			}
			sb.Remove(sb.Length - 2, 2);
			return sb.ToString();
		}
		else
		{
			return list == null ? NULL : EMPTY;
		}
	}

	public static string DictToString(IDictionary dict)
	{
		if (!dict.IsNullOrEmptyCollection())
		{
			StringBuilder sb = new StringBuilder ();
			foreach (object key in dict.Keys)
			{
				sb.AppendFormat("{0}: {1}, ", ObjectToString(key), ObjectToString(dict[key]));
			}
			sb.Remove(sb.Length - 2, 2);
			return sb.ToString();
		}
		return dict == null ? NULL : EMPTY;
	}

	private static string ObjectToString(object obj)
	{
		if (obj == null)
		{
			return NULL;
		}

		System.Type type = obj.GetType();

		if (typeof(IDictionary).IsAssignableFrom(type))
		{
			return "[ " + DictToString(obj as IDictionary) + " ]";
		}
		if (typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string) && type != typeof(Transform))
		{
			return "[ " + EnumerableToString(obj as IEnumerable) + " ]";
		}
		return obj.ToString();
	}

	private static readonly int MAIN_THREAD_ID = Thread.CurrentThread.ManagedThreadId;
	private static bool? _isDebugBuild;
	public static bool IsDebugBuild 
	{
		get { 
            if (_isDebugBuild.HasValue == false)
            {
				if (Thread.CurrentThread.ManagedThreadId == MAIN_THREAD_ID)
				{
					_isDebugBuild = UnityDebug.isDebugBuild;
				}
				else
				{
					return true;
				}
            }
            return _isDebugBuild.Value;	
        }
		set { _isDebugBuild = value; }
	}

	public static LogLevels LogLevelInDebugBuild = LogLevels.Info;

	public static LogLevels LogLevel = LogLevels.Error;

	private static bool LogIt(LogLevels logLevel) 
	{
		if (IsDebugBuild)
		{
			return LogLevelInDebugBuild <= logLevel;
		}
		return LogLevel <= logLevel;
	}

	public enum LogLevels
	{
		None = -1,
		Trace = 0,
		Info = 1,
		Warning = 2,
		Error = 3
	}
}

public static class IEnumerableExtensions
{
	public static bool IsNullOrEmptyCollection (this ICollection collection)
	{
		return collection == null || collection.Count == 0;
	}

	public static bool IsNullOrEmptyEnumerable (this IEnumerable enumerable)
	{
		return enumerable == null || !enumerable.GetEnumerator().MoveNext();
	}
}

