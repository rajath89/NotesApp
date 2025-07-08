namespace Domain.Entities;
using Microsoft.AspNetCore.Identity;

public class ApplicationUser
{
    public string Id { get; set; }
    public string Email { get; set; }
    public ICollection<Workspace> Workspaces { get; set; }
}