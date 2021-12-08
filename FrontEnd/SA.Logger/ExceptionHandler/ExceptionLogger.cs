using System;
using System.IO;
using System.Xml;

namespace SA.LA
{
    public class ExceptionLogger
    {
        private static readonly log4net.ILog logger
      = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public ExceptionLogger()
        {
        }

        public static void ExceptionHandler(ExceptionHandlingEntity exceptionEntity)
        {
            try
            {
                string errorLevel = ((log4net.Repository.Hierarchy.Logger)((log4net.Core.LoggerWrapperImpl)logger).Logger).EffectiveLevel.Name;

                if (errorLevel.ToUpper().Equals("DEBUG")) //Write log for Debug mode
                    WriteLogDebug(exceptionEntity);
                else if(errorLevel.ToUpper().Equals("ERROR"))
                    WriteLogError(exceptionEntity);
                else if (errorLevel.ToUpper().Equals("WARN"))
                    WriteLogWarn(exceptionEntity);
                else if (errorLevel.ToUpper().Equals("INFO"))
                    WriteLogInfo(exceptionEntity);
            }
            catch (Exception ex)
            {
                throw ex;
                //WriteTraceLog(ex);
            }
        }

        private static void WriteLogDebug(ExceptionHandlingEntity exceptionEntity)
        {
            if (exceptionEntity != null && exceptionEntity.Message != null)
                logger.Debug(exceptionEntity.Message);
            else if (exceptionEntity.Exception != null)
                logger.Debug(exceptionEntity.Message, exceptionEntity.Exception);
        }
        private static void WriteLogError(ExceptionHandlingEntity exceptionEntity)
        {
            if (exceptionEntity != null && exceptionEntity.Message != null)
                logger.Error(exceptionEntity.Message);
            else if (exceptionEntity.Exception != null)
                logger.Error(exceptionEntity.Message, exceptionEntity.Exception);
        }
        private static void WriteLogWarn(ExceptionHandlingEntity exceptionEntity)
        {
            if (exceptionEntity != null && exceptionEntity.Message != null)
                logger.Warn(exceptionEntity.Message);
            else if (exceptionEntity.Exception != null)
                logger.Warn(exceptionEntity.Message, exceptionEntity.Exception);
        }
        private static void WriteLogInfo(ExceptionHandlingEntity exceptionEntity)
        {
            if (exceptionEntity != null && exceptionEntity.Message != null)
                logger.Info(exceptionEntity.Message);
            else if (exceptionEntity.Exception != null)
                logger.Info(exceptionEntity.Message, exceptionEntity.Exception);
        }
    }
}
