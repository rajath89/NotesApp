namespace Domain.Entities;
using Microsoft.AspNetCore.Identity;

public class ApplicationUser : IdentityUser
{
    public ICollection<Workspace> Workspaces { get; set; } = new List<Workspace>();
}