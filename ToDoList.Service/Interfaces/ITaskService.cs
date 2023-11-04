using System.Data.SqlTypes;
using ToDoList.Domain.Entity;
using ToDoList.Domain.Filters.Task;
using ToDoList.Domain.Response;
using ToDoList.Domain.ViewModels.Task;

namespace ToDoList.Service.Interfaces;

public interface ITaskService
{
    Task<IBaseResponse<IEnumerable<TaskViewModel>>> CalculateCompletedTasks();

    Task<IBaseResponse<IEnumerable<TaskCompletedViewModel>>> GetCompletedTasks();

    Task<IBaseResponse<TaskEntity>> Create(CreateTaskViewModel model);

    Task<IBaseResponse<bool>> EndTask(long id);

    Task<DataTableResult> GetTask(TaskFilter filter);
}