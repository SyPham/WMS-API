using Data.ViewModel.Line;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Service.Interface
{
   public interface ILineService
    {
        Task SendMessage(MessageParams msg);
        Task SendWithSticker(MessageParams msg); 
        Task SendWithPicture(MessageParams msg);
        Task<string> FetchToken(string code);
    }
}
