using System;
using System.Collections.Generic;
using System.Text;

namespace SorteFederation.Shared.Models;


public class MailData
{
    public string To { get; }
    public string From { get; }
    public string DisplayName { get; }
    public string ReplyTo { get; }
    public string ReplyToName { get; }
    public string Subject { get; }
    public string Body { get; }
    public string Attached { get; }
    public MailData(
        string to,
        string subject,
        string body = null,
        string from = null,
        string displayName = null,
        string replyTo = null,
        string replyToName = null,
        string attached = null
        )
    {
        To = to;
        From = from;
        DisplayName = displayName;
        ReplyTo = replyTo;
        ReplyToName = replyToName;
        Subject = subject;
        Body = body;
        Attached = attached;
    }
}
