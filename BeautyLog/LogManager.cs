using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
namespace BeautyLog
{
    /// <summary>
    /// 日志管理器
    /// <para>周期性写入文件</para>
    /// <para>周期LOG_PERIOD、写入路径LOG_PATH和分文件大小限制LOG_UNIT_FILE_MAX_SIZE可以直接调用静态修改（程序启动时未第一次调用就应修改完成）</para>
    /// </summary>
    internal static class LogManager
    {
        /// <summary>
        /// 文件信息
        /// </summary>
        private static FileInfo? fileInfo = null;
        /// <summary>
        /// 日志ID
        /// </summary>
        private static readonly string logId;
        /// <summary>
        /// 日志索引，如果单个时间内日志太大则分开
        /// </summary>
        private static int logIndex = 0;
        /// <summary>
        /// 进程名称
        /// </summary>
        private static readonly string proccessName = "";
        /// <summary>
        /// 锁
        /// </summary>
        private static readonly object m_lock = new object();
        /// <summary>
        /// 构造函数
        /// </summary>
        static LogManager()
        {
            proccessName = Process.GetCurrentProcess().ProcessName.ToLower();
            logId = Randomizer.Random.Next(100, 999).ToString();
            SystemInfo();
        }

        /// <summary>
        /// 打印系统环境信息日志
        /// </summary>
        private static void SystemInfo()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("process:");
            sb.Append(LogConfig.ProcessName);
            sb.Append("; version:");
            sb.Append(LogConfig.ProcessVersion);
            sb.Append("; esf:");
            sb.Append(LogConfig.FrameVersion);
            sb.Append("; ");
            sb.Append(".net:");
            sb.Append(LogConfig.DotNetVersion);
            sb.Append("; path:");
            sb.Append(LogConfig.Path);
            sb.Append("; sys:");
            sb.Append(LogConfig.SystemVersion);
            sb.Append("; user:");
            sb.Append(LogConfig.UserName);
            sb.Append("; cores:");
            sb.Append(LogConfig.ProcessorCount);

            bool LOG_CONSOLE_STACK_TRACE_OUTPUT = LogConfig.LOG_CONSOLE_STACK_TRACE_OUTPUT;
            bool LOG_FILE_STACK_TRACE_OUTPUT = LogConfig.LOG_FILE_STACK_TRACE_OUTPUT;
            LogConfig.LOG_CONSOLE_STACK_TRACE_OUTPUT = false;
            LogConfig.LOG_FILE_STACK_TRACE_OUTPUT = false;
            Log.Info(sb);
            LogConfig.LOG_CONSOLE_STACK_TRACE_OUTPUT = LOG_CONSOLE_STACK_TRACE_OUTPUT;
            LogConfig.LOG_FILE_STACK_TRACE_OUTPUT = LOG_FILE_STACK_TRACE_OUTPUT;
        }

        /// <summary>
        /// 写入日志
        /// </summary>
        /// <param name="type">日志类型</param>
        /// <param name="log">日志数据</param>
        /// <param name="stackTrace">堆栈数据</param>
        internal static void WriteLine(LogType type, string log, StackTrace stackTrace)
        {
            LogInfo logInfo = new LogInfo();
            logInfo.time = DateTime.Now;
            logInfo.type = type;
            logInfo.data = log;

            StackFrame? frame = stackTrace.GetFrame(0);
            MethodBase? method = frame?.GetMethod();
            string fileName = frame?.GetFileName()?? "null";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                fileName = fileName.Split("/")[fileName.Split('/').Length - 1];
            }
            else
            {
                fileName = fileName.Split("\\")[fileName.Split('\\').Length - 1];
            }
            logInfo.stack = $"file: ({fileName}) -> class: ({method?.DeclaringType?.FullName}) -> func: ({method?.Name}) -> line: ({frame?.GetFileLineNumber().ToString()})";

            if (!LogConfig.LOG_CONSOLE_ASYNC_OUTPUT)
            {
                FormatLog(ref logInfo);
                lock (m_lock) OutputLog(ref logInfo);
            }

