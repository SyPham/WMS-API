using Data.ViewModel.Project;
using Data.ViewModel.Task;
using Data.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Service.Interface
{
    public interface ITaskService
    {
        System.Threading.Tasks.Task TaskListIsLate();
        Task<Tuple<bool, string, object>> CreateTask(CreateTaskViewModel task);
        Task<object> CreateSubTask(CreateTaskViewModel task);
        Task<object> UpdateTask(UpdateTaskViewModel task);
        Task<object> Delete(int id, int userid);
        Task<Tuple<bool, bool, string>> Done(int id, int userid);
        Task<object> From(int userid);
        Task<List<ProjectViewModel>> GetListProject();
        Task<object> Follow(int userid, int taskid);
        Task<object> Undo(int id);
        Task<object> Unsubscribe(int id, int userid);
        Task<object> GetDeputies();
        Task<object> GetListUser(int userid, int projectid);
        Task<object> UpdateDueDateTime();
        Task<string> GetCodeLineAsync(string code, string state);
        Task<List<HierarchyNode<TreeViewTask>>> Todolist(string sort = "", string priority = "", int userid = 0, string startDate = "", string endDate = "", string weekdays = "", string monthly = "", string quarterly = "");
        Task<List<HierarchyNode<TreeViewTask>>> TodolistSortBy(string beAssigned, string assigned, int userid);
        Task<List<HierarchyNode<TreeViewTask>>> Routine(string sort, string priority, int userid, int ocid);
        Task<List<HierarchyNode<TreeViewTask>>> Abnormal(int ocid, string priority, int userid, string startDate, string endDate, string weekdays);
        Task<List<HierarchyNode<TreeViewTask>>> ProjectDetail(string sort = "", string priority = "", int userid = 0, int? projectid = null);
        Task<List<HierarchyNode<TreeViewTask>>> Follow(string sort = "", string priority = "", int userid = 0);
        Task<List<HierarchyNode<TreeViewTaskForHistory>>> History(int userid, string start, string end);
        Task<List<HierarchyNode<TreeViewTask>>> TodolistSortBy(Data.Enum.Status status, int userid);
        Task<List<HierarchyNode<TreeViewTaskForHistory>>> HistoryFilterByDueDateTime(int userid, string start, string end);
    }
}
