namespace PowerShell.Clipboard
{
    /// <summary>
    /// Specifies the supported text formats used by the set and get clipboard cmdlets.
    /// </summary>
    public enum TextDataFormat
    {
        /// <summary>
        /// Specifies a comma-separated value (CSV) format, which is a common interchange format used by spreadsheets.
        /// </summary>
        CommaSeparatedValue,

        /// <summary>
        /// Specifies text consisting of HTML data.
        /// </summary>
        Html,

        /// <summary>
        /// Specifies text consisting of rich text format (RTF) data.
        /// </summary>
        Rtf,

        /// <summary>
        /// Specifies the standard ANSI text format.
        /// </summary>
        Text,

        /// <summary>
        /// Specifies the standard Windows Unicode text format.
        /// </summary>
        Unicode,

        /// <summary>
        /// Specifies an XML spreadsheet text format.
        /// </summary>
        XmlSpreadsheet,

        /// <summary>
        /// Specifies an HDROP filelist format.
        /// </summary>
        FileList
    }

}
