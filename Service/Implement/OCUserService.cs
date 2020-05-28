using Data;
using Data.Models;
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
    public class OCUserService : IOCUserService
    {
        private readonly DataContext _context;

        public OCUserService(DataContext context)
        {
            _context = context;
        }

        public async Task<object> AddOrUpdate(int userid, int ocid, bool status)
        {

            try
            {
                var item = await _context.OCUsers.Include(x => x.OC).FirstOrDefaultAsync(x => x.OCID == ocid && x.UserID == userid);
                var user = await _context.Users.FindAsync(userid);
                //Neu user do chuyen  status ve false thi xoa luon
                if (!status && item != null)
                {
                    user.LevelOC = 0;
                    user.OCID = 0;
                    _context.OCUsers.Remove(item);

                }
                else
                {
                    //Kiem tra xem user do co thuoc phong nao khac khong
                    var item2 = await _context.OCUsers.FirstOrDefaultAsync(x => x.UserID == userid);
                    if (item2 != null && item2.Status)
                        return new
                        {
                            status = false,
                            message = "The user has already existed in other department!"
                        };
                    else
                    {
                        var ocModel = await _context.OCs.FindAsync(ocid);
                        user.LevelOC = ocModel.Level;
                        user.OCID = ocid;

                        var oc = new OCUser
                        {
                            OCID = ocid,
                            UserID = userid,
                            Status = true
                        };
                        _context.OCUsers.Add(oc);

                    }

                }
                await _context.SaveChangesAsync();
                ////Neu chua co thi them moi
                //if (item == null)
                //{

                //    //Kiem tra xem lai xem trong OCUSer da gan user nay cho department nao chua
                //    var item2 = await _context.OCUsers.FirstOrDefaultAsync(x => x.UserID == userid);
                //    if (item2 != null && item2.Status)
                //        return new
                //        {
                //            status = false,
                //            message = "The user has already existed in other department!"
                //        };
                //    user.LevelOC = item.OC.Level;
                //    var oc = new OCUser();
                //    oc.OCID = ocid;
                //    oc.UserID = userid;
                //    oc.Status = true;
                //    user.OCID = ocid;
                //    _context.OCUsers.Add(oc);
                //    await _context.SaveChangesAsync();
                //}//co roi thi update
                //else
                //{
                //    item.Status = !item.Status;
                //    if (item.Status == true)
                //    {
                //        user.OCID = ocid;
                //        user.LevelOC = item.OC.Level;
                //    }
                //    else
                //    {
                //        user.OCID = 0;
                //        user.LevelOC = 0;
                //    }
                //    await _context.SaveChangesAsync();
                //}

                return new
                {
                    status = true,
                    message = "Successfully!"
                };
            }
            catch (Exception)
            {
                return new
                {
                    status = false,
                    message = "Error!"
                };
            }
        }

        public async Task<PagedList<Data.ViewModel.OCUser.User>> GetListUser(int page = 1, int pageSize = 10, int ocid = 0, string text = "")
        {
            var source = await _context.Users.Select(x => new Data.ViewModel.OCUser.User
            {
                ID = x.ID,
                Username = x.Username,
                RoleName = x.Role.Name,
                RoleID = x.RoleID,
                Status = _context.OCUsers.Any(a => a.UserID == x.ID && a.OCID == ocid && a.Status == true)
            }).ToListAsync();
            if (!text.IsNullOrEmpty())
            {
                source = source.Where(x => x.Username.ToLower().Contains(text.ToLower())).ToList();
            }
            return  PagedList<Data.ViewModel.OCUser.User>.Create(source, page, pageSize);
        }
        public async Task<object> GetListUser(int ocid)
        {
            var source = await _context.Users.Select(x => new Data.ViewModel.OCUser.User
            {
                ID = x.ID,
                Username = x.Username,
                RoleName = x.Role.Name,
                RoleID = x.RoleID,
                Status = _context.OCUsers.Any(a => a.UserID == x.ID && a.OCID == ocid && a.Status == true)
            }).ToListAsync();
            return source;
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
