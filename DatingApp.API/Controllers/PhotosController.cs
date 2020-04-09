using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DatingApp.API.Controllers
{
  [Authorize]
  [Route("api/users/{userId}/photos")]
  [ApiController]
  public class PhotosController : ControllerBase
  {
    private readonly IDatingRepository _repo;
    private readonly IMapper _mapper;
    private readonly IOptions<CloudinarySettings> _cloudinarySettings;
    private Cloudinary _cloudinary;

    public PhotosController(IDatingRepository repo, IMapper mapper, IOptions<CloudinarySettings> cloudinarySettings)
    {
      this._cloudinarySettings = cloudinarySettings;
      this._mapper = mapper;
      this._repo = repo;

      Account acc = new Account(
          _cloudinarySettings.Value.CloudName,
          _cloudinarySettings.Value.ApiKey,
          _cloudinarySettings.Value.ApiSecret
        );

      _cloudinary = new Cloudinary(acc);
    }

    [HttpGet("{id}", Name = "GetPhoto")]
    public async Task<IActionResult> GetPhoto(int id)
    {
      var photoFromRepo = await _repo.GetPhoto(id);

      var photo = _mapper.Map<PhotoForReturnDto>(photoFromRepo);

      return Ok(photo);
    }

    [HttpPost]
    public async Task<IActionResult> AddPhotoForUser(int userId, [FromForm] PhotoForCreationDto photoForCreationDto)
    {
      if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
        return Unauthorized();

      var userFromRepo = await this._repo.GetUser(userId);

      var file = photoForCreationDto.File;

      var uploadResult = new ImageUploadResult();

      if (file != null && file.Length > 0)
      {
        using (var stream = file.OpenReadStream())
        {
          var uploadParams = new ImageUploadParams()
          {
            File = new FileDescription(file.Name, stream),
            Transformation = new Transformation().Width(500).Height(500).Crop("fill").Gravity("face")
          };

          uploadResult = _cloudinary.Upload(uploadParams);
        }
      }

      if (uploadResult.PublicId != null && uploadResult.PublicId != "")
      {
        photoForCreationDto.Url = uploadResult.Uri.ToString();
        photoForCreationDto.PublicId = uploadResult.PublicId;

        var photo = _mapper.Map<Photo>(photoForCreationDto);

        if (!userFromRepo.Photos.Any(u => u.IsMain))
          photo.IsMain = true;

        userFromRepo.Photos.Add(photo);

        if (await _repo.SaveAll())
        {
          var photoToReturn = _mapper.Map<PhotoForReturnDto>(photo);
          return CreatedAtRoute("GetPhoto", new { userId, id = photo.Id }, photoToReturn);
        }
      }

      return BadRequest("No fue posible agregar la foto");

    }

    [HttpPost("{id}/setMain")]
    public async Task<IActionResult> SetMainPhoto(int userId, int id)
    {
      if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
        return Unauthorized();

      var userFromRepo = await this._repo.GetUser(userId);

      if (!userFromRepo.Photos.Any(p => p.Id == id))
        return Unauthorized();

      var photoFromRepo = await this._repo.GetPhoto(id);

      if (photoFromRepo.IsMain)
        return BadRequest("Esta ya es la foto principal");

      var currentMainPhoto = await this._repo.GetMainPhotoForUser(userId);
      if (currentMainPhoto != null)
        currentMainPhoto.IsMain = false;

      photoFromRepo.IsMain = true;

      if (await this._repo.SaveAll())
      {
        return NoContent();
      }

      return BadRequest("No se pudo establecer la foto principal");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePhoto(int userId, int id)
    {
      if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
        return Unauthorized();

      var userFromRepo = await this._repo.GetUser(userId);

      if (!userFromRepo.Photos.Any(p => p.Id == id))
        return Unauthorized();

      var photoFromRepo = await this._repo.GetPhoto(id);

      if (photoFromRepo.IsMain)
        return BadRequest("No puedes eliminar la foto principal");

      if (photoFromRepo != null)
      {
        if (photoFromRepo.PublicId == null)
        {
          _repo.Delete(photoFromRepo);
        }
        else
        {
          var deleteParams = new DeletionParams(photoFromRepo.PublicId);
          var result = _cloudinary.Destroy(deleteParams);

          if (result.Result == "ok")
          {
            _repo.Delete(photoFromRepo);
          }
        }


        if (await _repo.SaveAll())
        {
          return Ok();
        }
      }

      return BadRequest("Error al borrar la foto");
    }

  }
}