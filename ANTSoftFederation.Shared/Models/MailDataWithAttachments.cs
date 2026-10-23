using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace SorteFederation.Shared.Models;

public class MailDataWithAttachments
{
    public string To { get; set; }
    public List<string> Bcc { get; set; }
    public List<string> Cc { get; set; }
    public string From { get; set; }
    public string DisplayName { get; set; }
    public string ReplyTo { get; set; }
    public string ReplyToName { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public IFormFileCollection Attachments { get; set; }
}

public class ICalAttachment
{
    public string FilePath { get; set; }
    public string ContentType { get; set; }
    public string FileName { get; set; }

    public ICalAttachment(string filePath, string contentType, string fileName)
    {
        FilePath = filePath;
        ContentType = contentType;
        FileName = fileName;
    }
}

