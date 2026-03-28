using Microsoft.AspNetCore.Mvc;
using TodoApi.Models;

namespace TodoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TodosController : ControllerBase
{
    private static readonly List<Todo> _todos = new();

    // GET /api/todos
    [HttpGet]
    public IActionResult Get() => Ok(_todos);

    // POST /api/todos
    [HttpPost]
    public IActionResult Create([FromBody] Todo input)
    {
        if (string.IsNullOrWhiteSpace(input.Title))
            return BadRequest(new { error = "Title must not be empty." });

        var todo = new Todo
        {
            Id = Guid.NewGuid(),
            Title = input.Title.Trim(),
            Completed = input.Completed
        };

        _todos.Add(todo);
        return CreatedAtAction(nameof(Get), new { id = todo.Id }, todo);
    }

    // PUT /api/todos/{id}
    [HttpPut("{id:guid}")]
    public IActionResult Update(Guid id, [FromBody] Todo input)
    {
        if (string.IsNullOrWhiteSpace(input.Title))
            return BadRequest(new { error = "Title must not be empty." });

        var todo = _todos.FirstOrDefault(t => t.Id == id);
        if (todo is null)
            return NotFound();

        todo.Title = input.Title.Trim();
        todo.Completed = input.Completed;

        return NoContent();
    }

    // DELETE /api/todos/{id}
    [HttpDelete("{id:guid}")]
    public IActionResult Delete(Guid id)
    {
        var todo = _todos.FirstOrDefault(t => t.Id == id);
        if (todo is null)
            return NotFound();

        _todos.Remove(todo);
        return NoContent();
    }
}
