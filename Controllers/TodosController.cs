using Microsoft.AspNetCore.Mvc;
using TodoApi.Models;

namespace TodoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TodosController : ControllerBase
{
    private static readonly List<Todo> _todos = new();

    // GET /api/todos - returns list of todos
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(_todos);
    }

    // POST /api/todos - creates a new todo
    [HttpPost]
    public IActionResult Create([FromBody] Todo input)
    {
        if (string.IsNullOrWhiteSpace(input.Title))
        {
            return BadRequest(new { error = "Title must not be empty." });
        }

        var todo = new Todo
        {
            Id = Guid.NewGuid(),
            Title = input.Title.Trim(),
            Completed = input.Completed
        };

        _todos.Add(todo);
        return CreatedAtAction(nameof(Get), new { id = todo.Id }, todo);
    }

    // PUT /api/todos/{id} - updates an existing todo
    [HttpPut("{id:guid}")]
    public IActionResult Update(Guid id, [FromBody] Todo input)
    {
        if (string.IsNullOrWhiteSpace(input.Title))
        {
            return BadRequest(new { error = "Title must not be empty." });
        }

        var todo = _todos.FirstOrDefault(t => t.Id == id);
        if (todo == null)
        {
            return NotFound();
        }

        todo.Title = input.Title.Trim();
        todo.Completed = input.Completed;

        return NoContent();
    }

    // DELETE /api/todos/{id} - removes a todo
    [HttpDelete("{id:guid}")]
    public IActionResult Delete(Guid id)
    {
        var todo = _todos.FirstOrDefault(t => t.Id == id);
        if (todo == null)
        {
            return NotFound();
        }

        _todos.Remove(todo);
        return NoContent();
    }
}
