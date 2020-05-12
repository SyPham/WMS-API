using AutoMapper;
using Data;
using Data.Models;
using Data.ViewModel;
using Data.ViewModel.OC;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGeneration.Contracts.Messaging;
using Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Implement
{
    public class OCService : IOCService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public OCService(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public Task<bool> AddOrUpdate(OC entity)
        {
            throw new NotImplementedException();
        }

        public async Task<List<TreeView>> GetListTree()
        {
            var listLevels = await _context.OCs.OrderBy(x => x.Level).ToListAsync();
            var levels = new List<TreeView>();
            foreach (var item in listLevels)
            {
                var levelItem = new TreeView();
                levelItem.key = item.ID;
                levelItem.title = item.Name;
                levelItem.levelnumber = item.Level;
                levelItem.parentid = item.ParentID;
                levels.Add(levelItem);
            }

            List<TreeView> hierarchy = new List<TreeView>();

            hierarchy = levels.Where(c => c.parentid == 0)
                            .Select(c => new TreeView()
                            {
                                key = c.key,
                                title = c.title,
                                code = c.code,
                                levelnumber = c.levelnumber,
                                parentid = c.parentid,
                                children = GetChildren(levels, c.key)
                            })
                            .ToList();

            return hierarchy;
        }
        private async Task<List<TreeView>> GetListTree(int parentID, int id)
        {
            var listLevels = await _context.OCs.OrderBy(x => x.Level).ToListAsync();
            var levels = new List<TreeView>();
            foreach (var item in listLevels)
            {
                var levelItem = new TreeView();
                levelItem.key = item.ID;
                levelItem.title = item.Name;
                levelItem.levelnumber = item.Level;
                levelItem.parentid = item.ParentID;
                levels.Add(levelItem);
            }

            List<TreeView> hierarchy = new List<TreeView>();

            hierarchy = levels.Where(c => c.key == id && c.parentid == parentID)
                            .Select(c => new TreeView()
                            {
                                key = c.key,
                                title = c.title,
                                code = c.code,
                                levelnumber = c.levelnumber,
                                parentid = c.parentid,
                                children = GetChildren(levels, c.key)
                            })
                            .ToList();


            HieararchyWalk(hierarchy);

            return hierarchy;
        }
        private void HieararchyWalk(List<TreeView> hierarchy)
        {
            if (hierarchy != null)
            {
                foreach (var item in hierarchy)
                {
                    //Console.WriteLine(string.Format("{0} {1}", item.Id, item.Text));
                    HieararchyWalk(item.children);
                }
            }
        }
        public List<TreeView> GetChildren(List<TreeView> levels, int parentid)
        {
            return levels
                    .Where(c => c.parentid == parentid)
                    .Select(c => new TreeView()
                    {
                        key = c.key,
                        title = c.title,
                        code = c.code,
                        levelnumber = c.levelnumber,
                        parentid = c.parentid,
                        children = GetChildren(levels, c.key)
                    })
                    .ToList();
        }

        public string GetNode(string code)
        {
            throw new NotImplementedException();

        }

        public string GetNode(int id)
        {
            var list = new List<OC>();
            list = _context.OCs.ToList();
            var list2 = new List<OC>();
            list2.Add(list.FirstOrDefault(x => x.ID == id));
            var parentID = list.FirstOrDefault(x => x.ID == id).ParentID;
            foreach (var item in list)
            {
                if (parentID == 0)
                    break;
                if (parentID != 0)
                {
                    //add vao list1
                    list2.Add(list.FirstOrDefault(x => x.ID == parentID));
                }
                //cap nhat lai parentID
                parentID = list.FirstOrDefault(x => x.ID == parentID).ParentID;

            }
            return string.Join("->", list2.OrderBy(x => x.ParentID).Select(x => x.Name).ToArray());
        }

        public async Task<bool> IsExistsCode(int ID)
        {
            return await _context.OCs.AnyAsync(x => x.ID == ID);
        }

        public async Task<bool> Rename(TreeViewRename level)
        {
            var item = await _context.OCs.FindAsync(level.key);
            item.Name = level.title;
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


        public async Task<IEnumerable<TreeViewOC>> GetListTreeOC(int parentID, int id)
        {
            var listLevels = await _context.OCs.ToListAsync();
            var levels = new List<TreeViewOC>();
            foreach (var item in listLevels)
            {
                var levelItem = new TreeViewOC();
                levelItem.ID = item.ID;
                levelItem.Name = item.Name;
                levelItem.Level = item.Level;
                levelItem.ParentID = item.ParentID;
                levels.Add(levelItem);
            }

            List<TreeViewOC> hierarchy = new List<TreeViewOC>();

            hierarchy = levels.Where(c => c.ID == id && c.ParentID == parentID)
                            .Select(c => new TreeViewOC()
                            {
                                ID = c.ID,
                                Name = c.Name,
                                Level = c.Level,
                                ParentID = c.ParentID,
                                children = GetTreeChildren(levels, c.ID)
                            })
                            .ToList();


            HieararchyWalkTree(hierarchy);

            return hierarchy;
        }
        private void HieararchyWalkTree(List<TreeViewOC> hierarchy)
        {
            if (hierarchy != null)
            {
                foreach (var item in hierarchy)
                {
                    //Console.WriteLine(string.Format("{0} {1}", item.Id, item.Text));
                    HieararchyWalkTree(item.children);
                }
            }
        }
        public List<TreeViewOC> GetTreeChildren(List<TreeViewOC> levels, int parentid)
        {
            return levels
                    .Where(c => c.ParentID == parentid)
                    .Select(c => new TreeViewOC()
                    {
                        ID = c.ID,
                        Name = c.Name,
                        Level = c.Level,
                        ParentID = c.ParentID,
                        children = GetTreeChildren(levels, c.ID)
                    })
                    .ToList();
        }

        public async Task<object> CreateOC(CreateOCViewModel oc)
        {
            try
            {
                if(oc.ID == 0)
                {
                    var item = _mapper.Map<Data.Models.OC>(oc);
                    item.Level = 1;

                    await _context.OCs.AddAsync(item);
                }
                else
                {
                   var item= await _context.OCs.FindAsync(oc.ID);
                    item.Name = oc.Name;
                }
             
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public async Task<object> ListOCIDofUser(int ocid)
        {
            try
            {
                var item = await _context.OCs.FindAsync(ocid);
                var ocs = await GetListTree(item.ParentID, item.ID);

                var arrocs = GetAllDescendants(ocs).Select(x => x.key).ToArray();
                return arrocs;
            }
            catch (Exception)
            {

                return new int[] { };
            }
          
        }
        public async Task<object> CreateSubOC(CreateOCViewModel oc)
        {
           
                var item = _mapper.Map<Data.Models.OC>(oc);

                //Level cha tang len 1 va gan parentid cho subtask
                var taskParent = _context.OCs.Find(item.ParentID);
                item.Level = taskParent.Level + 1;
                item.ParentID = oc.ParentID;
                await _context.OCs.AddAsync(item);
            
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
        private IEnumerable<TreeView> GetAllDescendants(IEnumerable<TreeView> rootNodes)
        {
            var descendants = rootNodes.SelectMany(x => GetAllDescendants(x.children));
            return rootNodes.Concat(descendants);
        }
        public async Task<bool> Delete(int ID)
        {
            var item = await _context.OCs.FindAsync(ID);
            //var OCS = await _ocService.GetListTreeOC(item.ParentID, item.ID);
            //var arrOCs = GetAllDescendants(OCS).Select(x => x.ID).ToArray();
            //var items = await _context.Tasks.Where(x => arrOCs.Contains(x.ID)).ToListAsync();
            _context.OCs.Remove(item);
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

        public async Task<object> Test()
        {
            var source = await _context.OCs.Select(x =>
                new 
                {
                    Key = x.ParentID,
                    Message  = x
                }).ToListAsync();
            var query = source.Select(e => e.Key).Distinct()
            .Select(key => new Test
            {
                Key = key,
                Name = source.Where(x => x.Key == key).Select(x=>x.Message.Name).ToList()
            });
            return query;
        }
    }
}
