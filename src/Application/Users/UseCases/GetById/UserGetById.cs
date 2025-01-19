using Domain.UserEntity;

namespace Application.Users.UseCases.GetById;

public class UserGetById(IUserRepository userRepo)
{
    public async Task<User?> GetUserAsync(int id)
    {
        var userId = new UserId(id);
        var user = await userRepo.SearchByIdAsync(userId);
        return user;
    }
}