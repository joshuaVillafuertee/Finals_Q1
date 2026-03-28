using Microsoft.AspNetCore.Mvc;
using TodoApi.Models;
using System.Security.Cryptography;
using System.Text;

namespace TodoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TodosController : ControllerBase
{
    private static readonly List<Todo> _todos = new();
    private const int MAX_ACTIVE_TASKS = 5;

    // Helper method for SHA-256 hashing (Challenge B)
    private string CalculateHash(Todo todo, string previousHash)
    {
        var content = $"{todo.Id}{todo.Title}{todo.Completed}{todo.CreatedAt:O}{previousHash}";
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(content));
        return Convert.ToHexString(hashBytes).ToLower();
    }

    // GET /api/todos - returns list of todos
    [HttpGet]
    public IActionResult Get()
    {
        // Challenge A: Remove completed tasks after 15 seconds
        var now = DateTime.UtcNow;
        var validTodos = _todos.Where(t => !t.Completed || (now - t.CreatedAt).TotalSeconds <= 15).ToList();
        
        return Ok(validTodos);
    }

    // GET /api/todos/verify - Challenge B: Blockchain verification
    [HttpGet("verify")]
    public IActionResult VerifyChain()
    {
        try
        {
            for (int i = 0; i < _todos.Count; i++)
            {
                var current = _todos[i];
                var previousHash = i == 0 ? "genesis" : _todos[i - 1].Hash ?? "";
                var expectedHash = CalculateHash(current, previousHash);

                if (current.Hash != expectedHash)
                {
                    return Conflict("Chain Tampered");
                }
            }
            return Ok("Chain Valid");
        }
        catch
        {
            return Conflict("Chain Tampered");
        }
    }

    // POST /api/todos - creates a new todo
    [HttpPost]
    public IActionResult Create([FromBody] Todo input)
    {
        // Challenge A: Check max active tasks constraint
        var activeTasks = _todos.Count(t => !t.Completed);
        if (activeTasks >= MAX_ACTIVE_TASKS)
        {
            return BadRequest(new { error = $"Maximum {MAX_ACTIVE_TASKS} active tasks allowed." });
        }

        if (string.IsNullOrWhiteSpace(input.Title))
        {
            return BadRequest(new { error = "Title must not be empty." });
        }

        var todo = new Todo
        {
            Id = Guid.NewGuid(),
            Title = input.Title.Trim(),
            Completed = false,
            CreatedAt = DateTime.UtcNow
        };

        // Challenge B: Calculate blockchain hash
        var previousHash = _todos.Any() ? _todos.Last().Hash ?? "" : "genesis";
        todo.PreviousHash = previousHash;
        todo.Hash = CalculateHash(todo, previousHash);

        _todos.Add(todo);
        return CreatedAtAction(nameof(Get), new { id = todo.Id }, todo);
    }

    // PUT /api/todos/{id} - updates an existing todo
    [HttpPut("{id:guid}")]
    public IActionResult Update(Guid id, [FromBody] Todo input)
    {
        var todoIndex = _todos.FindIndex(t => t.Id == id);
        if (todoIndex == -1)
        {
            return NotFound();
        }

        // Challenge A: Sequential integrity - can only complete in order
        if (input.Completed)
        {
            var incompleteBefore = _todos.Where(t => !t.Completed && t.CreatedAt < _todos[todoIndex].CreatedAt).ToList();
            if (incompleteBefore.Any())
            {
                return BadRequest(new { error = "Must complete tasks in order of creation (FIFO)." });
            }
        }

        if (string.IsNullOrWhiteSpace(input.Title))
        {
            return BadRequest(new { error = "Title must not be empty." });
        }

        var todo = _todos[todoIndex];
        todo.Title = input.Title.Trim();
        todo.Completed = input.Completed;

        // Challenge B: Recalculate hash for updated item and all subsequent items
        var previousHash = todoIndex == 0 ? "genesis" : _todos[todoIndex - 1].Hash ?? "";
        todo.PreviousHash = previousHash;
        todo.Hash = CalculateHash(todo, previousHash);

        // Recalculate hashes for all subsequent items
        for (int i = todoIndex + 1; i < _todos.Count; i++)
        {
            var subsequentTodo = _todos[i];
            subsequentTodo.PreviousHash = _todos[i - 1].Hash;
            subsequentTodo.Hash = CalculateHash(subsequentTodo, _todos[i - 1].Hash);
        }

        return NoContent();
    }

    // DELETE /api/todos/{id} - removes a todo
    [HttpDelete("{id:guid}")]
    public IActionResult Delete(Guid id)
    {
        var todoIndex = _todos.FindIndex(t => t.Id == id);
        if (todoIndex == -1)
        {
            return NotFound();
        }

        _todos.RemoveAt(todoIndex);

        // Challenge B: Recalculate hashes for all subsequent items
        for (int i = todoIndex; i < _todos.Count; i++)
        {
            var todo = _todos[i];
            var previousHash = i == 0 ? "genesis" : _todos[i - 1].Hash ?? "";
            todo.PreviousHash = previousHash;
            todo.Hash = CalculateHash(todo, previousHash);
        }

        return NoContent();
    }
}
