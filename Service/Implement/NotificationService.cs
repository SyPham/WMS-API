using AutoMapper;
using AutoMapper.QueryableExtensions;
using Data;
using Data.Models;
using Data.ViewModel.Notification;
using Microsoft.EntityFrameworkCore;
using Service.Helpers;
using Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Implement
{
    public class NotificationService : INotificationService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private MapperConfiguration _configMapper;
        public NotificationService(DataContext context, IMapper mapper, MapperConfiguration configMapper)
        {
            _context = context;
            _mapper = mapper;
            _configMapper = configMapper;
        }


        public async Task<bool> Create(CreateNotifyParams entity)
        {
            try
            {
                var item = new Notification
                {
                    TaskID = entity.TaskID,
                    Message = entity.Message,
                    URL = entity.URL,
                    Function = entity.AlertType.ToString()
                };
                item.UserID = entity.UserID.GetValueOrDefault();
                await _context.Notifications.AddAsync(item);
                await _context.SaveChangesAsync();

                if (entity.Users.Count > 0 || entity.Users != null)
                {
                    var details = new List<NotificationDetail>();
                    foreach (var user in entity.Users)
                    {
                        details.Add(new NotificationDetail
                        {
                            NotificationID = item.ID,
                            UserID = user,
                            Seen = false
                        });
                    }
                    await _context.AddRangeAsync(details);
                }
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;

            }
        }
        public async System.Threading.Tasks.Task CreateRange(List<CreateNotifyParams> entity)
        {
            foreach (var item in entity)
            {
                await Create(item);
            }
        }


        public async Task<bool> Delete(int id)
        {
            var entity = await _context.Notifications.FindAsync(id);
            if (entity == null)
            {
                return false;
            }

            _context.Notifications.Remove(entity);
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
        public async Task<bool> Seen(int id)
        {
            var entity = await _context.NotificationDetails.FindAsync(id);
            if (entity == null)
            {
                return false;
            }

            entity.Seen = true;
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
        public async Task<List<Notification>> GetAll()
        {
            return await _context.Notifications.ToListAsync();
        }
        public async Task<object> GetAllByUserID(int userid, int page, int pageSize)
        {
            var list = _context.NotificationDetails
                .Where(x=>x.UserID == userid)
                .Include(x => x.Notification).ThenInclude(x=>x.User)
                .Include(x => x.Notification).ThenInclude(x => x.NotificationDetails).ThenInclude(x=>x.User)
                .Include(x => x.User).ProjectTo<NotificationViewModel>(_configMapper);
           // var listAsync = await model.ToListAsync();
           //var list =  _mapper.Map<List<NotificationViewModel>>(model);
            var total = 0;
            var listID = new List<int>();

            foreach (var item in list)
            {
                if (item.Seen == false)
                {
                    total++;
                    listID.Add(item.ID);
                }
            }
            var paging =await PagedList<NotificationViewModel>.CreateAsync(list, page, pageSize);

            return new
            {
                model = paging.OrderByDescending(x => x.ID).ToList(),
                total,
                paging.TotalCount
            };

        }

        public async Task<object> GetNotificationByUser(int userid, int page, int pageSize)
        {
            var userModel = _context.Users;
            var model = await _context.NotificationDetails.Where(x => x.UserID == userid)
                .Join(_context.Notifications,
                 detail => detail.NotificationID,
                notify => notify.ID,
                (detail, notify) => new
                {
                    notify,
                    detail
                })
                 .Join(_context.Users,
                 a => a.notify.UserID,
                b => b.ID,
                (a, b) => new
                {
                    a,
                    b
                })
                .Select(_ => new NotificationViewModel
                {
                    ID = _.a.detail.ID,
                    Message = _.a.notify.Message,
                    Function = _.a.notify.Function,
                    CreatedBy = _.a.notify.UserID,
                    BeAssigned = _.a.detail.UserID,
                    Seen = _.a.detail.Seen,
                    URL = _.a.notify.URL,
                    Sender = _.b.Username,
                    CreatedTime = _.a.notify.CreatedTime,
                }).OrderByDescending(_ => _.CreatedTime).ToListAsync();
            var total = 0;
            var listID = new List<int>();
            foreach (var item in model)
            {
                if (item.Seen == false)
                {
                    total++;
                    listID.Add(item.ID);
                }

            }
            var paging = PagedList<NotificationViewModel>.Create(model, page, pageSize);

            return new
            {
                model = paging,
                total
            };
        }
        public async Task<PagedList<Notification>> GetAllPaging(int page, int pageSize)
        {
            var source = _context.Notifications.AsQueryable();

            return await PagedList<Notification>.CreateAsync(source, page, pageSize);
        }

        public async Task<Notification> GetByID(int id)
        {
            return await _context.Notifications.FindAsync(id);
        }

        public async Task<bool> Update(Notification entity)
        {
            var item = await _context.Notifications.FindAsync(entity.ID);
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
    }
}
