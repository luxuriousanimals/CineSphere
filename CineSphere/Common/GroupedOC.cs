using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CineSphere.Common
{
    public class GroupedOC<T>:ObservableCollection<T>
    {
        /// <summary>
        /// The Group Title
        /// </summary>
        public string Title
        {
            get;
            set;
        }

        /// <summary>
        /// Constructor ensure that a Group Title is included
        /// </summary>
        /// <param name="name">string to be used as the Group Title</param>
        public GroupedOC(string name)
        {
            this.Title = name;
        }

        /// <summary>
        /// Returns true if the group has a count more than zero
        /// </summary>
        public bool HasItems
        {
            get
            {
                return (Count != 0);
            }
            private set
            {
            }
        }
    }
}