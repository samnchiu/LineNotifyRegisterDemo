//別忘了using
using Microsoft.EntityFrameworkCore;
using website.Models;
using website.DBContext;

namespace website.DBContext {
  //繼承DbContext
  public class EFCoreContext : DbContext {
    //複寫OnConfiguring
    protected override void OnConfiguring(DbContextOptionsBuilder options) {
      //指定連線字串，連到SQLite
      options.UseSqlite("Data Source=LineUser.sqlite");
    }
    //設定student資料表
    public DbSet<LineUser> LineUsers { get; set; }
    public DbSet<SendLog> SendLogs {get; set;}

    public async Task<LineUser> FindOrCreateLineUserAsync(LineUser lineUser)
        {
            var user = await LineUsers.FirstOrDefaultAsync(u => u.sub == lineUser.sub);

            if (user == null)
            {
                user = new LineUser
                {
                    sub = lineUser.sub,
                    Name = lineUser.Name,
                    email = lineUser.email,
                    IdToken = lineUser.IdToken,
                    AccessToken = lineUser.AccessToken,
                    registed = false
                };

                LineUsers.Add(user);
                await SaveChangesAsync();
            }

            return user;
        }

    public async Task<LineUser> UpdateLineUserAsync(LineUser updatedUser)
    {
        var user = await LineUsers.FirstOrDefaultAsync(u => u.sub == updatedUser.sub);

        if (user == null)
        {
            // 處理使用者不存在的情況
            throw new Exception("User not found.");
        }

        // 更新使用者資料
        user.Name = updatedUser.Name;
        user.email = updatedUser.email;
        user.IdToken = updatedUser.IdToken;
        user.AccessToken = updatedUser.AccessToken;
        user.registed = updatedUser.registed;


        await SaveChangesAsync();

        return user;
    }
        public async Task AddSendLogAsync(SendLog sendLog)
    {
        await SendLogs.AddAsync(sendLog);
        await SaveChangesAsync();
    }

    public async Task<List<SendLog>> GetAllSendLogsAsync()
    {
        return await SendLogs.ToListAsync();
    }
    public async Task<List<string?>> GetAllAccessTokensAsync()
   {
      var accessTokens = await LineUsers.Where(u => u.AccessToken != null).Select(u => u.AccessToken).ToListAsync();
      return accessTokens;
   }

  public async Task<List<LineUser>> GetAllLineUsersAsync()
  {
      var lineUsers = await LineUsers.ToListAsync();
      return lineUsers;
  }

    
  }
  

  
}