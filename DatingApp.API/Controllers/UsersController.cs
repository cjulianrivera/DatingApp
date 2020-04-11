using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  [ServiceFilter(typeof(LogUserActivity))]
  public class UsersController : ControllerBase
  {
    private readonly IDatingRepository _repo;
    private readonly IMapper _mapper;
    public UsersController(IDatingRepository repo, IMapper mapper)
    {
      this._mapper = mapper;
      this._repo = repo;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
      var users = await this._repo.GetUsers();

      var usersToReturn = this._mapper.Map<IEnumerable<UserForListDto>>(users);

      return Ok(usersToReturn);
    }

    [HttpGet("{id}", Name = "GetUser")]
    public async Task<IActionResult> GetUser(int id)
    {
      var user = await this._repo.GetUser(id);

      var userToReturn = this._mapper.Map<UserForDetailedDto>(user);

      return Ok(userToReturn);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, UserForUpdateDto userForUpdateDto)
    {
      if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
        return Unauthorized();


      var userFromRepo = await this._repo.GetUser(id);

      this._mapper.Map(userForUpdateDto, userFromRepo);

      this._repo.Update(userFromRepo);

      if (await this._repo.SaveAll())
      {
        return NoContent();
      }

      throw new Exception($"Updating user {id} failed on save");

    }

  }
}