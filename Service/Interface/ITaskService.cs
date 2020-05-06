using Data.ViewModel.Project;
using Data.ViewModel.Task;
using Service.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Service.Interface
{
    public interface ITaskService
    {
        System.Threading.Tasks.Task TaskListIsLate(int userid);
        Task<bool> CheckTask();
        Task<bool> CreateCheckTask();
        Task<HashSet<TreeViewTask>> GetListTree(string beAssigned = "", string assigned = "", int userid = 0);
        Task<HashSet<TreeViewTask>> GetListTree();
        Task<Tuple<bool, string, object>> CreateTask(CreateTaskViewModel task);
        Task<object> CreateSubTask(CreateTaskViewModel task);
        Task<object> Delete(int id, int userid);
        Task<object> From(int userid);
        Task<Tuple<bool, bool, string>> Done(int id, int userid);
        Task<object> GetListUser(int userid, int projectid);
        Task<List<ProjectViewModel>> GetListProject();
        Task<object> UpdateTask(UpdateTaskViewModel task);
        Task<object> Follow(int userid, int taskid);
        Task<object> Undo(int id);
        Task<object> GetDeputies();
        string CastPriority(string value);
        void HieararchyWalk(HashSet<TreeViewTask> hierarchy);
        HashSet<TreeViewTask> GetChildren(HashSet<TreeViewTask> tasks, int parentid);
        Task<IEnumerable<TreeViewTask>> GetListTree(int parentID, int id);
        IEnumerable<TreeViewTask> GetAllTaskDescendants(IEnumerable<TreeViewTask> rootNodes);
        Task<object> Unsubscribe(int id, int userid);

        Task<HashSet<HierarchyNode<TreeViewTask>>> Todolist(string sort = "", string priority = "", int userid = 0, string startDate = "", string endDate = "", string weekdays = "", string monthly = "", string quarterly = "");
        Task<List<HierarchyNode<TreeViewTask>>> TodolistSortBy(string beAssigned, string assigned, int userid);
        Task<List<HierarchyNode<TreeViewTask>>> Routine(string sort, string priority, int userid, int ocid);
        Task<List<HierarchyNode<TreeViewTask>>> Abnormal(int ocid, string priority, int userid, string startDate, string endDate, string weekdays);
        Task<List<HierarchyNode<TreeViewTask>>> ProjectDetail(string sort = "", string priority = "", int userid = 0, int projectid = 0);
        Task<List<HierarchyNode<TreeViewTask>>> Follow(string sort = "", string priority = "", int userid = 0);
        Task<List<HierarchyNode<TreeViewTask>>> History(int userid, string start, string end);
        Task<List<HierarchyNode<TreeViewTask>>> TodolistSortBy(Data.Enum.Status status, int userid);
    }
}
