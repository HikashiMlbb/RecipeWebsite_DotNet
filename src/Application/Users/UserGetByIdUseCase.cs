using Domain.UserEntity;

namespace Application.Users;

public class UserGetByIdUseCase(IUserRepository userRepo)
{
    public async Task<User?> GetUser(int id)
    {
        var userId = new UserId(id);
        var user = await userRepo.SearchById(userId);
        return user;
    } 
}