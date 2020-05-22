using AutoMapper;
using Data;
using Data.Models;
using Data.ViewModel.User;
using Microsoft.EntityFrameworkCore;
using Service.Helpers;
using Service.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Implement
{
    public class UserService : IUserService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public UserService(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<bool> Create(UserViewModel entity)
        {
            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(entity.Password, out passwordHash, out passwordSalt);
            var user = _mapper.Map<User>(entity);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            await _context.Users.AddAsync(user);

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;

            }
        }
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }
        public async Task<bool> ChangeAvatar(int id, string imagePath)
        {
            try
            {
                var item = await _context.Users.FindAsync(id);
                item.ImageBase64 = Convert.FromBase64String(imagePath);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> Delete(int id)
        {
            var entity = await _context.Users.FindAsync(id);
            var tags = await _context.Tags.Where(x=>x.UserID.Equals(id)).ToListAsync();
            var tasks = await _context.Tasks.Where(x=>x.CreatedBy.Equals(id)).ToListAsync();
            var managers = await _context.Managers.Where(x => x.UserID.Equals(id)).ToListAsync();
            var members = await _context.TeamMembers.Where(x => x.UserID.Equals(id)).ToListAsync();
            var notifications = await _context.Notifications.Where(x => x.UserID.Equals(id)).ToListAsync();
            var notificationDetails = await _context.NotificationDetails.Where(x => x.UserID.Equals(id)).ToListAsync();
            var ocUsers = await _context.OCUsers.Where(x => x.UserID.Equals(id)).ToListAsync();
            var deputies = await _context.Deputies.Where(x => x.UserID.Equals(id)).ToListAsync();


            if (entity == null)
            {
                return false;
            }

            try
            {
                _context.Tags.RemoveRange(tags);
                _context.Tasks.RemoveRange(tasks);
                _context.Managers.RemoveRange(managers);
                _context.TeamMembers.RemoveRange(members);
                _context.Notifications.RemoveRange(notifications);
                _context.NotificationDetails.RemoveRange(notificationDetails);
                _context.OCUsers.RemoveRange(ocUsers);
                _context.Deputies.RemoveRange(deputies);
                _context.Users.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;

            }
        }
        public async Task<bool> UploapProfile(int id,byte[] image)
        {
            var entity = await _context.Users.FindAsync(id);
            if (entity == null)
            {
                return false;
            }

            try
            {
                entity.ImageBase64 = image;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;

            }
        }

        public async Task<List<User>> GetAll()
        {
            return await _context.Users.Where(x => x.Username != "admin").ToListAsync();
        }

        public async Task<PagedList<ListViewModel>> GetAllPaging(int page, int pageSize, string keyword)
        {
            var source = _context.Users.Where(x => x.Username != "admin").Select(x => new ListViewModel { isLeader = x.isLeader, ID = x.ID, Username = x.Username, Email = x.Email, RoleName = x.Role.Name, RoleID = x.RoleID }).AsQueryable();
            if (!keyword.IsNullOrEmpty())
            {
                source = source.Where(x => x.Username.Contains(keyword) || x.Email.Contains(keyword));
            }
            return await PagedList<ListViewModel>.CreateAsync(source, page, pageSize);
        }

        public async Task<User> GetByID(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<object> GetListUser()
        {
            return await _context.Users.Where(x => x.Username != "admin").Select(x => new { x.ID, x.Username, x.Email, RoleName = x.Role.Name, x.RoleID }).ToListAsync();

        }

        public async Task<bool> Update(User entity)
        {
            var item = await _context.Users.FindAsync(entity.ID);
            item.Username = entity.Username;
            item.Email = entity.Email;
            item.RoleID = entity.RoleID;
            item.isLeader = entity.isLeader;
            if (item.PasswordHash.IsNullOrEmpty() && item.PasswordSalt.IsNullOrEmpty())
            {
                byte[] passwordHash, passwordSalt;
                CreatePasswordHash("1", out passwordHash, out passwordSalt);

                item.PasswordHash = passwordHash;
                item.PasswordSalt = passwordSalt;
            }
            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;

            }
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

        public async Task<List<string>> GetUsernames()
        {
            return await _context.Users.Where(x => x.Username != "admin").Select(x => x.Username).ToListAsync();

        }

        public async Task<bool> ResetPassword(int id)
        {
            byte[] passwordHash, passwordSalt;
            var item = await _context.Users.FindAsync(id);
            string pass = "1";
            CreatePasswordHash("1", out passwordHash, out passwordSalt);
            try
            {
                item.PasswordHash = passwordHash;
                item.PasswordSalt = passwordSalt;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
                throw;
            }
            throw new NotImplementedException();
        }

        public async Task<bool> UpdateTokenLineForUser(string token, int userID)
        {
            var item = await _context.Users.FindAsync(userID);
            item.AccessTokenLineNotify = token;
            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public async Task<bool> RemoveTokenLineForUser(int userID)
        {
            var item = await _context.Users.FindAsync(userID);
            item.AccessTokenLineNotify = null;
            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
