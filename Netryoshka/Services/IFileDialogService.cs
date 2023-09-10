using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netryoshka.Services
{
    public interface IFileDialogService
    {
        Task ShowSaveDialogAndExecuteAction(string filter, string defaultExt, string title, string fileName, Func<string, Task> saveAction);
        Task ShowOpenDialogAndExecuteAction(string filter, string defaultExt, string title, Func<string, Task> loadAction);
    }
}
