using Serilog;
using System.Runtime.CompilerServices;

namespace DASIT.LoggingSupport
{

        public static class LoggerExtensions
        {
            public static ILogger AddCallerDetails(this ILogger logger,
                [CallerMemberName] string memberName = "",
                [CallerFilePath] string sourceFilePath = "",
                [CallerLineNumber] int sourceLineNumber = 0)
            {
                return logger
                    .ForContext("MemberName", memberName)
                    .ForContext("FilePath", sourceFilePath)
                    .ForContext("LineNumber", sourceLineNumber);
            }
        }

    
}
