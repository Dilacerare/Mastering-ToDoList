using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ToDoList.DAL.Interfaces;
using ToDoList.Domain.Entity;
using ToDoList.Domain.Enum;
using ToDoList.Domain.Extensions;
using ToDoList.Domain.Filters.Task;
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

    public async Task<IBaseResponse<IEnumerable<TaskViewModel>>> CalculateCompletedTasks()
    {
        try
        {
            var tasks = await _taskRepository.GetAll()
                .Where(x => x.Created.Date == DateTime.Today)
                .Select(x => new TaskViewModel()
                {
                    Id = x.Id,
                    Name = x.Name,
                    Isdone = x.IsDone == true ? "Готово" : "Не готово",
                    Description = x.Description,
                    Priority = x.Priority.ToString(),
                    Created = x.Created.ToString(CultureInfo.InvariantCulture)
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
            _logger.LogError(e,$"[TaskService.CalculateCompletedTasks]: {e.Message}" );
            return new BaseResponse<IEnumerable<TaskViewModel>>()
            {
                StatusCode = StatusCode.InternalServerError
            };
        }
    }

    public async Task<IBaseResponse<IEnumerable<TaskCompletedViewModel>>> GetCompletedTasks()
    {
        try
        {
            var tasks = await _taskRepository.GetAll()
                .Where(x => x.IsDone)
                .Where(x => x.Created.Date == DateTime.Today)
                .Select(x => new TaskCompletedViewModel()
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                })
                .ToListAsync();

            return new BaseResponse<IEnumerable<TaskCompletedViewModel>>()
            {
                Data = tasks,
                StatusCode = StatusCode.Ok
            };
        }
        catch (Exception e)
        {
            _logger.LogError(e,$"[TaskService.GetCompletedTasks]: {e.Message}" );
            return new BaseResponse<IEnumerable<TaskCompletedViewModel>>()
            {
                StatusCode = StatusCode.InternalServerError
            };
        }
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

    public async Task<IBaseResponse<bool>> EndTask(long id)
    {
        try
        {
            var task = await _taskRepository.GetAll().FirstOrDefaultAsync(x => x.Id == id);
            if (task == null)
            {
                return new BaseResponse<bool>()
                {
                    Description = "Задача не найдена",
                    StatusCode = StatusCode.TaskNotFound
                };
            }

            task.IsDone = true;

            await _taskRepository.Update(task);
            
            return new BaseResponse<bool>()
            {
                Description = "Задача завершена",
                StatusCode = StatusCode.Ok
            };
        }
        catch (Exception e)
        {
            _logger.LogError(e,$"[TaskService.EndTask]: {e.Message}" );
            return new BaseResponse<bool>()
            {
                Description = $"{e.Message}",
                StatusCode = StatusCode.InternalServerError
            };
        }
    }

    public async Task<DataTableResult> GetTask(TaskFilter filter)
    {
        try
        {
            var tasks = await _taskRepository.GetAll()
                .Where(x => !x.IsDone)
                .WhereIf(!string.IsNullOrWhiteSpace(filter.Name), x => x.Name == filter.Name)
                .WhereIf(filter.Priority.HasValue, x => x.Priority == filter.Priority)
                .Select(x => new TaskViewModel()
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    Isdone = x.IsDone == true ? "Готов" : "Не готов",
                    Priority = x.Priority.GetDisplayName(),
                    Created = x.Created.ToLongDateString()
                })
                .Skip(filter.Skip)
                .Take(filter.PageSize)
                .ToListAsync();

            var count = _taskRepository.GetAll().Count(x => !x.IsDone);
            
            return new DataTableResult()
            {
                Data = tasks,
                Total = count
            };
        }
        catch (Exception e)
        {
            _logger.LogError(e,$"[TaskService.Create]: {e.Message}" );
            return new DataTableResult()
            {
                Data = null,
                Total = 0
            };
        }
    }
}