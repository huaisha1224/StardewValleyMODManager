using System;
using System.Collections.Generic;

namespace Stardrop.Models.Data
{
    public class UserModChineseName
    {
        public string UniqueId { get; set; }
        public string Name { get; set; }
        public string CustomChineseName { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        public UserModChineseName()
        {
            CreatedDate = DateTime.Now;
            ModifiedDate = DateTime.Now;
        }

        public UserModChineseName(string uniqueId, string name, string customChineseName) : this()
        {
            UniqueId = uniqueId;
            Name = name;
            CustomChineseName = customChineseName;
        }
    }
}