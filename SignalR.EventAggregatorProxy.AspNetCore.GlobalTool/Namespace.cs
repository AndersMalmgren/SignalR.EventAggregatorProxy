using System.Collections.Generic;
using System.Linq;

namespace SignalR.EventAggregatorProxy.AspNetCore.GlobalTool
{
    public class Namespace
    {
        public Namespace Parent { get; set; }
        public string Name { get; private set; }
        public Dictionary<string, Namespace> Children { get; private set; }

        public Namespace() : this(null, "window") { }

        public Namespace(Namespace parent, string name)
        {
            Parent = parent;
            Name = name;
            Children = new Dictionary<string, Namespace>();
        }

        public void Add(IEnumerable<string> childTree)
        {
            var name = childTree.FirstOrDefault();
            if (name != null)
            {
                var child = GetChild(name);
                child.Add(childTree.Skip(1));
            }
        }

        public IEnumerable<string> Render()
        {
            var parts = new List<string>();

            foreach (var child in Children.Values)
            {
                var closureSafe = string.Format("{0} = ({0} || {{}});", child.GetFullName());

                parts.Add(closureSafe);
                parts.AddRange(child.Render());
            }

            return parts;
        }

        private Namespace GetChild(string name)
        {
            if (!Children.ContainsKey(name))
            {
                Children[name] = new Namespace(this, name);
            }
            return Children[name];
        }



        private string GetFullName()
        {
            string name = null;
            if (Parent != null)
            {
                name = Parent.GetFullName();
            }
            if (name != null)
                return string.Format("{0}.{1}", name, Name);


            return Name;
        }
    }
}