            // 压入队列
            Update(logInfo);
        }

        /// <summary>
        /// 写入日志
        /// </summary>
        /// <param name="type">日志类型</param>
        /// <param name="log">日志数据</param>
        internal static void WriteLine(LogType type, string log)
        {
            LogInfo logInfo = new LogInfo();
            logInfo.time = DateTime.Now;
            logInfo.type = type;
            logInfo.data = log;

            if (LogConfig.LOG_CONSOLE_STACK_TRACE_OUTPUT || LogConfig.LOG_FILE_STACK_TRACE_OUTPUT)
            {
                var frame = new StackTrace().GetFrame(2);
                var method = frame?.GetMethod();

                if (method != null && method.DeclaringType != null)
                    logInfo.stack = $"class: ({method?.DeclaringType?.FullName}) -> func: ({method?.Name})";
            }

            if (!LogConfig.LOG_CONSOLE_ASYNC_OUTPUT)
            {
                FormatLog(ref logInfo);
                lock (m_lock) OutputLog(ref logInfo);
            }

            // 压入队列
            Update(logInfo);
        }

        /// <summary>
        /// 格式化日志
        /// </summary>
        /// <param name="log"></param>
        private static void FormatLog(ref LogInfo log)
        {
            string logType = "";
            switch (log.type)
            {
                case LogType.DEBUG:
                    logType = "DEBUG";
                    break;
                case LogType.INFO:
                    logType = "INFO";
                    break;
                case LogType.WARN:
                    logType = "WARN";
                    break;
                case LogType.ERROR:
                    logType = "ERROR";
                    break;
                case LogType.FATAL:
                    logType = "FATAL";
                    break;
                case LogType.INPUT:
                    logType = "INPUT";
                    break;
            }
            log.log = $"[{log.time:yyyy/MM/dd HH:mm:ss.fff}] [{logType.PadRight(5, ' ')}] [{log.data}]";
        }

        /// <summary>
        /// 输出日志
        /// </summary>
        /// <param name="log"></param>
        private static void OutputLog(ref LogInfo log)
        {
            if (log.type < LogConfig.CONSOLE_OUTPUT_LOG_TYPE)
                return;

            switch (log.type)
            {
                case LogType.DEBUG:
                    Console.ForegroundColor = LogConfig.FOREGROUND_DEBUG_COLOR;
                    if (LogConfig.BACKGROUND_DEBUG_COLOR != null) Console.BackgroundColor = (ConsoleColor)LogConfig.BACKGROUND_DEBUG_COLOR;
                    break;

                case LogType.INFO:
                    Console.ForegroundColor = LogConfig.FOREGROUND_INFO_COLOR;
                    if (LogConfig.BACKGROUND_INFO_COLOR != null) Console.BackgroundColor = (ConsoleColor)LogConfig.BACKGROUND_INFO_COLOR;
                    break;

                case LogType.WARN:
                    Console.ForegroundColor = LogConfig.FOREGROUND_WARN_COLOR;
                    if (LogConfig.BACKGROUND_WARN_COLOR != null) Console.BackgroundColor = (ConsoleColor)LogConfig.BACKGROUND_WARN_COLOR;
                    break;

                case LogType.ERROR:
                    Console.ForegroundColor = LogConfig.FOREGROUND_ERROR_COLOR;
                    if (LogConfig.BACKGROUND_ERROR_COLOR != null) Console.BackgroundColor = (ConsoleColor)LogConfig.BACKGROUND_ERROR_COLOR;
                    break;

                case LogType.FATAL:
                    Console.ForegroundColor = LogConfig.FOREGROUND_EXCEPTION_COLOR;
                    if (LogConfig.BACKGROUND_EXCEPTION_COLOR != null) Console.BackgroundColor = (ConsoleColor)LogConfig.BACKGROUND_EXCEPTION_COLOR;
                    break;

                case LogType.INPUT:
                    Console.ForegroundColor = LogConfig.FOREGROUND_INPUT_COLOR;
                    if (LogConfig.BACKGROUND_INPUT_COLOR != null) Console.BackgroundColor = (ConsoleColor)LogConfig.BACKGROUND_INPUT_COLOR;
                    break;
            }
            if(LogConfig.LOG_CONSOLE_MAIN_OUTPUT)
            {
                Console.WriteLine($"{log.log}{(LogConfig.LOG_CONSOLE_STACK_TRACE_OUTPUT && !string.IsNullOrEmpty(log.stack) ? $" ---> [{log.stack}]" : " ")}");
                Console.ResetColor();
            }
        }


        /// <summary>
        /// 日志信息数据
        /// </summary>
        private struct LogInfo
        {
            /// <summary>
            /// 日志类型
            /// </summary>
            internal LogType type;
            /// <summary>
            /// 日志时间
            /// </summary>
            internal DateTime time;
            /// <summary>
            /// 日志内容
            /// </summary>
            internal string data;
            /// <summary>
            /// 堆栈信息
            /// </summary>
            internal string stack;
            /// <summary>
            /// 日志字符串
            /// </summary>
            internal string log;
        }

        private static void Update(LogInfo log)
        {
            // 创建目录
            if (!Directory.Exists(LogConfig.LOG_PATH))
            {
                Directory.CreateDirectory(LogConfig.LOG_PATH);
            }

            string dateStr = DateTime.Now.ToString("yyyy_MM_dd/");
            // 创建当日目录
            if (!Directory.Exists(LogConfig.LOG_PATH + dateStr))
            {
                Directory.CreateDirectory(LogConfig.LOG_PATH + dateStr);
            }

            string filename = string.Format(DateTime.Now.ToString("{4}yyyy_MM_dd/{2}_HH_{0}_{1}{3}"), logIndex, logId, proccessName, LogConfig.LOG_FILE_SUFFIX, LogConfig.LOG_PATH);

            if (!File.Exists(filename))
                fileInfo = null;

            // 检查文件
            if (fileInfo == null)
                fileInfo = new FileInfo(filename);
            else
                fileInfo.Refresh();

            if (fileInfo.Exists)
            {
                if (fileInfo.Length > LogConfig.LOG_UNIT_FILE_MAX_SIZE)
                {
                    fileInfo = new FileInfo(string.Format(DateTime.Now.ToString("{4}yyyy_MM_dd/{2}_HH_{0}_{1}{3}"), ++logIndex, logId, proccessName, LogConfig.LOG_FILE_SUFFIX, LogConfig.LOG_PATH));
                    FileStream fs = fileInfo.Create();
                    fs.Close();
                    fileInfo.Refresh();
                }
            }
            else
            {
                FileStream fs = fileInfo.Create();
                fs.Close();
                fileInfo.Refresh();
            }

            using (StreamWriter sw = fileInfo.AppendText())
            {
                if (LogConfig.LOG_CONSOLE_ASYNC_OUTPUT)
                {
                    FormatLog(ref log);
                    OutputLog(ref log);
                }

                if (log.type >= LogConfig.FILE_OUTPUT_LOG_TYPE)
                {
                    sw.WriteLine($"{log.log}{(LogConfig.LOG_FILE_STACK_TRACE_OUTPUT && !string.IsNullOrEmpty(log.stack) ? $" ---> [{log.stack}]" : " ")}");
                }
            }
        }
    }

    /// <summary>
    /// 随机器
    /// <para>用于生成指定长度的符号代码或者获取一个共享的随机器</para>
    /// <para>全局共享一个随机种子的随机器</para>
    /// </summary>
    internal static class Randomizer
    {
        /// <summary>
        /// 随机器
        /// </summary>
        private static Random _rand = new Random();

        /// <summary>
        /// 随机器
        /// </summary>
        /// <returns></returns>
        public static Random Random => _rand;

        /// <summary>
        /// 重置随机器
        /// </summary>
        /// <param name="seed">随机种子</param>
        public static void Reset(int seed)
        {
            _rand = new Random(seed);
        }


        /// <summary>
        /// 随机字母类型
        /// </summary>
        public enum RandomCodeType
        {
            /// <summary>
            /// 大小写字母和数字和符号
            /// </summary>
            HighLowLetterAndNumberAndSymbol,
            /// <summary>
            /// 大小写字母和数字
            /// </summary>
            HighLowLetterAndNumber,
            /// <summary>
            /// 大写字母和数字
            /// </summary>
            HighLetterAndNumber,
            /// <summary>
            /// 大写字母
            /// </summary>
            HighLetter,
            /// <summary>
            /// 数字
            /// </summary>
            Number,
        }

        /// <summary>
        /// 符号库 大写小写字母数字特殊符号
        /// </summary>
        private readonly static char[] symbols = {
            '0','1','2','3','4','5','6','7','8','9',
            'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z',
            'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z',
            '!','@','#','$','%','^','&','*'
        };

        /// <summary>
        /// 生成大小写字母和数字组合的字符串
        /// <para>默认随机为大小写和数字</para>
        /// </summary>
        /// <param name="len">生成长度</param>
        /// <param name="type">随机代码类型</param>
        /// <returns>生成的字符串</returns>
        public static string Generate(int len, RandomCodeType type = RandomCodeType.HighLowLetterAndNumber)
        {
            StringBuilder? newRandom = null;

            switch (type)
            {
                case RandomCodeType.HighLowLetterAndNumberAndSymbol:
                    newRandom = new StringBuilder(70);
                    for (int i = 0, arrlen = 70; i < len; i++)
                    {
                        newRandom.Append(symbols[_rand.Next(arrlen)]);
                    }
                    break;
                case RandomCodeType.HighLowLetterAndNumber:
                    newRandom = new StringBuilder(62);
                    for (int i = 0, arrlen = 62; i < len; i++)
                    {
                        newRandom.Append(symbols[_rand.Next(arrlen)]);
                    }
                    break;
                case RandomCodeType.HighLetterAndNumber:
                    newRandom = new StringBuilder(36);
                    for (int i = 0, arrlen = 36; i < len; i++)
                    {
                        newRandom.Append(symbols[_rand.Next(arrlen)]);
                    }
                    break;
                case RandomCodeType.HighLetter:
                    newRandom = new StringBuilder(26);
                    for (int i = 0, arrlen = 26; i < len; i++)
                    {
                        newRandom.Append(symbols[_rand.Next(arrlen) + 10]);
                    }
                    break;
                case RandomCodeType.Number:
                    newRandom = new StringBuilder(10);
                    for (int i = 0, arrlen = 10; i < len; i++)
                    {
                        newRandom.Append(symbols[_rand.Next(arrlen)]);
                    }
                    break;
            }
            return newRandom?.ToString() ?? "";
        }

        /// <summary>
        /// 生成唯一Guid
        /// <para>默认无横线 格式为32个字符</para>
        /// </summary>
        /// <param name="hasLine">是否需要分段横线 默认无横线</param>
        /// <returns></returns>
        public static string GenerateGuid(bool hasLine = false)
        {
            if (hasLine)
                return Guid.NewGuid().ToString();
            else
                return Guid.NewGuid().ToString("N");
        }
    }
}

