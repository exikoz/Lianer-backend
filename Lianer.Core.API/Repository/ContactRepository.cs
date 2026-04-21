using Lianer.Core.API.Data;
using Lianer.Core.API.Models;

public class ContactRepository(AppDbContext context) : ACrud<Contact>(context), IContactRepository
{}

