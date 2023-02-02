namespace BeautyLog
{
    /// <summary>
    /// 日志配置
    /// <para>配置修改建议在第一次调用Log前修改完成，避免出现奇怪的问题</para>
    /// </summary>
    public static class LogConfig
    {
        /// <summary>
        /// 日志控制台异步输出开关
        /// <para>异步输出采用其它线程处理，调试时可能出现意外，建议调试采用同步输出</para>
        /// <para>默认关闭</para>
        /// </summary>
        public static bool LOG_CONSOLE_ASYNC_OUTPUT = false;
        /// <summary>
        /// 日志控制台总输出开关
        /// <para>是否打印日志，主要针对后台程序</para>
        /// <para>默认开启</para>
        /// </summary>
        public static bool LOG_CONSOLE_MAIN_OUTPUT = true;
        /// <summary>
        /// 日志控制台堆栈跟踪输出
        /// <para>开启后控制台中日志将写入堆栈信息</para>
        /// <para>默认关闭</para>
        /// </summary>
        public static bool LOG_CONSOLE_STACK_TRACE_OUTPUT = false;
        /// <summary>
        /// 日志文件堆栈跟踪输出
        /// <para>开启后日志文件中日志将写入堆栈信息</para>
        /// <para>默认开启</para>
        /// </summary>
        public static bool LOG_FILE_STACK_TRACE_OUTPUT = true;
        /// <summary>
        /// 控制台输出最小日志类型 默认：调试性日志
        /// </summary>
        public static LogType CONSOLE_OUTPUT_LOG_TYPE = LogType.DEBUG;
        /// <summary>
        /// 文件输出最小日志类型 默认：信息性日志
        /// </summary>
        public static LogType FILE_OUTPUT_LOG_TYPE = LogType.INFO;
        /// <summary>
        /// 日志写入周期 单位 ms
        /// </summary>
        public static int LOG_PERIOD = 1000;
        /// <summary>
        /// 日志写入文件后缀
        /// </summary>
        public static string LOG_FILE_SUFFIX = ".log";
        /// <summary>
        /// 日志单个文件最多大小
        /// 单位 byte 默认 50MB大小
        /// </summary>
        public static long LOG_UNIT_FILE_MAX_SIZE = 52428800;
        /// <summary>
        /// 日志根路径
        /// </summary>
        public static string LOG_PATH = Environment.CurrentDirectory + (System.OperatingSystem.IsWindows()? @"\logs\" : @"/logs/");
        /// <summary>
        /// 调试性 日志字体颜色
        /// </summary>
        public static System.ConsoleColor FOREGROUND_DEBUG_COLOR = System.ConsoleColor.DarkGray;
        /// <summary>
        /// 信息性 日志字体颜色
        /// </summary>
        public static System.ConsoleColor FOREGROUND_INFO_COLOR = System.ConsoleColor.Green;
        /// <summary>
        /// 警告性 日志字体颜色
        /// </summary>
        public static System.ConsoleColor FOREGROUND_WARN_COLOR = System.ConsoleColor.Cyan;
        /// <summary>
        /// 错误性 日志字体颜色
        /// </summary>
        public static System.ConsoleColor FOREGROUND_ERROR_COLOR = System.ConsoleColor.Red;
        /// <summary>
        /// 异常 日志字体颜色
        /// </summary>
        public static System.ConsoleColor FOREGROUND_EXCEPTION_COLOR = System.ConsoleColor.Yellow;
        /// <summary>
        /// 输入 日志字体颜色
        /// </summary>
        public static System.ConsoleColor FOREGROUND_INPUT_COLOR = System.ConsoleColor.Blue;
        /// <summary>
        /// 信息性 日志字体背景颜色
        /// </summary>
        public static System.ConsoleColor? BACKGROUND_INFO_COLOR = null;
        /// <summary>
        /// 调试性 日志字体背景颜色
        /// </summary>
        public static System.ConsoleColor? BACKGROUND_DEBUG_COLOR = null;
        /// <summary>
        /// 警告性 日志字体背景颜色
        /// </summary>
        public static System.ConsoleColor? BACKGROUND_WARN_COLOR = null;
        /// <summary>
        /// 错误性 日志字体背景颜色
        /// </summary>
        public static System.ConsoleColor? BACKGROUND_ERROR_COLOR = null;
        /// <summary>
        /// 异常 日志字体背景颜色
        /// </summary>
        public static System.ConsoleColor? BACKGROUND_EXCEPTION_COLOR = System.ConsoleColor.DarkRed;
        /// <summary>
        /// 输入 日志字体背景颜色
        /// </summary>
        public static System.ConsoleColor? BACKGROUND_INPUT_COLOR = null;
//----------------------------------------------------------------------------------
        /// <summary>
        /// 系统版本
        /// </summary>
        public static string SystemVersion { get; } = System.Environment.OSVersion.ToString();
        /// <summary>
        /// DotNet版本
        /// </summary>
        public static string DotNetVersion { get; } = System.Environment.Version.ToString();
        /// <summary>
        /// 用户名称
        /// </summary>
        public static string UserName { get; } = System.Environment.UserName;
        /// <summary>
        /// 当前执行路径
        /// </summary>
        public static string Path { get; } = System.Environment.CurrentDirectory;
        /// <summary>
        /// 进程名称
        /// </summary>
        public static string ProcessName { get; } = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name?.ToString() ?? "";
        /// <summary>
        /// 进程版本
        /// </summary>
        public static string ProcessVersion { get; } = System.Text.RegularExpressions.Regex.Match(System.Reflection.Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "", "[0-9]+\\.[0-9]+\\.[0-9]+").Value;
        /// <summary>
        /// 框架版本
        /// </summary>
        public static string FrameVersion { get; } = System.Text.RegularExpressions.Regex.Match(System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "", "[0-9]+\\.[0-9]+\\.[0-9]+").Value;
        /// <summary>
        /// 逻辑线程数
        /// </summary>
        public static int ProcessorCount { get; } = System.Environment.ProcessorCount;
    }
}