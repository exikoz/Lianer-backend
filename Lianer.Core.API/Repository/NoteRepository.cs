using Lianer.Core.API.Data;

public class NoteRepository(AppDbContext context) : ACrud<Note>(context), INoteRepository
{}