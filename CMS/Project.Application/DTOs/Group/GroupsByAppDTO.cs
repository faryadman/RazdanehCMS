using Project.Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Application.DTOs.Group
{
    public class GroupsByAppDTO : BaseEntity
    {
        public string Title { get; set; }
        public bool DoTheyHaveRelation { get; set; }
        public bool IsAd { get; set; }
    }
}
