﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using UnityEngine;



namespace GameLog
{
    public enum LoggerType 
    {
        Unity,
        Console,
    }

    public enum LogColor
    {
        None,
        Red,
        Green,
        Blue,
        Cyan,
        Magenta,
        Yellow
    }

    public class LogConfig
    {
        /// <summary>
        /// 是否启用
        /// </summary>
        public bool enableLog = true;
        /// <summary>
        /// 日志前缀
        /// </summary>
        public string logPrefix = "#";
        /// <summary>
        /// 时间标记 显示毫秒
        /// </summary>
        public bool enableTime = true;
        /// <summary>
        /// 间隔符号
        /// </summary>
        public string logSeparate = ">>";
        /// <summary>
        /// 线程ID
        /// </summary>
        public bool enableThreadID = true;
        /// <summary>
        /// 堆栈信息
        /// </summary>
        public bool enableTrace = true;
        /// <summary>
        /// 文件保存
        /// </summary>
        public bool enableSave = true;
        /// <summary>
        /// 日志覆盖
        /// </summary>
        public bool enableCover = true;
        private string _savePath;
        /// <summary>
        /// 日志文件名称
        /// </summary>
        public string saveName = "GameLog.txt";
        /// <summary>
        /// 日志输出器类型
        /// </summary>
        public LoggerType loggerEnum = LoggerType.Unity;
        public string savePath
        {
            get
            {
                if (_savePath == null)
                {
                    if (loggerEnum == LoggerType.Unity)
                    {
                        _savePath = Application.persistentDataPath + "/GameLog/";
                    }
                    else
                    {
                        _savePath = string.Format("{0}Logs\\", AppDomain.CurrentDomain.BaseDirectory);
                    }
                }
                return _savePath;
            }
            set
            {
                _savePath = value;
            }
        }
    }


    interface ILogger
    {
        void Log(string msg, LogColor logColor = LogColor.None);
        void Warn(string msg);
        void Error(string msg);
    }

    public class LogUtils
    {
        class UnityLogger : ILogger
        {
            public void Error(string msg)
            {
                msg = ColorUnityLog(msg, LogColor.Red);
                UnityEngine.Debug.LogError(msg);
            }


            public void Log(string msg, LogColor color)
            {
                if (color != LogColor.None)
                {
                    msg = ColorUnityLog(msg, color);
                }
                UnityEngine.Debug.Log(msg);
            }



            public void Warn(string msg)
            {
                msg = ColorUnityLog(msg, LogColor.Yellow);
                UnityEngine.Debug.LogWarning(msg);
            }

            private string ColorUnityLog(string msg, LogColor color)
            {
                switch (color)
                {
                    case LogColor.Red:
                        msg = string.Format("<color=#FF0000>{0}</color>", msg);
                        break;
                    case LogColor.Green:
                        msg = string.Format("<color=#00FF00>{0}</color>", msg);
                        break;
                    case LogColor.Blue:
                        msg = string.Format("<color=#0000FF>{0}</color>", msg);
                        break;
                    case LogColor.Cyan:
                        msg = string.Format("<color=#00FFFF>{0}</color>", msg);
                        break;
                    case LogColor.Magenta:
                        msg = string.Format("<color=#FF00FF>{0}</color>", msg);
                        break;
                    case LogColor.Yellow:
                        msg = string.Format("<color=#FFFF00>{0}</color>", msg);
                        break;
                    default:
                        break;
                }

                return msg;
            }

        }
        class ConsoleLogger : ILogger
        {
            public void Log(string msg, LogColor color = LogColor.None)
            {
                WriteConsoleLog(msg, color);
            }
            public void Warn(string msg)
            {
                WriteConsoleLog(msg, LogColor.Yellow);
            }
            public void Error(string msg)
            {
                WriteConsoleLog(msg, LogColor.Red);
            }
            private void WriteConsoleLog(string msg, LogColor color)
            {
                switch (color)
                {
                    case LogColor.Red:
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine(msg);
                        Console.ForegroundColor = ConsoleColor.Gray;
                        break;
                    case LogColor.Green:
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine(msg);
                        Console.ForegroundColor = ConsoleColor.Gray;
                        break;
                    case LogColor.Blue:
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(msg);
                        Console.ForegroundColor = ConsoleColor.Gray;
                        break;
                    case LogColor.Cyan:
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine(msg);
                        Console.ForegroundColor = ConsoleColor.Gray;
                        break;
                    case LogColor.Magenta:
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.WriteLine(msg);
                        Console.ForegroundColor = ConsoleColor.Gray;
                        break;
                    case LogColor.Yellow:
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.WriteLine(msg);
                        Console.ForegroundColor = ConsoleColor.Gray;
                        break;
                    case LogColor.None:
                    default:
                        Console.WriteLine(msg);
                        break;
                }
            }
        }
        private static ILogger logger;
        public static LogConfig cfg;
        private static StreamWriter LogFileWriter = null;
        private const string logLock = "GameLogLock";

