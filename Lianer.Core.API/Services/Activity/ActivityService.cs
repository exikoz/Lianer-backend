using System.Collections.ObjectModel;

public interface IActivityService{}
public class ActivityService(IActivityService service)
{
    private readonly IActivityService _service = service;

}