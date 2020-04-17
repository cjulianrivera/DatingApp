using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
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
    public async Task<IActionResult> GetUsers([FromQuery]UserParams userParams)
    {
      var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

      var userFromRepo = await _repo.GetUser(currentUserId);

      userParams.UserId = currentUserId;

      if (string.IsNullOrEmpty(userParams.Gender))
      {
        userParams.Gender = userFromRepo.Gender == "male" ? "female" : "male";
      }

      var users = await this._repo.GetUsers(userParams);

      var usersToReturn = this._mapper.Map<IEnumerable<UserForListDto>>(users);

      Response.AddPagination(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

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


    [HttpPost("{id}/like/{recipientId}")]
    public async Task<IActionResult> LikeUser(int id, int recipientId)
    {
      if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
        return Unauthorized();

      var like = await _repo.GetLike(id, recipientId);

      if (like != null)
      {
        return BadRequest("you already like this user");
      }

      if (await _repo.GetUser(recipientId) == null)
      {
        return NotFound();
      }

      like = new Like
      {
        LikerId = id,
        LikeeId = recipientId
      };

      _repo.Add<Like>(like);

      if (await _repo.SaveAll())
        return Ok();

      return BadRequest("Failed to like user");
    }

  }
}