        public static void InitSettings(LogConfig cfg = null)
        {
            if (cfg == null)
            {
                cfg = new LogConfig();
            }
            LogUtils.cfg = cfg;

            if (cfg.loggerEnum == LoggerType.Console)
            {
                logger = new ConsoleLogger();
            }
            else
            {
                logger = new UnityLogger();
            }

            if (cfg.enableSave == false)
            {
                return;
            }
            if (cfg.enableCover)
            {
                string path = cfg.savePath + cfg.saveName;
                try
                {
                    if (Directory.Exists(cfg.savePath))
                    {
                        if (File.Exists(path))
                        {
                            File.Delete(path);
                        }
                    }
                    else
                    {
                        Directory.CreateDirectory(cfg.savePath);
                    }
                    LogFileWriter = File.AppendText(path);
                    LogFileWriter.AutoFlush = true;
                }
                catch (Exception e)
                {
                    LogFileWriter = null;
                }
            }
            else
            {
                string prefix = DateTime.Now.ToString("yyyyMMdd@HH-mm-ss");
                string path = cfg.savePath + prefix + cfg.saveName;
                try
                {
                    if (Directory.Exists(cfg.savePath) == false)
                    {
                        Directory.CreateDirectory(cfg.savePath);
                    }
                    LogFileWriter = File.AppendText(path);
                    LogFileWriter.AutoFlush = true;
                }
                catch (Exception e)
                {
                    LogFileWriter = null;
                }
            }
        }

        public static void Log(string msg, params object[] args)
        {
            if (cfg.enableLog == false)
            {
                return;
            }
            msg = DecorateLog(string.Format(msg, args));
            lock (logLock)
            {
                logger.Log(msg);
                if (cfg.enableSave)
                {
                    WriteToFile(string.Format("[L]{0}", msg));
                }
            }
        }

        /// <summary>
        /// 支持自定义颜色的日志
        /// </summary>
        public static void ColorLog(LogColor color, string msg, params object[] args)
        {
            if (cfg.enableLog == false)
            {
                return;
            }
            msg = DecorateLog(string.Format(msg, args),cfg.enableTrace);
            lock (logLock)
            {
                logger.Log(msg, color);
                if (cfg.enableSave)
                {
                    WriteToFile(string.Format("[L]{0}", msg));
                }
            }
        }

        /// <summary>
        /// 支持Format的堆栈日志
        /// </summary>:
        public static void Trace(string msg, params object[] args)
        {
            if (cfg.enableLog == false)
            {
                return;
            }
            msg = DecorateLog(string.Format(msg, args), cfg.enableTrace);
            lock (logLock)
            {
                logger.Log(msg, LogColor.Magenta);
                if (cfg.enableSave)
                {
                    WriteToFile(string.Format("[T]{0}", msg));
                }
            }
        }

        /// <summary>
        /// 警告日志（黄色）
        /// </summary>
        public static void Warn(string msg, params object[] args)
        {
            if (cfg.enableLog == false)
            {
                return;
            }
            msg = DecorateLog(string.Format(msg, args), cfg.enableTrace);
            lock (logLock)
            {
                logger.Warn(msg);
                if (cfg.enableSave)
                {
                    WriteToFile(string.Format("[W]{0}", msg));
                }
            }
        }

        // <summary>
        /// 错误日志（红色，输出堆栈）
        /// </summary>
        public static void Error(string msg, params object[] args)
        {
            if (cfg.enableLog == false)
            {
                return;
            }
            msg = DecorateLog(string.Format(msg, args), cfg.enableTrace);
            lock (logLock)
            {
                logger.Error(msg);
                if (cfg.enableSave)
                {
                    WriteToFile(string.Format("[E]{0}", msg));
                }
            }
        }


        //Tool
        private static string DecorateLog(string msg, bool isTrace = false)
        {
            StringBuilder sb = new StringBuilder(cfg.logPrefix, 100);
            if (cfg.enableTime)
            {
                sb.AppendFormat(" {0}", DateTime.Now.ToString("hh:mm:ss-fff"));
            }

            if (cfg.enableThreadID)
            {
                sb.AppendFormat(" {0}", GetThreadID());
            }

            sb.AppendFormat(" {0} {1}", cfg.logSeparate, msg);

            if (isTrace)
            {
                sb.AppendFormat("\nStackTrace:{0}", GetLogTrace());
            }


            return sb.ToString();
        }

        private static string GetThreadID()
        {
            return string.Format(" ThreadID:{0}", Thread.CurrentThread.ManagedThreadId);
        }

        private static string GetLogTrace()
        {
            StringBuilder sb = new StringBuilder(20);
            StackTrace st = new StackTrace(3, true);//跳跃3帧
            for (int i = 0; i < st.FrameCount; i++)
            {
                StackFrame sf = st.GetFrame(i);
                sb.Append(string.Format("\n    {0}::{1} Line:{2}", sf.GetFileName(), sf.GetMethod(), sf.GetFileLineNumber()));
            }
            return sb.ToString();
        }

        //private static string GetLogTrace()
        //{
        //    StackTrace st = new StackTrace(3, true);//跳跃3帧
        //    string traceInfo = "";
        //    for (int i = 0; i < st.FrameCount; i++)
        //    {
        //        StackFrame sf = st.GetFrame(i);
        //        traceInfo += string.Format("\n    {0}::{1} Line:{2}", sf.GetFileName(), sf.GetMethod(), sf.GetFileLineNumber());
        //    }
        //    return traceInfo;
        //}

        private static void WriteToFile(string msg)
        {
            if (cfg.enableSave && LogFileWriter != null)
            {
                try
                {
                    LogFileWriter.WriteLine(msg);
                }
                catch (Exception)
                {
                    LogFileWriter = null;
                    return;
                }
            }
        }




    }
}

