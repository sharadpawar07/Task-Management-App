using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TaskManagementApp.Models;

public partial class User
{
    public int UserId { get; set; }

    [Required(ErrorMessage = "Full Name is required.")]
    public string FullName { get; set; } = null!;


    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; } = null!;


    [Required(ErrorMessage = "Password is required.")]
    public string UserPassword { get; set; } = null!;

    public int RoleId { get; set; }

    public virtual Role? Role { get; set; }

    public virtual ICollection<Task> TaskAssignedUsers { get; set; } = new List<Task>();

    public virtual ICollection<Task> TaskCreatedByNavigations { get; set; } = new List<Task>();
}