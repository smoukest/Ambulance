using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ambulance.Models
{
    public class DataGroup : ReactiveObject
    {
        public DataGroup(int id, string name, int count, bool check1, bool? check2 = null, Type type = default)
        {
            Id = id;
            Name = name;
            Count = count;
            Check1 = check1;
            Check2 = check2;
            Type = type;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public Type Type { get; set; }
        public int Count { get; set; }
        public bool Check1 { get; set; }
        public bool? Check2 { get; set; }
        [Reactive]
        public bool? IsSelected { get; set; }
    }

    public enum Type
    {
        State,
        Reorder,
        Error
    }
}
