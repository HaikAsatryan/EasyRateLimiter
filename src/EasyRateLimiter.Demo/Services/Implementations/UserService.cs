using EasyRateLimiter.Demo.Contexts;
using EasyRateLimiter.Demo.DTOs.Token;
using EasyRateLimiter.Demo.DTOs.User;
using EasyRateLimiter.Demo.Enums;
using EasyRateLimiter.Demo.Models;
using EasyRateLimiter.Demo.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using ResponseCrafter.Dtos;
using ResponseCrafter.StandardHttpExceptions;

namespace EasyRateLimiter.Demo.Services.Implementations;

public class UserService : IUserService
{
    private readonly MemoryDb _context;
    private readonly ContextUser _contextUser;
    private readonly IAuthenticationService _authenticationService;


    public UserService(MemoryDb context,
        ContextUser contextUser, IAuthenticationService authenticationService)
    {
        _context = context;
        _contextUser = contextUser;
        _authenticationService = authenticationService;
    }

    public async Task CreateUserAsync(CreateUserDto createUserDto)
    {
        if (createUserDto.Role == Roles.SuperAdmin)
            throw new ForbiddenException("superadmin_cannot_be_created");

        var checkIfUsernameExists =
            await _context.Users.FirstOrDefaultAsync(x => x.Username == createUserDto.Username.ToLower());
        if (checkIfUsernameExists != null)
            throw new BadRequestException("username already exists");

        var user = new User
        {
            FullName = createUserDto.FullName,
            CreatedAt = DateTime.UtcNow,
            Role = createUserDto.Role,
            Username = createUserDto.Username.ToLower(),
            Status = Statuses.Active,
            Comment = createUserDto.Comment,
            Password = createUserDto.Password
        };

        await _context.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateUserAsync(UpdateUserDto updateUserDto)
    {
        if (updateUserDto.Role == Roles.SuperAdmin)
            throw new ForbiddenException("superadmin_cannot_be_updated");

        var checkIfUsernameExists =
            await _context.Users.FirstOrDefaultAsync(x => x.Username == updateUserDto.Username.ToLower());
        if (checkIfUsernameExists != null && checkIfUsernameExists.Id != updateUserDto.Id)
            throw new BadRequestException("username_already_exists");

        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == updateUserDto.Id);

        if (user == null)
            throw new NotFoundException("user_not_found");

        user.FullName = updateUserDto.FullName;
        user.Role = updateUserDto.Role;
        user.Username = updateUserDto.Username.ToLower();
        user.Comment = updateUserDto.Comment;

        await _context.SaveChangesAsync();
    }

    public async Task UpdatePasswordAsync(UpdatePasswordDto updatePasswordDto)
    {
        var userToUpdate = await _context.Users.FirstOrDefaultAsync(x => x.Id == updatePasswordDto.Id);
        if (userToUpdate == null)
            throw new NotFoundException();

        if (userToUpdate.Role == Roles.SuperAdmin && _contextUser.Role != Roles.SuperAdmin)
            throw new ForbiddenException("superadmin_password_cannot_be_changed");

        userToUpdate.Password = updatePasswordDto.NewPassword;
        await _context.SaveChangesAsync();

        await _authenticationService.LogoutAllAsync(userToUpdate.Id);
    }

    public async Task UpdateUserStatusAsync(UpdateUserStatusDto updateUserStatusDto)
    {
        var userToUpdate = await _context.Users.FirstOrDefaultAsync(x => x.Id == updateUserStatusDto.Id);

        if (userToUpdate is null)
            throw new NotFoundException();

        if (userToUpdate.Role == Roles.SuperAdmin)
            throw new ForbiddenException("superadmin_cannot_be_updated");

        userToUpdate.Status = updateUserStatusDto.Status;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteUsersAsync(List<long> ids)
    {
        var users = await _context.Users.Where(x => ids.Contains(x.Id)).ToListAsync();

        if (users.Count == 0)
            throw new NotFoundException("users_not_found");

        if (users.Exists(user => user.Role == Roles.SuperAdmin))
        {
            throw new ForbiddenException("superadmin_cannot_be_deleted");
        }

        List<User> usersToDelete = new();

        foreach (var user in users)
        {
            user.Status = Statuses.Deleted;

            usersToDelete.Add(user);
        }

        _context.RemoveRange(usersToDelete);
        await _context.SaveChangesAsync();
    }

    public async Task<PagedResponse<GetUserDto>> GetUsersAsync(int page, int pageSize)
    {
        if (page < 1)
            throw new BadRequestException("page must be positive number");
        if (pageSize < 1)
            throw new BadRequestException("pageSize must be positive number");

        var usersQuery = _context.Users
            .Where(s => s.Status != Statuses.Deleted && s.Role != Roles.SuperAdmin)
            .AsQueryable();

        usersQuery = usersQuery
            .Where(x => x.Status != Statuses.Deleted)
            .OrderByDescending(x => x.FullName);

        var users = (await usersQuery.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync())
            .Select(u => new GetUserDto
            {
                Id = u.Id,
                Username = u.Username,
                FullName = u.FullName,
                Role = new RolesSelect()
                {
                    Id = u.Role,
                    Name = u.Role,
                },
                CreatedAt = u.CreatedAt,
                Status = u.Status,
                Comment = u.Comment
            }).ToList();

        var totalCount = await usersQuery.CountAsync();

        var response = new PagedResponse<GetUserDto>(users, page, pageSize, totalCount);

        return response;
    }

    public void SetUserContext(IdentifyTokenDto token, ContextUser contextUser)
    {
        contextUser.Id = token.User.Id;
        contextUser.Role = token.User.Role;
        contextUser.Username = token.User.Username;
        contextUser.TokenId = token.TokenId;
        contextUser.TokenExpirationDate = token.ExpirationDate;
    }
}