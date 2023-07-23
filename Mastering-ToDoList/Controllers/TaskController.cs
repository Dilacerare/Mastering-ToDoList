using Microsoft.AspNetCore.Mvc;
using ToDoList.Domain.Response;
using ToDoList.Domain.ViewModels.Task;
using ToDoList.Service.Interfaces;

namespace Mastering_ToDoList.Controllers;

public class TaskController : Controller
{

    private readonly ITaskService _taskService;

    public TaskController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateTaskViewModel model)
    {
        var response = await _taskService.Create(model);
        if (response.StatusCode == ToDoList.Domain.Enum.StatusCode.Ok)
        {
            return Ok(new { description = response.Description });
        }
        return BadRequest(new {description = response.Description});
    }
}