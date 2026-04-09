using ViamaticaApi.Application.DTOs.Users;

namespace ViamaticaApi.Application.Interfaces;

public interface IUserService
{
    Task<UserResponseDto> CreateAsync(CreateUserDto dto, int creatorUserId, int creatorRolId);
    Task<IEnumerable<UserResponseDto>> GetAllAsync(int? rolId, string? statusId);
    Task<UserResponseDto> GetByIdAsync(int userId);
    Task<UserResponseDto> UpdateAsync(int userId, UpdateUserDto dto);
    Task DeleteAsync(int userId, int deletedByUserId);
    Task<UserResponseDto> ApproveAsync(int userId, int approvedByUserId);
    Task<UserResponseDto> ChangeStatusAsync(int userId, ChangeUserStatusDto dto);
}
