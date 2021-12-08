using System;
using System.IO;
using System.Xml;

namespace SA.LA
{
    public class ExceptionHandling
    {
        IExceptionHanlerRepository<ExceptionLogDetails> exceptionHanlerRepository;
        public ExceptionHandling(IExceptionHanlerRepository<ExceptionLogDetails> _exceptionHanlerRepository)
        {
            exceptionHanlerRepository = _exceptionHanlerRepository;
        }

        public void ExceptionHandler(ExceptionHandlingEntity exceptionEntity)
        {
            try
            {
                if (exceptionEntity != null && exceptionEntity.Message != null)
                {
                    StringWriter sw = new StringWriter();
                    using (XmlWriter writer = XmlWriter.Create(sw))
                    {
                        if (exceptionEntity == null) return;
                        writer.WriteStartElement("Exception");
                        //writer.WriteStartElement("Title", exceptionEntity.Title);
                        //writer.WriteStartElement("TimeStamp", DateTime.Now.ToString());
                        //writer.WriteStartElement("AppDomainName", AppDomain.CurrentDomain.FriendlyName);
                        //writer.WriteStartElement("Severity", Convert.ToString(TraceEventType.Error));
                        writer.WriteElementString("Message", exceptionEntity.Message);
                        writer.WriteElementString("Source", exceptionEntity.Source);
                        WriteException(writer, "InnerException", exceptionEntity.Exception.InnerException);
                        writer.WriteElementString("StackTrace", exceptionEntity.Exception.ToString());
                        writer.WriteEndElement();
                    }
                    exceptionHanlerRepository.WriteExceptionLogs(sw.ToString());
                }
            }
            catch (Exception ex)
            {
                //WriteTraceLog(ex);
            }
        }

        public static string WriteXmlException( Exception exception)
        {
            if (exception == null) throw new ArgumentNullException("exception");
            StringWriter sw = new StringWriter();
            using (XmlWriter xw = XmlWriter.Create(sw))
            {
                WriteException(xw, "exception", exception);
            }
            return sw.ToString();
        }

        static void WriteException(XmlWriter writer, string name, Exception exception)
        {
            if (exception == null) return;
            writer.WriteStartElement(name);
            writer.WriteElementString("message", exception.Message);
            writer.WriteElementString("source", exception.Source);
            WriteException(writer, "innerException", exception.InnerException);
            writer.WriteEndElement();
        }

    }
}
