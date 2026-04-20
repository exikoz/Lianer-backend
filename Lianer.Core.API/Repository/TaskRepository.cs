using Lianer.Core.API.Data;

public class TaskRepository(AppDbContext context) : ACrud<Task>(context), ITaskRepository
{}
