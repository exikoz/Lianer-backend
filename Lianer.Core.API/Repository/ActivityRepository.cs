using System.Collections.Frozen;
using Lianer.Core.API.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;



public class ActivityRepository(AppDbContext context) : ACrud<Activity>(context),  IActivityRepository
{
    protected readonly AppDbContext _c = context;




 
}
