using System;
using System.Collections.Generic;

namespace DAL_Group4_E_Commerce.Models;

public partial class Feedback
{
    public string FeedbackId { get; set; } = null!;

    public int TopicId { get; set; }

    public string Content { get; set; } = null!;

    public DateOnly? FeedbackDate { get; set; }

    public string? FullName { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public bool? NeedsReply { get; set; }

    public string? ReplyContent { get; set; }

    public DateOnly? ReplyDate { get; set; }

    public virtual FeedbackTopic Topic { get; set; } = null!;
}
