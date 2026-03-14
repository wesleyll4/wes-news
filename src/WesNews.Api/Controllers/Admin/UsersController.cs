using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WesNews.Application.Interfaces.Repositories;
using WesNews.Domain.Entities;

namespace WesNews.Api.Controllers.Admin;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/admin/users")]
public class UsersController(IUserRepository userRepository) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        IEnumerable<User> users = await userRepository.GetAllAsync();
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        User? user = await userRepository.GetByIdAsync(id);
        if (user == null) return NotFound();
        return Ok(user);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] User userUpdate)
    {
        User? user = await userRepository.GetByIdAsync(id);
        if (user == null) return NotFound();

        user.FullName = userUpdate.FullName;
        user.Email = userUpdate.Email;
        user.Role = userUpdate.Role;
        // Password update is usually handled differently, skipping for simplicity in this basic CRUD

        await userRepository.UpdateAsync(user);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await userRepository.DeleteAsync(id);
        return NoContent();
    }
}
