using Data;
using Data.Models;
using Microsoft.EntityFrameworkCore;
using Service.Helpers;
using Service.Interface;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Service.Implement
{
    public class AuthService : IAuthService
    {
        private readonly DataContext _context;
        private readonly ILineService _lineService;
        public AuthService(DataContext context, ILineService lineService)
        {
            _context = context;
            _lineService = lineService;
        }
        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != passwordHash[i]) return false;
                }
                return true;
            }
        }
        public async Task<User> Login(string username, string password)
        {
            if (username.IsNullOrEmpty())
                return null;
            var user =await FindByNameAsync(username);

            if (user == null)
                return null;

            if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
                return null;
            return user;
        }
        private async void PublishhMessage()
        {
            // mã token truy cập
            var code = "HENRY";
            var grant_type = "authorization_code";
            var redirect_uri = "http://10.4.4.224:100/todolist";
            var client_id = "HF6qOCM9xL4lXFsqOLPzhJ";
            var client_secret = "IvjiGAE8TAD8DOONBJ0Z71Ir9daUNlqMsy69ebokcQN";
            using (var client = new HttpClient())
            {
                // tin nhắn sẽ được thông báo
                var content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "grant_type", grant_type },
                    { "code", code },
                    { "redirect_uri", redirect_uri },
                    { "client_id", client_id },
                    { "client_secret", client_secret },
                });

                // thêm mã token vào header
              //  client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ACCESS_TOKEN);

                // Thực hiện gửi thông báo
                var result = await client.PostAsync("https://api.line.me/oauth2/v2.1/token", content);
            }
        }
        public async Task<Role> GetRolesAsync(int role)
        {
            return await _context.Roles.FirstOrDefaultAsync(x => x.ID == role);
        }
        public async Task<User> FindByNameAsync(string username)
        {
            var item = await _context.Users.FirstOrDefaultAsync(x => x.Username.ToLower().Equals(username.ToLower()));
            if (item != null)
                return item;

            return null;
        }
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }
        public async Task<User> Register(User user, string password)
        {
            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(password, out passwordHash, out passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            await _context.Users.AddAsync(user);

            await _context.SaveChangesAsync();

            return user;
        }
        public async Task<User> Edit(string username)
        {
            var item = await _context.Users.FirstOrDefaultAsync(x => x.Username == username);
            byte[] passwordHash, passwordSalt;
            CreatePasswordHash("1", out passwordHash, out passwordSalt);
            item.PasswordHash = passwordHash;
            item.PasswordSalt = passwordSalt;

            await _context.SaveChangesAsync();

            return item;
        }
        public async Task<User> GetById(int Id)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.ID == Id);
        }
        private bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            this.disposed = true;
        }
    }
}
