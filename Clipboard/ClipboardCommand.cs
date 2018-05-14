using System;
using System.Management.Automation;

namespace PowerShell.Clipboard
{
    public abstract class ClipboardCommand : PSCmdlet
    {
        /// <summary>
        /// Writes an exception to the error stream.
        /// </summary>
        /// <param name="exception">The exception to write.</param>
        /// <param name="errorId">A string id identifying the error source.</param>
        /// <param name="errorCategory">An <see cref="ErrorCategory"/> categorizing the error.</param>
        protected void WriteError(Exception exception, string errorId, ErrorCategory errorCategory)
        {
            ErrorRecord error = new ErrorRecord(exception, errorId, errorCategory, null);
            base.WriteError(error);
        }
    }
}
