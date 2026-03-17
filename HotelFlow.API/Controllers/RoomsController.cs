using HotelFlow.Application.DTOs.Requests.Rooms;
using HotelFlow.Application.Interfaces;
using HotelFlow.Infrastructure.Data.DbContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace HotelFlow.API.Controllers;

[ApiController]
[Route("api/rooms")]
public class RoomsController : ControllerBase
{
    private readonly IRoomService _roomService;

    public RoomsController(IRoomService roomService)
    {
        _roomService = roomService;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
        => Ok(await _roomService.GetAllAsync());
    
    [Authorize(Roles = "Staff")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateRoomRequest request)
    {
        var id = await _roomService.CreateAsync(request);
        return CreatedAtAction(nameof(Get), new { id }, null);
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateRoomRequest request)
    {
        await _roomService.UpdateAsync(id, request);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _roomService.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("roomtypes")]
    public async Task<IActionResult> GetRoomTypes()
    => Ok(await _roomService.GetAllRoomTypesAsync());

    [Authorize(Roles = "Staff")]
    [HttpPost("roomtypes")]
    public async Task<IActionResult> CreateRoomType(CreateRoomTypeRequest request)
    {
        var id = await _roomService.CreateRoomTypeAsync(request);
        return CreatedAtAction(nameof(GetRoomTypes), new { id }, null);
    }

    [Authorize(Roles = "Staff")]
    [HttpPut("roomtypes/{id}")]
    public async Task<IActionResult> UpdateRoomType(Guid id, UpdateRoomTypeRequest request)
    {
        await _roomService.UpdateRoomTypeAsync(id, request);
        return NoContent();
    }

    [Authorize(Roles = "Staff")]
    [HttpDelete("roomtypes/{id}")]
    public async Task<IActionResult> DeleteRoomType(Guid id)
    {
        await _roomService.DeleteRoomTypeAsync(id);
        return NoContent();
    }
}