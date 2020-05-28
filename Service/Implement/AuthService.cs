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
        public AuthService(DataContext context)
        {
            _context = context;
        }
        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt);
            var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != passwordHash[i]) return false;
            }
            return true;
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
            using var hmac = new System.Security.Cryptography.HMACSHA512();
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
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
