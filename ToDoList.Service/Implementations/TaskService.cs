using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ToDoList.DAL.Interfaces;
using ToDoList.Domain.Entity;
using ToDoList.Domain.Enum;
using ToDoList.Domain.Extensions;
using ToDoList.Domain.Response;
using ToDoList.Domain.ViewModels.Task;
using ToDoList.Service.Interfaces;

namespace ToDoList.Service.Implementations;

public class TaskService : ITaskService
{
    private readonly IBaseRepository<TaskEntity> _taskRepository;
    private ILogger<TaskEntity> _logger;

    public TaskService(IBaseRepository<TaskEntity> taskRepository, ILogger<TaskEntity> logger)
    {
        _taskRepository = taskRepository;
        _logger = logger;
    }

    public async Task<IBaseResponse<TaskEntity>> Create(CreateTaskViewModel model)
    {
        try
        {
            model.Validate();

            _logger.LogInformation($"Запрос на создании задачи - {model.Name}");

             var task = await _taskRepository.GetAll().Where(x => x.Created.Date == DateTime.Today)
                 .FirstOrDefaultAsync(x => x.Name == model.Name);

             if (task != null)
             {
                 return new BaseResponse<TaskEntity>()
                 {
                     Description = "Задача с таким названием уже есть",
                     StatusCode = StatusCode.TaskIsHasAlready
                 };
             }

             task = new TaskEntity()
             {
                 Name = model.Name,
                 Description = model.Description,
                 IsDone = false,
                 Priority = model.Priority,
                 Created = DateTime.Now,
             };

             await _taskRepository.Create(task);
             
             _logger.LogInformation($"Задача создана: {task.Name} {task.Created}");
             return new BaseResponse<TaskEntity>()
             {
                 StatusCode = StatusCode.Ok,
                 Description = "Задача создана"
             };
        }
        catch (Exception e)
        {
            _logger.LogError(e,$"[TaskService.Create]: {e.Message}" );
            return new BaseResponse<TaskEntity>()
            {
                Description = $"{e.Message}",
                StatusCode = StatusCode.InternalServerError
            };
        }
    }

    public async Task<IBaseResponse<IEnumerable<TaskViewModel>>> GetTask()
    {
        try
        {
            var tasks = await _taskRepository.GetAll()
                .Select(x => new TaskViewModel()
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    Isdone = x.IsDone == true ? "Готов" : "Не готов",
                    Priority = x.Priority.GetDisplayName(),
                    Created = x.Created.ToLongDateString()
                })
                .ToListAsync();

            return new BaseResponse<IEnumerable<TaskViewModel>>()
            {
                Data = tasks,
                StatusCode = StatusCode.Ok
            };
        }
        catch (Exception e)
        {
            _logger.LogError(e,$"[TaskService.Create]: {e.Message}" );
            return new BaseResponse<IEnumerable<TaskViewModel>>()
            {
                Description = $"{e.Message}",
                StatusCode = StatusCode.InternalServerError
            };
        }
    }
}