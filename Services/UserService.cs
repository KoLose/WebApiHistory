using Microsoft.AspNetCore.Mvc;
using WebApi.Interfaces;
using WebApi.Models;
using WebApi.Models.Response;
using WebApi.Requests;

namespace WebApi.Services;

public class UserService : IUserService
{
    private readonly Supabase.Client _supabaseClient;
    
    public UserService(Supabase.Client supabaseClient)
    {
        _supabaseClient = supabaseClient;
    }

    /// <summary>
    /// Variable for adding roleId for user
    /// </summary>
    private int role;

    /// <summary>
    /// Variable for login User and enjecting his role
    /// </summary>
    private string roleName;
    
    /// <summary>
    /// List for adding RoleId on time creating user
    /// </summary>
    private List<User> users;

    public async Task<IActionResult> GetUserAsync()
    {
        await _supabaseClient.InitializeAsync();                                              // Initialization Supabase object in method
        var usersResponse = await _supabaseClient.From<User>().Get();                         // Get all users from Supabase
        var users = usersResponse.Models                                // Creating list for users
            .Select(u => new UserResponse
        {
            UserId = u.UserId,
            UserName = u.UserName,
            Mail = u.Mail,
            RoleName = u.Role.RoleName,
        });
        
        try
        {
            
            return new OkObjectResult(new
                {
                    data = new
                    {
                        users = users
                    },
                    status = true,
                }
            );
        }
        
        catch (Exception e)
        {
            return new BadRequestObjectResult(new 
            { 
                status = false 
            });
        }
    }

    public async Task<IActionResult> PostUserAsync(CreateNewUser newUser)
    {
        await _supabaseClient.InitializeAsync();                            // Initialization Supabase object in method
        
        var usersResponse = await _supabaseClient.From<User>()              // Get all users from Supabase
            .Get(); 
        users = usersResponse.Models;                                       // Creating list for users
        
        var action = newUser.Action;                                  /* Initialization variable action
                                                                            (this is a variable to understand that our function is) */
        if (action == null)                                         
        {
            return new BadRequestObjectResult(new
            {
                status = false,
            });
        }
        
        try
        {
            // Register function
            if (action == "Register")
            {
                var roleResponse = await _supabaseClient.From<Role>()                 // Get roleId from Supabase
                    .Where(r => r.RoleName  == "User")
                    .Get();
                role = roleResponse.Models.FirstOrDefault().RoleId;                   // Creating variable for next creating user
                
                if (users.FirstOrDefault(u => u.Mail == newUser.Mail) == null)  // If this user not exist -> create new user
                {
                    var user = new User()   // Creating new user
                    {
                        UserName = newUser.UserName,
                        Mail = newUser.Mail,
                        Password = newUser.Password,
                        RoleId = role,
                    };
                    
                    await _supabaseClient.From<User>().Insert(user);    // Saving new user

                    return new OkObjectResult(new                       // If user created -> send ok request
                        {
                            status = true,
                        }
                    );
                }
                
                return new BadRequestObjectResult(new             // Else this user exist -> bad request
                {
                    status = false,
                });
            }

            // Login function
            if (action == "Login")
            {
                var user = users.FirstOrDefault(u => u.Mail == newUser.Mail &&  u.Password == newUser.Password);
                if (user != null)
                {

                   Console.WriteLine();
                        
                    return new OkObjectResult(new   // Return 
                    {
                        userId = user.UserId,
                        userName = user.UserName,
                        mail = user.Mail,
                        password = user.Password,
                        roleName = user.Role.RoleName,
                        status = true
                    });
                }
            }
            
            return new BadRequestObjectResult(new 
            { 
                status = false
            });
        }
        
        catch (Exception e)
        {
            return new BadRequestObjectResult(new 
            { 
                status = false,
                error = e.Message, // Покажет текст ошибки
                inner = e.InnerException?.Message // Покажет внутреннюю ошибку, если есть
            });
        }
    }

    
    
    public async Task<IActionResult> DeleteUserAsync(DeleteUser deleteUser)
    {
        var userResponse = await _supabaseClient.From<User>()       // Initialization user that has id of deleteUser     
            .Where(u => u.UserId  == deleteUser.UserId)
            .Get();
        var user = userResponse.Models.FirstOrDefault();            // Creating variable for this user
        
        try
        {
            if (user != null && user.Role.RoleName != "Admin")
            {
                await _supabaseClient.From<User>().Delete(user);    // Deleting user
                return new OkObjectResult(new
                    {
                        status = true,
                    }
                );
            }

            return new BadRequestObjectResult(new
                {
                    status = false,
                }
            );
        }
        
        catch (Exception e)
        {
            return new BadRequestObjectResult(new 
            { 
                status = false 
            });
        }
    }

    
    
    public async Task<IActionResult> PatchUserAsync(UpdateUser updateUser)
    {
        
        try
        {
            return new OkObjectResult(new
                {
                    status = true,
                }
            );
        }
        
        catch (Exception e)
        {
            return new BadRequestObjectResult(new 
            { 
                status = false 
            });
        }
    }
}