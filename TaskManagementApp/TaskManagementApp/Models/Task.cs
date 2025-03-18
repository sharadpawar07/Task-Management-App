using System;
using System.Collections.Generic;

namespace TaskManagementApp.Models;

public partial class Task
{
    public int TaskId { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public int AssignedUserId { get; set; }

    public DateTime? DueDate { get; set; }

    public string Status { get; set; } = null!;

    public int CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual User? AssignedUser { get; set; }

    public virtual User CreatedByNavigation { get; set; } = null!;
}
