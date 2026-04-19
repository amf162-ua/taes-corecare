using CommunityToolkit.Mvvm.ComponentModel;
using CoreCare.Models;
using CoreCare.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace CoreCare.ViewModels
{
    public class ProcessViewModel : ObservableObject
    {
        private readonly ProcessService _service = new();

        // Esta es la lista que se mostrará en el DataGrid/ListView
        public ObservableCollection<ProcessItem> Processes { get; set; } = new();

        public void Refresh()
        {
            var data = _service.GetActiveProcesses();
            Processes.Clear();
            foreach (var item in data) Processes.Add(item);
        }
    }
}
